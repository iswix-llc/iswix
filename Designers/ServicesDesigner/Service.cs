using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace ServicesDesigner
{
    public partial class Service : Component
    {
        IsWiXServiceInstall _serviceInstall;
        IsWiXServiceControl _serviceControl;

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
        public string Name { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"This column is the string that user interface programs use to identify the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public string DisplayName { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Sets the description of the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public string Description { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines whether the existing service description will be ignored. If 'yes', the service description will be null, even if the Description attribute is set.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public YesNo? EraseDescription { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Contains any command line arguments or properties required to run the service.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public string Arguments { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The Windows Installer does not currently support kernelDriver or systemDriver.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public IsWiXAutomationInterface.Type Type { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines when the service should be started. The Windows Installer does not support boot or system.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Start Startup { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Determines what action should be taken on an error.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ErrorControl ErrorControl { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The account under which to start the service. Valid only when ServiceType is ownProcess.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public string Account { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The password for the account. Valid only when the account has a password.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Password { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The load ordering group that this service should be a part of.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public string LoadOrderGroup { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"Whether or not the service interacts with the desktop.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public YesNo? Interactive { get; set; }

        [CategoryAttribute(SERVICEINSTALL)]
        [Description(@"The overall install should fail if this service fails to install.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public YesNo? Vital { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be started by the StartServices action on install, uninstall or both. For 'install', the service will be started only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be started only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be started in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public InstallUninstall? Start { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be stopped by the StopServices action on install, uninstall or both. For 'install', the service will be stopped only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be stopped only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be stopped in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        
        public InstallUninstall? Stop { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether the service should be stopped by the StopServices action on install, uninstall or both. For 'install', the service will be stopped only when the parent component is being installed (msiInstallStateLocal or msiInstallStateSource); for 'uninstall', the service will be stopped only when the parent component is being removed (msiInstallStateAbsent); for 'both', the service will be stopped in both cases.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
        public InstallUninstall? Remove { get; set; }

        [CategoryAttribute(SERVICECONTROL)]
        [Description(@"Specifies whether or not to wait for the service to complete before continuing. The default is 'yes'.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public YesNo? Wait { get; set; }

        public void Read(IsWiXService service)
        {
            _serviceInstall = service.ServiceInstall;
            _serviceControl = service.ServiceControl;
            Name = _serviceInstall.Name;

            if (_serviceInstall.Name != _serviceControl.Name)
            {
                _serviceControl.Name = _serviceInstall.Name;
            }
            DisplayName = _serviceInstall.DisplayName;
            Description = _serviceInstall.Description;
            EraseDescription = _serviceInstall.EraseDescription;
            Arguments = _serviceInstall.Arguments;
            Type = _serviceInstall.Type;
            Startup = _serviceInstall.Start;
            ErrorControl = _serviceInstall.ErrorControl;
            Account = _serviceInstall.Account;
            Password = _serviceInstall.Password;
            LoadOrderGroup = _serviceInstall.LoadOrderGroup;
            Interactive = _serviceInstall.Interactive;
            Vital = _serviceInstall.Vital;
            Start = _serviceControl.Start;
            Stop = _serviceControl.Stop;
            Remove = _serviceControl.Remove;
            Wait = _serviceControl.Wait;
            

        }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {

                case "Name":
                    _serviceInstall.Name = Name;
                    _serviceControl.Name = Name;
                    break;
                case "DisplayName":
                    _serviceInstall.DisplayName = DisplayName;
                    break;
                case "Description":
                    _serviceInstall.Description = Description;
                    break;
                case "EraseDescription":
                    _serviceInstall.EraseDescription = EraseDescription;
                    break;
                case "Arguments":
                    _serviceInstall.Arguments = Arguments;
                    break;
                case "Type":
                    _serviceInstall.Type = Type;
                    break;
                case "Startup":
                    _serviceInstall.Start = Startup;
                    break;
                case "ErrorControl":
                    _serviceInstall.ErrorControl = ErrorControl;
                    break;
                case "Account":
                    _serviceInstall.Account = Account;
                    break;
                case "Password":
                    _serviceInstall.Password = Password;
                    break;
                case "LoadOrderGroup":
                    _serviceInstall.LoadOrderGroup = LoadOrderGroup;
                    break;
                case "Interactive":
                    _serviceInstall.Interactive = Interactive;
                    break;
                case "Vital":
                    _serviceInstall.Vital = Vital;
                    break;
                case "Start":
                    _serviceControl.Start = Start;
                    break;
                case "Stop":
                    _serviceControl.Stop = Stop;
                    break;
                case "Remove":
                    _serviceControl.Remove = Remove;
                    break;
                case "Wait":
                    _serviceControl.Wait = Wait;
                    break;

            }
        }
    }
}
