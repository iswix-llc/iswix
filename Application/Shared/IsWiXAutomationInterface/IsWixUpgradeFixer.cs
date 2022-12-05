using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;
using FireworksFramework.Managers;
using System.Windows.Forms;

namespace IsWiXAutomationInterface
{
    public class IsWixUpgradeFixer 
    {

        public static void Fix()
        {
            DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
            XNamespace ns = _documentManager.Document.GetWiXNameSpace();
            if(_documentManager.Document.GetWiXVersion() == WiXVersion.v4 && _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {
                XElement module = _documentManager.Document.GetSecondOrderRoot();
                if (!module.Elements(ns + "Directory").Where(c=>c.Attribute("Id").Value == "TARGETDIR").Any())
                {
                    XElement targetDir = new XElement(ns + "Directory", new XAttribute("Id", "TARGETDIR"), new XAttribute("Name", "SourceDir"));
                    foreach (var element in module.Elements(ns + "Directory"))
                    {
                        targetDir.Add(element);
                        element.Remove();
                    }
                    foreach (var element in module.Elements(ns + "StandardDirectory"))
                    {
                        XElement newDirectory = new XElement(ns + "Directory", new XAttribute("Id", element.Attribute("Id").Value));
                        foreach (var subElement in element.Elements())
                        {
                            newDirectory.Add(subElement);
                        }
                        targetDir.Add(newDirectory);
                        element.Remove();
                    }
                    module.Add(targetDir);
                }
            }
        }      
    }
}