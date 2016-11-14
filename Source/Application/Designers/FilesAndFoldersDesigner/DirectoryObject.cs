using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace WixShield.Designers.FilesAndFolders
{
    class DirectoryObject
    {
        [CategoryAttribute("Directory")]
        public string Id { get; set; }
        [Category("Directory")]
        public string Name { get; set; }

        public void Read(XElement document)
        {
            IEnumerable<XElement> directories = 
                from dir in document.Elements("Directory")
                select dir;

            foreach (XElement directory in directories)
            {
                Id = (directory.Attribute("Id") != null) ? directory.Attribute("Id").Value : null;
                Name = (directory.Attribute("Name") != null) ? directory.Attribute("Name").Value : null;
            }
        }


    
    }
}
