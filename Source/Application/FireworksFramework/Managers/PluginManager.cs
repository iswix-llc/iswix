///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FireworksFramework.Interfaces;
using System.Windows.Forms;

namespace FireworksFramework.Managers
{

    // This class reflects application directory looking for assemblies that contain designers
    public partial class PluginManager : Component
    {
        SortedDictionary<string, IFireworksDesigner> _designers;

        public SortedDictionary<string, IFireworksDesigner> Designers
        {
            get
            {

                return _designers;
            }
        }

        public PluginManager()
        {
            InitializeComponent();
        }

        public PluginManager(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            PopulateDesigners();
        }

        public void PopulateDesigners()
        {
            _designers = new SortedDictionary<string, IFireworksDesigner>();
            DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath));
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(dInfo.GetFiles("*.dll", SearchOption.AllDirectories).ToList<FileInfo>());
            files.AddRange(dInfo.GetFiles("*.exe", SearchOption.AllDirectories).ToList<FileInfo>());

            List<Assembly> pluginAssemblies = new List<Assembly>();

            foreach (FileInfo file in files)
            {
                try
                {
                    // found an assembly
                    pluginAssemblies.Add(Assembly.LoadFile(file.FullName));
                }
                catch(Exception)
                {
                    //EXE's and DLL's are not always managed assemblies.
                }
            }

            foreach (Assembly pluginAssembly in pluginAssemblies)
            {
                try
                {
                    // Look for class(s) with our interface and construct them
                    foreach (var type in pluginAssembly.GetTypes())
                    {
                        Type iDesigner = type.GetInterface(typeof(IFireworksDesigner).FullName);
                        if (iDesigner != null)
                        {
                            IFireworksDesigner designer = Activator.CreateInstance(type) as IFireworksDesigner;
                            _designers.Add( designer.PluginType + designer.PluginOrder + Guid.NewGuid().ToString(), designer);
                        }
                    }
                }
                catch(Exception)
                {
                    //Something really bad must have happened. 
                    //MessageBox.Show("Fatal error reflecting plugins in assembly '" + pluginAssembly.FullName + "'.\r\n" +
                    //    "The error message is:\r\n\r\n" + e.Message);
                }
            }
        }
    }
}
