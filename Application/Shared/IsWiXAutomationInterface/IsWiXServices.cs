using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXServices : List<IsWiXService>
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        public IsWiXServices()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            Load();
        }

        public void Load()
        {
            Clear();

            try
            {
                foreach (var serviceInstallElement in _documentManager.Document.GetProductModuleOrFragmentElement().Descendants(ns + "ServiceInstall"))
                {
                    IsWiXServiceInstall isWiXServiceInstall = new IsWiXServiceInstall(_documentManager.Document, serviceInstallElement);

                    //Autoinstantiate a ServiceControl element when one does not exist
                    if (serviceInstallElement.Parent.Descendants(ns + "ServiceControl").Count().Equals(0))
                    {
                        XElement serviceControlElement = new XElement(ns + "ServiceControl");
                        serviceControlElement.SetAttributeValue("Id", isWiXServiceInstall.Id);
                        serviceControlElement.SetAttributeValue("Name", isWiXServiceInstall.Name);
                         serviceInstallElement.AddAfterSelf(serviceControlElement);
                    }

                    IsWiXServiceControl isWiXServiceControl = new IsWiXServiceControl(_documentManager.Document, serviceInstallElement.Parent.Descendants(ns + "ServiceControl").First());
                    IsWiXService isWiXService = new IsWiXService() { ServiceInstall = isWiXServiceInstall, ServiceControl = isWiXServiceControl };
                    Add(isWiXService);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Property elements.\r\n" + ex.Message);
            }
        }

        public List<ServiceCandidate> GetServiceCandidates()
        {
            List<ServiceCandidate> candidates = new List<ServiceCandidate>();


            var files = from f in _documentManager.Document.Descendants(ns + "File")
                        select f;
            foreach (var file in files)
            {
                if(file.GetOptionalYesNoAttribute("KeyPath", false).Equals(true))
                {
                    if(file.Attribute("Source").Value.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ServiceCandidate sc = new ServiceCandidate();
                        sc.Id = file.Attribute("Id").Value;
                        sc.DestinationFilePath = file.GetDestinationFilePath();
                        sc.FileName = Path.GetFileNameWithoutExtension(file.Attribute("Source").Value.Split('\\').Last());
                        candidates.Add(sc); 
                    }
                }

            }
            return candidates;
        }



        public IsWiXService Create(string name, string fileId)
        {

            XElement serviceInstallElement = new XElement(ns + "ServiceInstall");
            serviceInstallElement.SetAttributeValue("Id", "si" + IsWiXHelpers.GetMd5Hash(name));
            serviceInstallElement.SetAttributeValue("Name", name);
            serviceInstallElement.SetAttributeValue("DisplayName", $"{name} Service");
            serviceInstallElement.SetAttributeValue("Description", $"{name} Service");
            serviceInstallElement.SetAttributeValue("ErrorControl", "normal");
            serviceInstallElement.SetAttributeValue("Start", "auto");
            serviceInstallElement.SetAttributeValue("Type", "ownProcess");

            var elements = from a in _documentManager.Document.Descendants(ns + "File")
                      where a.Attribute("Id").Value == fileId
                      select a;

            XElement fileElement = elements.First();

            fileElement.AddAfterSelf(serviceInstallElement);

            XElement serviceControlElement = new XElement(ns + "ServiceControl");
            serviceControlElement.SetAttributeValue("Id", "sc"+ IsWiXHelpers.GetMd5Hash(name));
            serviceControlElement.SetAttributeValue("Name", name);
            serviceControlElement.SetAttributeValue("Start", "install");
            serviceControlElement.SetAttributeValue("Stop", "both");
            serviceControlElement.SetAttributeValue("Remove", "both");
            serviceControlElement.SetAttributeValue("Wait", "yes");

            serviceInstallElement.AddAfterSelf(serviceControlElement);


            IsWiXServiceInstall isWiXServiceInstall = new IsWiXServiceInstall(_documentManager.Document, serviceInstallElement);
            IsWiXServiceControl isWiXServiceControl = new IsWiXServiceControl(_documentManager.Document, serviceControlElement);
            IsWiXService isWiXService = new IsWiXService() { ServiceInstall = isWiXServiceInstall, ServiceControl = isWiXServiceControl };
            this.Add(isWiXService);
            return isWiXService;
        }

    }

    public class IsWiXService
    {
        public IsWiXServiceInstall ServiceInstall { get; set; }
        public IsWiXServiceControl ServiceControl { get; set; }
    }

    
    public class ServiceCandidate
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string DestinationFilePath { get; set; }
    }

    public class IsWiXServiceInstall
    {
        XNamespace ns;
        XElement _serviceElement;
        XDocument _document;

        public IsWiXServiceInstall(XDocument document, XElement serviceElement)
        {
            _document = document;
            ns = _document.GetWiXNameSpace();
            _serviceElement = serviceElement;
        }

        public string Id
        {
            get
            {
                return _serviceElement.Attribute("Id").Value;
            }
            private set
            {
                _serviceElement.Attribute("Id").Value = value;
            }
        }

        public string KeyPath
        {
            get
            {
                var files = from f in _serviceElement.Parent.Elements(ns + "File")
                            where f.Attribute("KeyPath").Value == "yes"
                            select f;
                string file = files.First().Attribute("Source").Value;
                string fileName = new FileInfo(file).Name;
                return fileName;
            }
        }

        public string DestinationFilePath
        {
            get
            {
                var files = from f in _serviceElement.Parent.Elements(ns + "File")
                            where f.Attribute("KeyPath").Value == "yes"
                            select f;
                return files.First().GetDestinationFilePath();
            }
        }

        public XElement ParentComponentXML
        {
            get
            {
                return _serviceElement.Parent;
            }
        }
        
        public string Name
        {
            get
            {
                return _serviceElement.Attribute("Name").Value;
            }
            set
            {
                _serviceElement.Attribute("Name").Value = value;
                this.Id = "si" + IsWiXHelpers.GetMd5Hash(value);
            }
        }

        public string DisplayName
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("DisplayName");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("DisplayName", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("DisplayName", null);
                }
            }
        }

        public string Description
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("Description");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("Description", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("Description", null);
                }
            }
        }

        public string Account
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("Account");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("Account", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("Account", null);
                }
            }
        }

        public string Arguments
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("Arguments");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("Arguments", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("Arguments", null);
                }
            }
        }

        public string Password
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("Password");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("Password", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("Password", null);
                }
            }
        }

        public string LoadOrderGroup
        {
            get
            {
                return _serviceElement.GetOptionalAttribute("LoadOrderGroup");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _serviceElement.SetAttributeValue("LoadOrderGroup", value);
                }
                else
                {
                    _serviceElement.SetAttributeValue("LoadOrderGroup", null);
                }
            }
        }

        public YesNo? Interactive
        {
            get
            {
                YesNo? yesno = null;
                string interactive = _serviceElement.GetOptionalAttribute("Interactive");
                if (interactive != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), interactive, true);
                }
                return yesno;
            }
            set
            {
                _serviceElement.SetAttributeValue("Interactive", value);
            }
        }

        public YesNo? EraseDescription
        {
            get
            {
                YesNo? yesno = null;
                string eraseDescription = _serviceElement.GetOptionalAttribute("EraseDescription");
                if (eraseDescription != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), eraseDescription, true);
                }
                return yesno;
            }
            set
            {
                _serviceElement.SetAttributeValue("EraseDescription", value);
            }
        }

        public ErrorControl ErrorControl
        {
            get
            {
                string errorControl = _serviceElement.Attribute("ErrorControl").Value;
                return (ErrorControl)Enum.Parse(typeof(ErrorControl), errorControl, true);
            }
            set
            {
                _serviceElement.SetAttributeValue("ErrorControl", value);
            }
        }

        public Start Start
        {
            get
            {
                string start = _serviceElement.Attribute("Start").Value;
                return (Start)Enum.Parse(typeof(Start), start, true);
            }
            set
            {
                _serviceElement.SetAttributeValue("Start", value);
            }
        }

        public Type Type
        {
            get
            {
                string type = _serviceElement.Attribute("Type").Value;
                return (Type)Enum.Parse(typeof(Type), type, true);
            }
            set
            {
                _serviceElement.SetAttributeValue("Type", value);
            }
        }

        public YesNo? Vital
        {
            get
            {
                YesNo? yesno = null;
                string vital = _serviceElement.GetOptionalAttribute("Vital");
                if (vital != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), vital, true);
                }
                return yesno;
            }
            set
            {
                _serviceElement.SetAttributeValue("Vital", value);
            }
        }

        public override string ToString()
        {
            XElement strippedElement = stripNS(_serviceElement);
            strippedElement.Value = string.Empty;
            return strippedElement.ToString(SaveOptions.DisableFormatting);
        }

        static XElement stripNS(XElement root)
        {
            XElement res = new XElement(
                root.Name.LocalName,
                root.HasElements ?
                    root.Elements().Select(el => stripNS(el)) :
                    (object)root.Value
            );

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

        public void Delete()
        {
            _serviceElement.Remove();
            //var foo = from a in _document.Descendants(ns + "ServiceInstall")
            //          where a.Attribute("Id").Value == this.Id
            //          select a;
            //foo.First().Remove();
        }
    }
    public class IsWiXServiceControl
    {
        XNamespace ns;
        XElement _serviceElement;
        XDocument _document;

        public IsWiXServiceControl(XDocument document, XElement serviceElement)
        {
            ns = _document.GetWiXNameSpace();
            _document = document;
            _serviceElement = serviceElement;
        }

        public string Id
        {
            get
            {
                return _serviceElement.Attribute("Id").Value;
            }
            private set
            {
                _serviceElement.Attribute("Id").Value = value;
            }
        }

        public string Name
        {
            get
            {
                return _serviceElement.Attribute("Name").Value;
            }
            set
            {
                _serviceElement.Attribute("Name").Value = value;
                this.Id = "sc" + IsWiXHelpers.GetMd5Hash(value);
            }
        }

        public InstallUninstall? Remove
        {
            get
            {
                InstallUninstall? installUninstall = null;
                string remove = _serviceElement.GetOptionalAttribute("Remove");
                if (remove != string.Empty)
                {
                    installUninstall = (InstallUninstall)Enum.Parse(typeof(InstallUninstall), remove, true);
                }
                return installUninstall;
            }
            set
            {
                _serviceElement.SetAttributeValue("Remove", value);
            }
        }

        public InstallUninstall? Start
        {
            get
            {
                InstallUninstall? installUninstall = null;
                string start = _serviceElement.GetOptionalAttribute("Start");
                if (start != string.Empty)
                {
                    installUninstall = (InstallUninstall)Enum.Parse(typeof(InstallUninstall), start, true);
                }
                return installUninstall;
            }
            set
            {
                _serviceElement.SetAttributeValue("Start", value);
            }
        }

        public InstallUninstall? Stop
        {
            get
            {
                InstallUninstall? installUninstall = null;
                string stop = _serviceElement.GetOptionalAttribute("Stop");
                if (stop != string.Empty)
                {
                    installUninstall = (InstallUninstall)Enum.Parse(typeof(InstallUninstall), stop, true);
                }
                return installUninstall;
            }
            set
            {
                _serviceElement.SetAttributeValue("Stop", value);
            }
        }

        public YesNo? Wait
        {
            get
            {
                YesNo? yesNo = null;
                string wait = _serviceElement.GetOptionalAttribute("Wait");
                if (wait != string.Empty)
                {
                    yesNo = (YesNo)Enum.Parse(typeof(YesNo), wait, true);
                }
                return yesNo;
            }
            set
            {
                _serviceElement.SetAttributeValue("Wait", value);
            }
        }

        public override string ToString()
        {
            XElement strippedElement = stripNS(_serviceElement);
            strippedElement.Value = string.Empty;
            return strippedElement.ToString(SaveOptions.DisableFormatting);
        }

        static XElement stripNS(XElement root)
        {
            XElement res = new XElement(
                root.Name.LocalName,
                root.HasElements ?
                    root.Elements().Select(el => stripNS(el)) :
                    (object)root.Value
            );

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

        public void Delete()
        {
            _serviceElement.Remove();
            //try
            //{
            //    var foo = from a in _document.Descendants(ns + "ServiceControl")
            //              where a.Attribute("Id").Value == this.Id
            //              select a;
            //    foo.First().Remove();
            //}
            //catch(Exception)
            //{
            //    // no element to remove
            //}
        }

    }
    
    public enum ErrorControl
    {
        ignore,
        normal,
        critical
    }

    public enum Start
    {
        auto,
        demand,
        disabled,
        boot,
        system
    }

    public enum Type
    {
        ownProcess,
        shareProcess,
        kernelDriver,
        systemDrive
    }

    public enum InstallUninstall
    {
        install,
        uninstall,
        both
    }

}
