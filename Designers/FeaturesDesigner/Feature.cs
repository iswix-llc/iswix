using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace FeaturesDesigner
{
    public partial class Feature : Component
    {
        IsWiXFeature _feature;
        
        public Feature()
        {
            InitializeComponent();
        }

        public Feature(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
                
        public void Clear()
        {
            _feature = null;
            Id = "";
            Title = "";
            Absent = null;
            AllowAdvertise = null;
            ConfigurableDirectory = "";
            InstallDefault = null;
            TypicalDefault = null;
            Level = null;
            Description = "";
            Display = null;

        }
        [CategoryAttribute("\tIdentity")]
        [Description(@"Unique identifier of the feature.")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string Id { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Short string of text identifying the feature. This string is listed as an item by the SelectionTree control of the Selection Dialog. ")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string Title { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines if a user will have the option to set a feature to absent in the user interface.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public Absent? Absent { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the possible advertise states for this feature.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public AllowAdvertise? AllowAdvertise { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Specify the Id of a Directory that can be configured by the user at installation time. This identifier must be a public property and therefore completely uppercase.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string ConfigurableDirectory { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default install/run location of a feature. This attribute cannot be specified if the value of the FollowParent attribute is 'yes' since that would ask the installer to force this feature to follow the parent installation state and simultaneously favor a particular installation state just for this feature.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public InstallDefault? InstallDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default advertise state of the feature.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public TypicalDefault? TypicalDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Sets the install level of this feature. A value of 0 will disable the feature. Processing the Condition Table can modify the level value (this is set via the Condition child element). The default value is '1'.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public Int16? Level { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Determines the initial display of this feature in the feature tree.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public Display? Display { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Longer string of text describing the feature. ")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string Description { get; set; }

        public void Read(IsWiXFeature Feature)
        {
            _feature = Feature;
            Id = _feature.Id;
            Title = _feature.Title;
            Absent = _feature.Absent;
            AllowAdvertise = _feature.AllowAdvertise;
            ConfigurableDirectory = _feature.ConfigurableDirectory;
            InstallDefault = _feature.InstallDefault;
            TypicalDefault = _feature.TypicalDefault;
            Level = _feature.Level;
            Description = _feature.Description;
            Display = _feature.Display;
        }
        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {
                case "Id":
                    _feature.Id = Id;
                    break;

                case "Title":
                    _feature.Title = Title;
                    break;

                case "Absent":
                    _feature.Absent = Absent;
                    break;

                case "AllowAdvertise":
                    _feature.AllowAdvertise = AllowAdvertise;
                    break;

                case "ConfigurableDirectory":
                    _feature.ConfigurableDirectory = ConfigurableDirectory;
                    break;

                case "InstallDefault":
                    _feature.InstallDefault = InstallDefault;
                    break;
                case "TypicalDefault":
                    _feature.TypicalDefault = TypicalDefault;
                    break;
                case "Level":
                    _feature.Level = Level;
                    break;
                case "Description":
                    _feature.Description = Description;
                    break;
                case "Display":
                    _feature.Display = Display;
                    break;
            }
        }
    }
}
