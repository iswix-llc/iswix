using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;

namespace IsWiX
{
    static class Program
    {
        [STAThreadAttribute()]
        static void Main(string[] args)
        {
//            try
//            {
                var fireworksManager = new FireworksManager(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.IsWiX.bmp")));
                fireworksManager.Start();
//            }
//            catch (Exception e)
//            {
//                MessageBox.Show(e.Message, Application.ProductName);
//            }
        }
    }
    public class IsWiXDesigner : IFireworksDesigner
    {
        public void LoadData()
        {
        }

        public IDesignerManager DesignerManager
        {
            set
            {
            }
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
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.MS-PL.txt")).ReadToEnd();
            }
        }

        public bool IsValidContext()
        {
            return false;
        }
    }

}