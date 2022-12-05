using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXPackage
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        XElement _packageElement;

        public IsWiXPackage()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _packageElement = _documentManager.Document.GetSecondOrderRoot().Element(ns + "Package");
        }
        
        public String Id 
        { 
            get{

                string id;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    id = _packageElement.GetOptionalAttribute("Id");
                }
                else
                {
                    id = _packageElement.Attribute("Id").Value;
                    if (id == "00000000-0000-0000-0000-000000000000")
                    {
                        id = Guid.NewGuid().ToString();
                        Id = id.ToString();
                    }
                }
                return id;
            }
            set
            {
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        value = null;
                    }
                }
                _packageElement.SetAttributeValue("Id", value);
            }
        }

        public Int32 InstallerVersion 
        {
            get
            {
                return Convert.ToInt32(_packageElement.GetOptionalAttribute("InstallerVersion"));
            }
            set
            {
                _packageElement.Attribute("InstallerVersion").Value = Convert.ToString(value);
            }
        }

        public string Manufacturer 
        {
            get
            {
                string manufacturer;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    manufacturer = _packageElement.GetOptionalAttribute("Manufacturer");
                }
                else
                {
                    manufacturer = _packageElement.Attribute("Manufacturer").Value;
                }
                return manufacturer;
            }
            set 
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Manufacturer", value);
            }
        }

        public YesNo? AdminImage
        {
            get
            {
                YesNo? yesno = null;
                string adminImage = _packageElement.GetOptionalAttribute("AdminImage");
                if (adminImage != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), adminImage, true);
                }
                return yesno;
            }
            set
            {
                _packageElement.SetAttributeValue("AdminImage", value);
            }
        }

        public string Comments 
        {
            get
            {
                return _packageElement.GetOptionalAttribute("Comments");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Comments", value);
            }
        }

        public YesNo? Compressed
        {
            get
            {
                YesNo? yesno = null;
                string compressed = _packageElement.GetOptionalAttribute("Compressed");
                if (compressed != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), compressed, true);
                }
                return yesno;
            }
            set
            {
                if ( value != null && _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module )
                {
                    throw new Exception("The commpressed attribute can not be set for a merge module.");
                }
                _packageElement.SetAttributeValue("Compressed", value);
            }
        }

        public string Description
        {
            get
            {
                return _packageElement.GetOptionalAttribute("Description");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Description", value);
            }
        }

        public InstallPrivileges? InstallPrivileges
        {
            get
            {
                InstallPrivileges? installprivileges = null;
                string installPrivileges = _packageElement.GetOptionalAttribute("InstallPrivileges");
                if (installPrivileges != string.Empty)
                {
                    installprivileges = (InstallPrivileges)Enum.Parse(typeof(InstallPrivileges), installPrivileges, true);
                }
                return installprivileges;
            }
            set
            {
                _packageElement.SetAttributeValue("InstallPrivileges", value);
            }
        }

        public InstallScope? InstallScope
        {
            get
            {
                InstallScope? installscope = null;
                string installScope = _packageElement.GetOptionalAttribute("InstallScope");
                if (installScope != string.Empty)
                {
                    installscope = (InstallScope)Enum.Parse(typeof(InstallScope), installScope, true);
                }
                return installscope;
            }
            set
            {
                _packageElement.SetAttributeValue("InstallScope", value);
            }
        }

        public string Keywords
        {
            get
            {
                return _packageElement.GetOptionalAttribute("Keywords");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Keywords", value);
            }
        }

        public string Languages
        {
            get
            {
                return _packageElement.GetOptionalAttribute("Languages");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Languages", value);
            }
        }

        public Platform? Platform
        {
            get
            {
                Platform? platform = null;
                string platForm = _packageElement.GetOptionalAttribute("Platform");
                if (platForm != string.Empty)
                {
                    platform = (Platform)Enum.Parse(typeof(Platform), platForm, true);
                }
                return platform;
            }
            set
            {
                _packageElement.SetAttributeValue("Platform", value);
            }
        }

        public YesNoDefault? ReadOnly
        {
            get
            {
                YesNoDefault? yesnodefault = null;
                string readOnly = _packageElement.GetOptionalAttribute("ReadOnly");
                if (readOnly != string.Empty)
                {
                    yesnodefault = (YesNoDefault)Enum.Parse(typeof(YesNoDefault), readOnly, true);
                }
                return yesnodefault;
            }
            set
            {
                _packageElement.SetAttributeValue("ReadOnly", value);
            }
        }

        public YesNo? ShortNames
        {
            get
            {
                YesNo? yesno = null;
                string shortNames = _packageElement.GetOptionalAttribute("ShortNames");
                if (shortNames != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), shortNames, true);
                }
                return yesno;
            }
            set
            {
                _packageElement.SetAttributeValue("ShortNames", value);
            }
        }

        public string SummaryCodepage 
        {
            get
            {
                return _packageElement.GetOptionalAttribute("SummaryCodepage");
            }
            set
            {
                if( string.IsNullOrEmpty( value ))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("SummaryCodepage", value);
            }
        }

    }

    public enum InstallPrivileges
    {
        elevated,
        limited
    } ;

    public enum YesNo
    {
        yes,
        no
    } ;

    public enum YesNoDefault
    {
        yes,
        no
    } ;

    public enum InstallScope
    {
        perMachine,
        perUser
    }

    public enum Platform
    {
        x86,
        ia64,
        x64
    }
}