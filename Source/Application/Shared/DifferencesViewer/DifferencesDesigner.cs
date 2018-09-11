using FireworksFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DifferencesViewer
{
    class DifferencesDesigner : IFireworksDesigner
    {
        public string PluginName { get { return "Differences"; } }


        public System.Drawing.Image PluginImage
        {
            get
            {
                // Pass off our embedded icon
                return Icon.ExtractAssociatedIcon(this.GetType().Assembly.Location).ToBitmap();
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
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DifferencesViewer.License.txt")).ReadToEnd();

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
