using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Documents;

namespace IsWiXExtension
{
    /// <summary>
    /// Command1 handler.
    /// </summary>
    [VisualStudioContribution]
    internal class CommandLaunchIsWiX : Command
    {
        private readonly TraceSource logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLaunchIsWiX"/> class.
        /// </summary>
        /// <param name="traceSource">Trace source instance to utilize.</param>
        public CommandLaunchIsWiX(TraceSource traceSource)
        {
            // This optional TraceSource can be used for logging in the command. You can use dependency injection to access
            // other services here as well.
            this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        }

        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new("Launch IsWiX")
        {
            // Use this object initializer to set optional parameters for the command. The required parameter,
            // displayName, is set above. DisplayName is localized and references an entry in .vsextension\string-resources.json.
            Icon = new(ImageMoniker.Custom("MyImage"), IconSettings.IconAndText),
            Placements = [CommandPlacement.KnownPlacements.ToolsMenu]             
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            // Use InitializeAsync for any one-time setup or initialization.
            return base.InitializeAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            string documentPath = string.Empty;
            string isWiXPath = string.Empty;
            string errorMessage = string.Empty;

            try
            {
                documentPath = $"\"{context.GetSelectedPathAsync(cancellationToken).Result.LocalPath}\"";
            }
            catch (Exception)
            {
            }

            try
            {
                using (RegistryKey local64Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey isWiXKey = local64Key.OpenSubKey(@"SOFTWARE\ISWIXLLC\IsWiX", false))
                    {
                        isWiXPath = isWiXKey.GetValue("IsWiXFilePath", string.Empty).ToString();
                    }
                }
                if (!File.Exists(isWiXPath))
                {
                    errorMessage = "IsWiX.exe not found";
                }
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.UseShellExecute = true;
                processStartInfo.WorkingDirectory = new FileInfo(documentPath).Directory.FullName;
                processStartInfo.FileName = isWiXPath;
                processStartInfo.Arguments = documentPath;
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                errorMessage = "IsWiX installation path registry entry not found.";
            }

            if(!string.IsNullOrEmpty(errorMessage))
            {
                await this.Extensibility.Shell().ShowPromptAsync(errorMessage, PromptOptions.OK, cancellationToken);
            }
        }
    }
}
