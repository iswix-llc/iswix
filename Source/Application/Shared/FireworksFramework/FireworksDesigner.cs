///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;

namespace FireworksFramework
{
    // This class is to get this component to show up in the installed components list.
    public class FireworksDesigner : IFireworksDesigner
    {
        public void LoadData()
        {
            // FireworksFramework does not implement the LoadData method as it is not really a Fireworks designer.
        }

        public string PluginName
        {
            get 
            {
                // The component name we want to be known as.
                return "FireworksFramework"; 
            }
        }

        public System.Drawing.Image PluginImage
        {
            get 
            {
                // Pass off our embedded icon
                return Icon.ExtractAssociatedIcon( this.GetType().Assembly.Location).ToBitmap();
            }
        }


        public PluginType PluginType { get { return PluginType.Component; } }

        public string PluginOrder
        {
            // The fireworks framework is best always displayed second.
            get
            {
                return "Fireworks"; 
            }
        }

        public string PluginInformation
        {
            get
            {
                // The fireworks framework is best always displayed second.
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FireworksFramework.License.txt")).ReadToEnd();
 
            }
        }

        public bool IsValidContext()
        {
            throw new NotImplementedException();
        }
    }
}
