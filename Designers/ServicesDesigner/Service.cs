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

namespace ServicesDesigner
{
    // Values are the attribute text, so expressions such as $(var.X) show and are kept.
    public partial class Service : Component
    {
        ServiceItem? _service;

        public Service()
        {
            InitializeComponent();
        }

        public Service(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        const string SERVICEINSTALL = "1. ServiceInstall";
        const string SERVICECONTROL= "2. ServiceControl";

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"This column is the string that gives the service name to install")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? Name { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"This column is the string that user interface programs use to identify the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? DisplayName { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Sets the description of the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? Description { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines whether the existing service description will be ignored. If 'yes', the service description will be null, even if the Description attribute is set.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string? EraseDescription { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Contains any command line arguments or properties required to run the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? Arguments { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The Windows Installer does not currently support kernelDriver or systemDriver.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(ServiceTypeConverter))]
        public string? Type { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines when the service should be started. The Windows Installer does not support boot or system.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(StartupConverter))]
        public string? Startup { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines what action should be taken on an error.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(ErrorControlConverter))]
        public string? ErrorControl { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The account under which to start the service. Valid only when ServiceType is ownProcess.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? Account { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The password for the account. Valid only when the account has a password.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? Password { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The load ordering group that this service should be a part of.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? LoadOrderGroup { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Whether or not the service interacts with the desktop.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string? Interactive { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The overall install should fail if this service fails to install.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string? Vital { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be started by the StartServices action on install, uninstall or both. For 'install', the service will be started only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be started only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be started in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(InstallUninstallConverter))]
        public string? Start { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be stopped by the StopServices action on install, uninstall or both. For 'install', the service will be stopped only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be stopped only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be stopped in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(InstallUninstallConverter))]
        public string? Stop { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be removed by the DeleteServices action on install, uninstall or both. For 'install', the service will be removed only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be removed only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be removed in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(InstallUninstallConverter))]
        public string? Remove { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether or not to wait for the service to complete before continuing. The default is 'yes'.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [TypeConverter(typeof(YesNoChoiceConverter))]
        public string? Wait { get; set; }

        public void Read(ServiceItem service)
        {
            _service = service;
            var install = service.Install;
            var control = service.Control;
            Name = install.Name;
            DisplayName = install.DisplayName;
            Description = install.Description;
            EraseDescription = WixText.Of(install.EraseDescription, WixConverters.YesNo);
            Arguments = install.Arguments;
            Type = WixText.Of(install.Type, Wxs.WixEnumConverters.ServiceInstallType);
            Startup = WixText.Of(install.Start, Wxs.WixEnumConverters.ServiceInstallStart);
            ErrorControl = WixText.Of(install.ErrorControl, Wxs.WixEnumConverters.ServiceInstallErrorControl);
            Account = install.Account;
            Password = install.Password;
            LoadOrderGroup = install.LoadOrderGroup;
            Interactive = WixText.Of(install.Interactive, WixConverters.YesNo);
            Vital = WixText.Of(install.Vital, WixConverters.YesNo);
            Start = WixText.Of(control?.Start, Wxs.WixEnumConverters.InstallUninstallType);
            Stop = WixText.Of(control?.Stop, Wxs.WixEnumConverters.InstallUninstallType);
            Remove = WixText.Of(control?.Remove, Wxs.WixEnumConverters.InstallUninstallType);
            Wait = WixText.Of(control?.Wait, WixConverters.YesNo);
        }

        // Only the attribute that changed is written.
        public void Write(string PropertyLabel)
        {
            var service = _service;
            if (service == null)
            {
                return;
            }

            var install = service.Install;
            switch (PropertyLabel)
            {

                case "Name":
                    ServiceDocument.SetName(service, Name);
                    break;
                case "DisplayName":
                    install.DisplayName = NullIfEmpty(DisplayName);
                    break;
                case "Description":
                    install.Description = NullIfEmpty(Description);
                    break;
                case "EraseDescription":
                    Set(EraseDescription, WixConverters.YesNo, PropertyLabel, false, value => install.EraseDescription = value);
                    break;
                case "Arguments":
                    install.Arguments = NullIfEmpty(Arguments);
                    break;
                case "Type":
                    Set(Type, Wxs.WixEnumConverters.ServiceInstallType, PropertyLabel, true, value => install.Type = value);
                    break;
                case "Startup":
                    Set(Startup, Wxs.WixEnumConverters.ServiceInstallStart, PropertyLabel, true, value => install.Start = value);
                    break;
                case "ErrorControl":
                    Set(ErrorControl, Wxs.WixEnumConverters.ServiceInstallErrorControl, PropertyLabel, true, value => install.ErrorControl = value);
                    break;
                case "Account":
                    install.Account = NullIfEmpty(Account);
                    break;
                case "Password":
                    install.Password = NullIfEmpty(Password);
                    break;
                case "LoadOrderGroup":
                    install.LoadOrderGroup = NullIfEmpty(LoadOrderGroup);
                    break;
                case "Interactive":
                    Set(Interactive, WixConverters.YesNo, PropertyLabel, false, value => install.Interactive = value);
                    break;
                case "Vital":
                    Set(Vital, WixConverters.YesNo, PropertyLabel, false, value => install.Vital = value);
                    break;
                case "Start":
                    Set(Start, Wxs.WixEnumConverters.InstallUninstallType, PropertyLabel, false, value => ServiceDocument.EnsureControl(service).Start = value);
                    break;
                case "Stop":
                    Set(Stop, Wxs.WixEnumConverters.InstallUninstallType, PropertyLabel, false, value => ServiceDocument.EnsureControl(service).Stop = value);
                    break;
                case "Remove":
                    Set(Remove, Wxs.WixEnumConverters.InstallUninstallType, PropertyLabel, false, value => ServiceDocument.EnsureControl(service).Remove = value);
                    break;
                case "Wait":
                    Set(Wait, WixConverters.YesNo, PropertyLabel, false, value => ServiceDocument.EnsureControl(service).Wait = value);
                    break;

            }
        }

        // A required attribute cannot be emptied; anything else must be a valid value or an expression.
        void Set<T>(string? text, WixValueConverter<T> converter, string label, bool required, Action<WixValue<T>?> set)
        {
            if (required && string.IsNullOrEmpty(text))
            {
                MessageBox.Show($"{label} is required.", "Services", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!WixText.TryParse(text, converter, out var value))
            {
                MessageBox.Show($"'{text}' is not a valid {label}.", "Services", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                set(value);
                return;
            }

            if (_service != null)
            {
                Read(_service);
            }
        }

        static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

        class ServiceTypeConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.ServiceInstallType);

            protected override bool Optional => false;
        }

        class StartupConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.ServiceInstallStart);

            protected override bool Optional => false;
        }

        class ErrorControlConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.ServiceInstallErrorControl);

            protected override bool Optional => false;
        }

        class InstallUninstallConverter : WixChoiceConverter
        {
            protected override IEnumerable<string> Choices => ChoicesOf(Wxs.WixEnumConverters.InstallUninstallType);
        }
    }
}
