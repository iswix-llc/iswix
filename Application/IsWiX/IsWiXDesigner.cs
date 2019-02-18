using FireworksFramework.Interfaces;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using static FireworksFramework.Types.Enums;

namespace IsWiX
{
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

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("IsWiX.IsWiX.ico"));
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
