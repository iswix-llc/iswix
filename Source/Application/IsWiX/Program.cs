using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using static FireworksFramework.Types.Enums;

namespace IsWiX
{
    static class Program
    {
        [STAThreadAttribute()]
        static void Main(string[] args)
        {
            var fireworksManager = FireworksManager.FireworksManagerInstance;
            // fireworksManager.BrandingBitMap = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.IsWiX.bmp");
            fireworksManager.ProductName = "IsWiX";
            fireworksManager.Start();
        }
    }
    public class IsWiXDesigner : IFireworksDesigner
    {
        public void LoadData()
        {
        }

        public PluginType PluginType { get { return PluginType.Application; } }

        public string PluginName
        {
            get { return "IsWiX"; }
        }

        public System.Drawing.Image PluginImage
        {
            get 
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap(); 
            }
        }

        public string PluginOrder
        {
            get { return "iswix"; }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.License.txt")).ReadToEnd();
            }
        }

        public bool IsValidContext()
        {
            return false;
        }
    }

}