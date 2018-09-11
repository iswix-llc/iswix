using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;

namespace IsWiX
{
    static class Program
    {
        [STAThreadAttribute()]
        static void Main(string[] args)
        {
            var fireworksManager = new FireworksManager(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.IsWiX.bmp")));
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