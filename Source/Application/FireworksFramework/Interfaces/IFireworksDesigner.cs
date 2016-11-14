///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Drawing;
using FireworksFramework.Types;

namespace FireworksFramework.Interfaces
{
    public interface IFireworksDesigner
    {
        void LoadData();
        IDesignerManager DesignerManager { set; }
        string PluginName { get; }
        Image PluginImage { get; }
        PluginType PluginType { get; }
        string PluginOrder { get; }
        string PluginInformation { get; }
        bool IsValidContext();
    }
}
