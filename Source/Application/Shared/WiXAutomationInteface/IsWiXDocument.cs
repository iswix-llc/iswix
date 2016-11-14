using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

namespace IsWiXAutomationInterface
{
    public class IsWiXDocument
    {
        XDocument _document;

        public IsWiXDocument(XDocument Document)
        {
            _document = Document;
        }

        public IsWiXDocumentType DocumentType 
        {
            get
            {
                return _document.GetDocumentType();
            }
        }

    }

}
