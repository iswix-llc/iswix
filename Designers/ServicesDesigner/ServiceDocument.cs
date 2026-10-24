using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Authoring;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace ServicesDesigner
{
    // A service: its ServiceInstall and the ServiceControl in the same component, if there is one yet.
    public class ServiceItem
    {
        public Wxs.ServiceInstall Install { get; set; }
        public Wxs.ServiceControl Control { get; set; }
    }

    public class ServiceCandidate
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string DestinationFilePath { get; set; }
        public XElement FileElement { get; set; }
    }

    // The services in a fragment, read and written through the typed model.
    internal static class ServiceDocument
    {
        /// <summary>Gets whether the document is one this designer edits: a WiX fragment.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            return root?.Name == Wxs.Wix.ElementName && root.Elements().FirstOrDefault()?.Name == Wxs.Fragment.ElementName;
        }

        public static List<ServiceItem> Load(XDocument document) =>
            document.Descendants(Wxs.ServiceInstall.ElementName)
                .Select(e => new ServiceItem
                {
                    Install = (Wxs.ServiceInstall)WixElement.Wrap(e),
                    Control = e.Parent?.Descendants(Wxs.ServiceControl.ElementName).Select(c => (Wxs.ServiceControl)WixElement.Wrap(c)).FirstOrDefault(),
                })
                .ToList();

        // Executables that are their component's key path.
        public static List<ServiceCandidate> GetServiceCandidates(XDocument document)
        {
            List<ServiceCandidate> candidates = new List<ServiceCandidate>();
            foreach (var file in document.Descendants(Wxs.File.ElementName))
            {
                string source = (string)file.Attribute("Source") ?? string.Empty;
                if (IsKeyPath(file) && source.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    ServiceCandidate sc = new ServiceCandidate();
                    sc.Id = (string)file.Attribute("Id") ?? string.Empty;
                    sc.DestinationFilePath = WixFiles.DestinationFilePath(file);
                    sc.FileElement = file;
                    sc.FileName = Path.GetFileNameWithoutExtension(source.Split('\\').Last());
                    candidates.Add(sc);
                }
            }
            return candidates;
        }

        // Added through the model to the file's component, with IsWiX's defaults.
        public static ServiceItem Create(XDocument document, string name, string fileId, XElement fileElement = null)
        {
            fileElement ??= document.Descendants(Wxs.File.ElementName).First(f => (string)f.Attribute("Id") == fileId);
            var component = (Wxs.Component)WixElement.Wrap(fileElement.Parent);

            var install = component.Add(new Wxs.ServiceInstall
            {
                Id = InstallId(name),
                Name = name,
                DisplayName = $"{name} Service",
                Description = $"{name} Service",
                ErrorControl = Wxs.ServiceInstallErrorControl.Normal,
                Start = Wxs.ServiceInstallStart.Auto,
                Type = Wxs.ServiceInstallType.OwnProcess,
            });
            var control = component.Add(new Wxs.ServiceControl
            {
                Id = ControlId(name),
                Name = name,
                Start = Wxs.InstallUninstallType.Install,
                Stop = Wxs.InstallUninstallType.Both,
                Remove = Wxs.InstallUninstallType.Both,
                Wait = true,
            });
            return new ServiceItem { Install = install, Control = control };
        }

        // A component without a ServiceControl gets one when a ServiceControl field is first set.
        public static Wxs.ServiceControl EnsureControl(ServiceItem service)
        {
            if (service.Control == null)
            {
                var component = (Wxs.Component)WixElement.Wrap(service.Install.Xml.Parent);
                service.Control = component.Add(new Wxs.ServiceControl { Id = ControlId(service.Install.Name), Name = service.Install.Name });
            }

            return service.Control;
        }

        // Renames both elements. Their Ids follow the name unless they were given other Ids by hand.
        public static void SetName(ServiceItem service, string name)
        {
            var install = service.Install;
            if (install.Id == InstallId(install.Name))
            {
                install.Id = InstallId(name);
            }

            install.Name = name;

            var control = service.Control;
            if (control != null)
            {
                if (control.Id == ControlId(control.Name))
                {
                    control.Id = ControlId(name);
                }

                control.Name = name;
            }
        }

        public static void Delete(ServiceItem service)
        {
            service.Control?.RemoveFromParent();
            service.Install.RemoveFromParent();
        }

        // Where the service's executable (its component's key path) is installed.
        public static string DestinationFilePath(ServiceItem service)
        {
            var file = service.Install.Xml.Parent?.Elements(Wxs.File.ElementName).FirstOrDefault(IsKeyPath);
            return file == null ? string.Empty : WixFiles.DestinationFilePath(file);
        }

        static bool IsKeyPath(XElement file) =>
            ((Wxs.File)WixElement.Wrap(file)).KeyPath is { IsLiteral: true } keyPath && keyPath.Value;

        static string InstallId(string name) => "si" + WixFiles.IdHash(name ?? string.Empty);

        static string ControlId(string name) => "sc" + WixFiles.IdHash(name ?? string.Empty);
    }
}
