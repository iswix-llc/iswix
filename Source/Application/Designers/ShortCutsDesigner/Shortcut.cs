using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace ShortCutsDesigner
{
    public partial class Shortcut : Component
    {
        IsWiXShortCut _shortcut;

        public Shortcut()
        {
            InitializeComponent();
        }

        public Shortcut(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public void Read(IsWiXShortCut shortcut)
        {
            _shortcut = shortcut;
            Name = _shortcut.Name;
            Description = _shortcut.Description;
            Show = _shortcut.Show;
            WorkingDirectory = _shortcut.WorkingDirectory;
            Arguments = _shortcut.Arguments;
        }



        [CategoryAttribute("General")]
        [Description(@"This column is the string that gives the shortcut a name.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public Show? Show { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {

                case "Name":
                    _shortcut.Name = Name;
                    break;
                case "Description":
                    _shortcut.Description = Description;
                    break;
                case "Show":
                    _shortcut.Show = Show;
                    break;
                case "WorkingDirectory":
                    _shortcut.WorkingDirectory = WorkingDirectory;
                    break;
                case "Arguments":
                    _shortcut.Arguments = Arguments;
                    break;
            }
        }


    }
}
