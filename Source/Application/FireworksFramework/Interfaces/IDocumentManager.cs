///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FireworksFramework.Interfaces
{
    public interface IDocumentManager
    {
        XDocument Document { get; }
        XmlSchemaSet Schemas { get; }
        string DocumentPath { get; }
        string DocumentText { get; set; }
        string ValidateXML(string DocumentText);
        XNamespace DefaultNamespace { get; }
        Dictionary< string, XNamespace> NameSpaces { get; }
        bool CanSave { set; }
    }
}