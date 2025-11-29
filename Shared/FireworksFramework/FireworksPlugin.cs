///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using FireworksFramework.Interfaces;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using static FireworksFramework.Types.Enums;

namespace FireworksFramework
{
    class FireworksPlugin : IFireworksDesigner
    {
        public PluginType PluginType { get { return PluginType.Component; } }

        public string PluginName
        {
            get { return "Fireworks"; }
        }

        public Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FireworksFramework.Fireworks.ico"));
            }
        }

        public string PluginOrder
        {
            get { return "zzzfireworks"; }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FireworksFramework.License.txt")).ReadToEnd();
            }
        }

        public bool IsValidContext()
        {
            return false;
        }

        public void LoadData()
        {
        }
    }
}
