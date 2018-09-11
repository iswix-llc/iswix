///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Drawing;

namespace FireworksFramework.Interfaces
{
    public interface IFireworksDesigner
    {
        void LoadData();
        string PluginName { get; }
        Image PluginImage { get; }
        PluginType PluginType { get; }
        string PluginOrder { get; }
        string PluginInformation { get; }
        bool IsValidContext();
    }
}
