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
        readonly List<Type> _designerTypes = new List<Type>();

        private static readonly Lazy<PluginManager> lazy = new Lazy<PluginManager>(() => new PluginManager());

        public static PluginManager PluginManagerInstance { get { return lazy.Value; } }

        private PluginManager()
        {
            FindDesignerTypes();
        }

        /// <summary>
        /// Creates a set of designers for one document, ordered by <see cref="IFireworksDesigner.PluginType"/> and
        /// <see cref="IFireworksDesigner.PluginOrder"/>. A designer that takes a <see cref="DocumentManager"/> in its
        /// constructor gets <paramref name="documentManager"/>; one that doesn't need a document has a parameterless
        /// constructor.
        /// </summary>
        public IReadOnlyList<IFireworksDesigner> CreateDesigners(DocumentManager documentManager)
        {
            var designers = new List<IFireworksDesigner>();
            foreach (var type in _designerTypes)
            {
                try
                {
                    var designer = type.GetConstructor(new[] { typeof(DocumentManager) }) is { } withDocument
                        ? withDocument.Invoke(new object[] { documentManager })
                        : Activator.CreateInstance(type);
                    if (designer is IFireworksDesigner fireworksDesigner)
                    {
                        designers.Add(fireworksDesigner);
                    }
                }
                catch (Exception)
                {
                    // A designer that fails to construct is left out, as before; the others still load.
                }
            }

            return designers.OrderBy(d => d.PluginType + d.PluginOrder, Comparer<string>.Default).ToList();
        }

        private void FindDesignerTypes()
        {
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
                    // Look for class(s) with our interface
                    foreach (var type in pluginAssembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && type.GetInterface(typeof(IFireworksDesigner).FullName!) != null)
                        {
                            _designerTypes.Add(type);
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
