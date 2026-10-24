///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FireworksFramework.Interfaces;

namespace FireworksFramework.Managers
{
    // This class reflects application directory looking for assemblies that contain designers
    public partial class PluginManager
    {
        SortedDictionary<string, IFireworksDesigner> _designers = new SortedDictionary<string, IFireworksDesigner>();

        private static readonly Lazy<PluginManager> lazy = new Lazy<PluginManager>(() => new PluginManager());

        public static PluginManager PluginManagerInstance { get { return lazy.Value; } }

        public SortedDictionary<string, IFireworksDesigner> Designers
        {
            get
            {

                return _designers;
            }
        }

        private PluginManager()
        {
            PopulateDesigners();
        }
        private void PopulateDesigners()
        {
            _designers.Clear();
            DirectoryInfo dInfo = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!;
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(dInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly).ToList<FileInfo>());
            files.AddRange(dInfo.GetFiles("*.exe", SearchOption.TopDirectoryOnly).ToList<FileInfo>());

            List<Assembly> pluginAssemblies = new List<Assembly>();

            foreach (FileInfo file in files)
            {
                try
                {
                    // found an assembly
                    pluginAssemblies.Add(AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file.FullName).GetName()));
                }
                catch (Exception)
                {
                    //EXE's and DLL's are not always managed assemblies.
                }
            }

            foreach (Assembly pluginAssembly in pluginAssemblies)
            {
                try
                {
                    // Look for class(s) with our interface and construct them
                    Type[] types = pluginAssembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.GetInterface(typeof(IFireworksDesigner).FullName!) != null &&
                            Activator.CreateInstance(type) is IFireworksDesigner designer)
                        {
                            _designers.Add(designer.PluginType + designer.PluginOrder + Guid.NewGuid().ToString(), designer);
                        }
                    }
                }
                catch (Exception)
                {
                    //Something really bad must have happened. 
                    //MessageBox.Show("Fatal error reflecting plugins in assembly '" + pluginAssembly.FullName + "'.\r\n" +
                    //    "The error message is:\r\n\r\n" + e.Message);
                }
            }
        }
    }
}
