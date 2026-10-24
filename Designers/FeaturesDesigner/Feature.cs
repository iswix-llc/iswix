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

namespace FeaturesDesigner
{
    // Values are the attribute text, so expressions such as $(var.X) show and are kept.
    public partial class Feature : Component
    {
        Wxs.Feature _feature;

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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string Id { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Short string of text identifying the feature. This string is listed as an item by the SelectionTree control of the Selection Dialog. ")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string Title { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines if a user will have the option to set a feature to absent in the user interface.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string AllowAbsent { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines whether the feature can be advertised.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string AllowAdvertise { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Specify the Id of a Directory that can be configured by the user at installation time. This identifier must be a public property and therefore completely uppercase.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ConfigurableDirectory { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default install/run location of a feature. This attribute cannot be specified if the value of the FollowParent attribute is 'yes' since that would ask the installer to force this feature to follow the parent installation state and simultaneously favor a particular installation state just for this feature.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(InstallDefaultConverter))]
        public string InstallDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"This attribute determines the default advertise state of the feature.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(TypicalDefaultConverter))]
        public string TypicalDefault { get; set; }

        [CategoryAttribute("Properties")]
        [Description(@"Sets the install level of this feature. A value of 0 will disable the feature. Processing the Condition Table can modify the level value (this is set via the Condition child element). The default value is '1'.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Level { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Determines the initial display of this feature in the feature tree: collapse, expand, hidden, or a number giving its position.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(DisplayConverter))]
        public string Display { get; set; }

        [CategoryAttribute("Display")]
        [Description(@"Longer string of text describing the feature. ")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Description { get; set; }

        public void Read(Wxs.Feature Feature)
        {
            _feature = Feature;
            Id = _feature.Id;
            Title = _feature.Title;
            AllowAbsent = WixText.Of(_feature.AllowAbsent, WixConverters.YesNo);
            AllowAdvertise = WixText.Of(_feature.AllowAdvertise, WixConverters.YesNo);
            ConfigurableDirectory = _feature.ConfigurableDirectory;
            InstallDefault = WixText.Of(_feature.InstallDefault, Wxs.WixEnumConverters.FeatureInstallDefault);
            TypicalDefault = WixText.Of(_feature.TypicalDefault, Wxs.WixEnumConverters.FeatureTypicalDefault);
            Level = WixText.Of(_feature.Level, WixConverters.Int32);
            Description = _feature.Description;
            Display = _feature.Display;
        }

        // Only the attribute that changed is written. (The Id is changed by renaming the feature in the tree.)
        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {
                case "Title":
                    _feature.Title = NullIfEmpty(Title);
                    break;

                case "AllowAbsent":
                    Set(AllowAbsent, WixConverters.YesNo, PropertyLabel, value => _feature.AllowAbsent = value);
                    break;

                case "AllowAdvertise":
                    Set(AllowAdvertise, WixConverters.YesNo, PropertyLabel, value => _feature.AllowAdvertise = value);
                    break;

                case "ConfigurableDirectory":
                    _feature.ConfigurableDirectory = NullIfEmpty(ConfigurableDirectory);
                    break;

                case "InstallDefault":
                    Set(InstallDefault, Wxs.WixEnumConverters.FeatureInstallDefault, PropertyLabel, value => _feature.InstallDefault = value);
                    break;
                case "TypicalDefault":
                    Set(TypicalDefault, Wxs.WixEnumConverters.FeatureTypicalDefault, PropertyLabel, value => _feature.TypicalDefault = value);
                    break;
                case "Level":
                    Set(Level, WixConverters.Int32, PropertyLabel, value => _feature.Level = value);
                    break;
                case "Description":
                    _feature.Description = NullIfEmpty(Description);
                    break;
                case "Display":
                    _feature.Display = NullIfEmpty(Display);
                    break;
            }
        }

        // Anything but empty must be a valid value or an expression.
        void Set<T>(string text, WixValueConverter<T> converter, string label, Action<WixValue<T>?> set)
        {
            if (WixText.TryParse(text, converter, out var value))
            {
                set(value);
                return;
            }

            MessageBox.Show($"'{text}' is not a valid {label}.", "Features", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Read(_feature);
        }

        static string NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        class InstallDefaultConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.FeatureInstallDefault);
        }

        class TypicalDefaultConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.FeatureTypicalDefault);
        }

        class DisplayConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => new[] { "collapse", "expand", "hidden" };
        }
    }
}
