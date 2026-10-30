// Copyright (c) ISWIX LLC. Licensed under the MS-PL.

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using NuGet.SolutionRestoreManager;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace IsWiX.ProjectSystem
{
    /// <summary>
    /// Tells NuGet about an IsWiX project (.iswixproj), as the .NET project system does for SDK-style .NET projects:
    /// whenever its NuGet properties or package items change, it nominates the project to NuGet's solution restore
    /// service with them. NuGet then restores it, and lists it in Manage NuGet Packages. A project that sets
    /// IsWiXNuGetNomination to false isn't nominated; IsWiX.DesignTime.targets then restores it with MSBuild instead.
    /// </summary>
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectDynamicLoadComponent))]
    [AppliesTo(IsWiXCapabilities.IsWiXProject)]
    internal sealed class IsWiXNuGetNominator : IProjectDynamicLoadComponent
    {
        const string PropertiesRule = "IsWiXNuGetRestore";

        // The item types NuGet restores from, each read through the rule of the same name.
        static readonly string[] ItemRules = { "PackageReference", "PackageVersion", "PackageDownload" };

        readonly UnconfiguredProject _project;
        readonly IVsSolutionRestoreService5 _restoreService;
        IDisposable? _link;
        ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>? _block;
        CancellationTokenSource? _unloading;
        string? _lastNomination;

        [ImportingConstructor]
        public IsWiXNuGetNominator(UnconfiguredProject project, IVsSolutionRestoreService5 restoreService)
        {
            _project = project;
            _restoreService = restoreService;
        }

        public Task LoadAsync()
        {
            var subscription = _project.Services.ActiveConfiguredProjectSubscription;
            if (subscription == null) return Task.CompletedTask;

            _unloading = new CancellationTokenSource();
            _block = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(NominateAsync);
            _link = subscription.ProjectRuleSource.SourceBlock.LinkTo(
                _block,
                new StandardRuleDataflowLinkOptions
                {
                    RuleNames = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, ItemRules.Concat(new[] { PropertiesRule }).ToArray()),
                    PropagateCompletion = true,
                });
            return Task.CompletedTask;
        }

        public Task UnloadAsync()
        {
            _unloading?.Cancel();
            _link?.Dispose();
            _block?.Complete();
            _link = null;
            _block = null;
            return Task.CompletedTask;
        }

        async Task NominateAsync(IProjectVersionedValue<IProjectSubscriptionUpdate> update)
        {
            // An exception would fault the block and end the subscription, so none leaves here.
            try
            {
                var state = update.Value.CurrentState;
                if (!state.TryGetValue(PropertiesRule, out var properties)) return;
                var values = properties.Properties;
                if (string.Equals(Value(values, "IsWiXNuGetNomination"), "false", StringComparison.OrdinalIgnoreCase)) return;

                var projectFolder = Path.GetDirectoryName(_project.FullPath) ?? string.Empty;
                var extensionsPath = Value(values, "MSBuildProjectExtensionsPath");
                if (extensionsPath.Length == 0) return;
                extensionsPath = Path.GetFullPath(Path.Combine(projectFolder, extensionsPath));

                var framework = Value(values, "TargetFramework");
                var frameworks = Value(values, "TargetFrameworks");
                var original = frameworks.Length > 0 ? frameworks : framework;
                if (original.Length == 0) return;

                var items = new Dictionary<string, IReadOnlyList<IVsReferenceItem2>>(StringComparer.OrdinalIgnoreCase);
                foreach (var rule in ItemRules)
                {
                    items[rule] = state.TryGetValue(rule, out var snapshot)
                        ? snapshot.Items.Select(i => (IVsReferenceItem2)new ReferenceItem(i.Key, NonEmpty(i.Value))).ToList()
                        : new List<IVsReferenceItem2>();
                }

                var info = new RestoreInfo(
                    extensionsPath,
                    original,
                    new IVsTargetFrameworkInfo4[] { new TargetFrameworkInfo(items, NonEmpty(values)) });

                // Nominate only when something NuGet sees has changed: each nomination starts a restore.
                var key = Describe(info);
                if (string.Equals(key, _lastNomination, StringComparison.Ordinal)) return;
                _lastNomination = key;

                await _restoreService.NominateProjectAsync(_project.FullPath, info, _unloading?.Token ?? CancellationToken.None).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException))
            {
                _lastNomination = null;
                System.Diagnostics.Debug.WriteLine($"IsWiX: NuGet nomination of {_project.FullPath} failed: {ex}");
            }
        }

        static string Value(IImmutableDictionary<string, string> values, string name) =>
            values.TryGetValue(name, out var value) && value != null ? value.Trim() : string.Empty;

        static IReadOnlyDictionary<string, string> NonEmpty(IImmutableDictionary<string, string> values) =>
            values.Where(v => !string.IsNullOrEmpty(v.Value)).ToDictionary(v => v.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

        static string Describe(RestoreInfo info)
        {
            var framework = info.TargetFrameworks[0];
            var parts = new List<string> { info.MSBuildProjectExtensionsPath, info.OriginalTargetFrameworks ?? string.Empty };
            parts.AddRange(framework.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase).Select(p => p.Key + "=" + p.Value));
            foreach (var group in framework.Items.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                foreach (var item in group.Value.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                    parts.Add(group.Key + ":" + item.Name + "|" + string.Join("|", item.Metadata.OrderBy(m => m.Key, StringComparer.OrdinalIgnoreCase).Select(m => m.Key + "=" + m.Value)));
            }

            return string.Join("\n", parts);
        }

        sealed class RestoreInfo : IVsProjectRestoreInfo3
        {
            public RestoreInfo(string extensionsPath, string originalTargetFrameworks, IReadOnlyList<IVsTargetFrameworkInfo4> targetFrameworks)
            {
                MSBuildProjectExtensionsPath = extensionsPath;
                OriginalTargetFrameworks = originalTargetFrameworks;
                TargetFrameworks = targetFrameworks;
            }

            public string MSBuildProjectExtensionsPath { get; }
            public IReadOnlyList<IVsTargetFrameworkInfo4> TargetFrameworks { get; }
            public IReadOnlyList<IVsReferenceItem2> ToolReferences { get; } = new IVsReferenceItem2[0];
            public string? OriginalTargetFrameworks { get; }
        }

        sealed class TargetFrameworkInfo : IVsTargetFrameworkInfo4
        {
            public TargetFrameworkInfo(IReadOnlyDictionary<string, IReadOnlyList<IVsReferenceItem2>> items, IReadOnlyDictionary<string, string> properties)
            {
                Items = items;
                Properties = properties;
            }

            public IReadOnlyDictionary<string, IReadOnlyList<IVsReferenceItem2>> Items { get; }
            public IReadOnlyDictionary<string, string> Properties { get; }
        }

        sealed class ReferenceItem : IVsReferenceItem2
        {
            public ReferenceItem(string name, IReadOnlyDictionary<string, string> metadata)
            {
                Name = name;
                Metadata = metadata;
            }

            public string Name { get; }
            public IReadOnlyDictionary<string, string> Metadata { get; }
        }
    }
}
