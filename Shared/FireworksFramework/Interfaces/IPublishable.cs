///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Xml.Linq;
using FireworksFramework.Types;

namespace FireworksFramework.Interfaces
{
    public interface IPublishable
    {
        void Publish(DocumentStates DocumentState);
        void Publish(string DocumentPath);
        void Publish(XNamespace Namespace);
        void DocumentUpdated();
    }
}
