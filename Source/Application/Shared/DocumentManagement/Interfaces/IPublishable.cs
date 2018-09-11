using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Types;
using System.Xml.Linq;

namespace DocumentManagement.Interfaces
{
    public interface IPublishable
    {
        void Publish(DocumentStates DocumentState);
        void Publish(string DocumentPath);
        void Publish(XNamespace Namespace);
        void DocumentUpdated();
    }
}
