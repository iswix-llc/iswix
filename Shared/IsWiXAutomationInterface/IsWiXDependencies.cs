using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;
using WixToolset.Dtf.WindowsInstaller;

namespace IsWiXAutomationInterface
{
    public class IsWiXDependencies : List<IsWiXDependency>
    {
        static XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXDependencies()
        {
            Load();
        }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            try
            {
                foreach (var dependencyElement in _documentManager.Document.Descendants(ns + "Dependency"))
                {
                    string RequiredId = dependencyElement.Attribute("RequiredId").Value;
                    string RequiredLanguage = dependencyElement.Attribute("RequiredLanguage").Value;
                    string RequiredVersion = dependencyElement.Attribute("RequiredVersion").Value;
                    base.Add(new IsWiXDependency(RequiredId, RequiredLanguage, RequiredVersion));
                }
            }
            catch (Exception)
            {
                throw new Exception( "Error parsing XML. Please check your Dependency elements.");
            }
        }

        public new void Add(IsWiXDependency Dependency)
        {
            base.Add(Dependency);
            var newdependency =
                new XElement(ns + "Dependency",
                    new XAttribute("RequiredId", Dependency.RequiredId),
                    new XAttribute("RequiredLanguage", Dependency.RequiredLanguage),
                    new XAttribute("RequiredVersion", Dependency.RequiredVersion
                        ));

            _documentManager.Document.GetElementToAddAfterSelf("Dependency").AddAfterSelf(newdependency);


            var currentDependencies = from a in _documentManager.Document.Descendants(ns + "Dependency")
                                      orderby (string)a.Attribute("RequiredId").Value, (string)a.Attribute("RequiredLanguage").Value ascending
                                      select new { dependency = a };

            var temp = XDocument.Parse(_documentManager.Document.ToString());
            temp.Descendants(ns + "Dependency").Remove();

            XElement previousNode = temp.Descendants(ns + "Package").First();
            foreach (var dependency in currentDependencies)
            {
                previousNode.AddAfterSelf(dependency.dependency);
                previousNode = temp.Descendants(ns + "Dependency").Last();
            }
            _documentManager.Document.Descendants(ns + "Dependency").Remove();
            _documentManager.Document.GetElementToAddAfterSelf("Dependency").AddAfterSelf(temp.Descendants(ns + "Dependency"));

        }

        public new void Remove(IsWiXDependency Dependency)
        {
            base.Remove(Dependency);
            _documentManager.Document.Descendants(ns + "Dependency").Where(x =>
                 (string)x.Attribute("RequiredId") == Dependency.RequiredId &&
                 (string)x.Attribute("RequiredLanguage") == Dependency.RequiredLanguage
                 ).Remove();
        }
    }

    public class IsWiXDependency
    {
        string _requiredId;
        string _requiredLanguage;
        string _requiredVersion;

        public IsWiXDependency(string FilePath)
        {
            try
            {
                using (var db = new Database(FilePath, DatabaseOpenMode.ReadOnly))
                {
                    using (var view =
                        db.OpenView(db.Tables["ModuleSignature"].SqlSelectString))
                    {
                        view.Execute();
                        using (var record = view.Fetch())
                        {
                            _requiredId = record["ModuleID"] as string;
                            _requiredLanguage = record["Language"].ToString();
                            _requiredVersion = record["Version"] as string;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error querying merge module.");
            }
        }

        public IsWiXDependency(string RequiredId, string RequiredLanguage, string RequiredVersion)
        {
            _requiredId = RequiredId;
            _requiredLanguage = RequiredLanguage;
            _requiredVersion = RequiredVersion;
        }
        public string RequiredId { get { return _requiredId; } }
        public string RequiredVersion { get { return _requiredVersion; } }
        public string RequiredLanguage { get { return _requiredLanguage; } }
    }
}