///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FireworksFramework.Interfaces;

namespace FireworksFramework.Managers
{
    public class FireworksManager 
    {
        // Branding BitMap provided by the branding exe.
        Bitmap _brandingBitMap;

        public string FilePath
        { 
            get
            {
                // Empty if no parms passed, otherwise the File Path of the default document to be opened.
                string filePath;
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    filePath = args[1];
                }
                else
                {
                    filePath = string.Empty;
                }
                return filePath;
            }
        }

        public Bitmap BrandingBitMap 
        {
            get
            {
                return _brandingBitMap;
            }
        }

        public FireworksManager(Bitmap BrandingBitMap)
        {
            try
            {
                Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            }
            catch (Exception)
            {
            }

            _brandingBitMap = BrandingBitMap;
        }

        public void Start()
        {
            new MainForm(this).ShowDialog();
        }

    }
}
