using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace NewFeaturesDesigner
{
    public partial class Feature : Component
    {
        IsWiXFeature4 _feature;
        
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
            AllowAbsent = null;
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
        public string Id { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Short string of text identifying the feature. This string is listed as an item by the SelectionTree control of the Selection Dialog. ")]
        public string Title { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines if a user will have the option to set a feature to absent in the user interface.")]
        public AllowAbsent? AllowAbsent { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the possible advertise states for this feature.")]
        public AllowAdvertise? AllowAdvertise { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Specify the Id of a Directory that can be configured by the user at installation time. This identifier must be a public property and therefore completely uppercase.")]
        public string ConfigurableDirectory { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default install/run location of a feature. This attribute cannot be specified if the value of the FollowParent attribute is 'yes' since that would ask the installer to force this feature to follow the parent installation state and simultaneously favor a particular installation state just for this feature.")]
        public InstallDefault? InstallDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default advertise state of the feature.")]
        public TypicalDefault? TypicalDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Sets the install level of this feature. A value of 0 will disable the feature. Processing the Condition Table can modify the level value (this is set via the Condition child element). The default value is '1'.")]
        public Int16? Level { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Determines the initial display of this feature in the feature tree.")]
        public Display? Display { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Longer string of text describing the feature. ")]
        public string Description { get; set; }

        public void Read(IsWiXFeature4 Feature)
        {
            _feature = Feature;
            Id = _feature.Id;
            Title = _feature.Title;
            AllowAbsent = _feature.AllowAbsent;
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

                case "AllowAbsent":
                    _feature.AllowAbsent = AllowAbsent;
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
