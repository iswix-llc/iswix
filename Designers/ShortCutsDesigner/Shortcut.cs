using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FireworksFramework.Authoring;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace ShortCutsDesigner
{
    public partial class Shortcut : Component
    {
        Wxs.Shortcut _shortcut;

        public Shortcut()
        {
            InitializeComponent();
        }

        public Shortcut(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        // Values are the attribute text, so expressions such as $(var.X) show and are kept.
        public void Read(Wxs.Shortcut shortcut)
        {
            _shortcut = shortcut;
            Name = _shortcut.Name;
            Description = _shortcut.Description;
            Show = _shortcut.Show.HasValue ? Wxs.WixEnumConverters.ShortcutShow.ToText(_shortcut.Show.Value) : null;
            WorkingDirectory = _shortcut.WorkingDirectory;
            Arguments = _shortcut.Arguments;
        }



        [CategoryAttribute("General")]
        [Description(@"This column is the string that gives the shortcut a name.")]

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Name { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Description { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(ShowConverter))]
        public string Show { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string WorkingDirectory { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Arguments { get; set; }

        // Only the attribute that changed is written.
        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {

                case "Name":
                    ShortcutDocument.SetName(_shortcut, Name);
                    break;
                case "Description":
                    _shortcut.Description = NullIfEmpty(Description);
                    break;
                case "Show":
                    if (string.IsNullOrEmpty(Show))
                    {
                        _shortcut.Show = null;
                        break;
                    }

                    var show = Wxs.WixEnumConverters.ShortcutShow.Parse(Show);
                    if (show.Kind == WixValueKind.Invalid)
                    {
                        MessageBox.Show($"'{Show}' is not a valid Show value.", "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Read(_shortcut);
                        break;
                    }

                    _shortcut.Show = show;
                    break;
                case "WorkingDirectory":
                    _shortcut.WorkingDirectory = NullIfEmpty(WorkingDirectory);
                    break;
                case "Arguments":
                    _shortcut.Arguments = NullIfEmpty(Arguments);
                    break;
            }
        }

        static string NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        class ShowConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.ShortcutShow);
        }
    }
}
