///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Drawing;
using static FireworksFramework.Types.Enums;

namespace FireworksFramework.Interfaces
{
    /// <summary>
    /// A designer the plugin manager finds and shows for documents it can edit. A designer that works on the
    /// document has a public constructor taking the <see cref="Managers.DocumentManager"/> of the document it
    /// belongs to; each open document gets its own instances.
    /// </summary>
    public interface IFireworksDesigner
    {
        void LoadData();
        string PluginName { get; }
        Image PluginImage { get; }
        PluginType PluginType { get; }
        string PluginOrder { get; }
        bool IsValidContext();
    }
}
