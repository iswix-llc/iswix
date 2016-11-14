using FireworksFramework.Types;

namespace WixShield.Designers.GeneralInformation
{
    partial class GeneralInformation
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.propertyGridProduct = new System.Windows.Forms.PropertyGrid();
            this.product = new WixShield.Designers.GeneralInformation.Product(this.components);
            this.propertyGridModule = new System.Windows.Forms.PropertyGrid();
            this.module = new WixShield.Designers.GeneralInformation.Module(this.components);
            this.propertyGridPackage = new System.Windows.Forms.PropertyGrid();
            this.package = new WixShield.Designers.GeneralInformation.Package(this.components);
            this.dependency = new GeneralInformationDesigner.Dependency();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dependency);
            this.splitContainer1.Size = new System.Drawing.Size(761, 462);
            this.splitContainer1.SplitterDistance = 269;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGridProduct);
            this.splitContainer2.Panel1.Controls.Add(this.propertyGridModule);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGridPackage);
            this.splitContainer2.Size = new System.Drawing.Size(761, 269);
            this.splitContainer2.SplitterDistance = 373;
            this.splitContainer2.TabIndex = 0;
            // 
            // propertyGridProduct
            // 
            this.propertyGridProduct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridProduct.Location = new System.Drawing.Point(0, 0);
            this.propertyGridProduct.Name = "propertyGridProduct";
            this.propertyGridProduct.SelectedObject = this.product;
            this.propertyGridProduct.Size = new System.Drawing.Size(373, 269);
            this.propertyGridProduct.TabIndex = 1;
            this.propertyGridProduct.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridProduct_PropertyValueChanged);
            // 
            // product
            // 
            this.product.Codepage = null;
            this.product.Id = null;
            this.product.Language = 0;
            this.product.Manufacturer = null;
            this.product.Name = null;
            this.product.UpgradeCode = null;
            this.product.Version = null;
            // 
            // propertyGridModule
            // 
            this.propertyGridModule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridModule.Location = new System.Drawing.Point(0, 0);
            this.propertyGridModule.Name = "propertyGridModule";
            this.propertyGridModule.SelectedObject = this.module;
            this.propertyGridModule.Size = new System.Drawing.Size(373, 269);
            this.propertyGridModule.TabIndex = 0;
            this.propertyGridModule.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridModule_PropertyValueChanged);
            // 
            // module
            // 
            this.module.Codepage = null;
            this.module.Id = null;
            this.module.Language = "0";
            this.module.Version = null;
            // 
            // propertyGridPackage
            // 
            this.propertyGridPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridPackage.Location = new System.Drawing.Point(0, 0);
            this.propertyGridPackage.Name = "propertyGridPackage";
            this.propertyGridPackage.SelectedObject = this.package;
            this.propertyGridPackage.Size = new System.Drawing.Size(384, 269);
            this.propertyGridPackage.TabIndex = 0;
            this.propertyGridPackage.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridPackage_PropertyValueChanged);
            // 
            // package
            // 
            this.package.AdminImage = null;
            this.package.Comments = null;
            this.package.Compressed = null;
            this.package.Description = null;
            this.package.Id = null;
            this.package.InstallerVersion = 200;
            this.package.InstallPrivileges = null;
            this.package.InstallScope = null;
            this.package.Keywords = null;
            this.package.Languages = null;
            this.package.Manufacturer = null;
            this.package.Platform = null;
            this.package.ReadOnly = null;
            this.package.ShortNames = null;
            this.package.SummaryCodepage = null;
            // 
            // dependency
            // 
            this.dependency.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dependency.Location = new System.Drawing.Point(0, 0);
            this.dependency.Name = "dependency";
            this.dependency.Size = new System.Drawing.Size(761, 189);
            this.dependency.TabIndex = 0;
            // 
            // GeneralInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "GeneralInformation";
            this.Size = new System.Drawing.Size(761, 462);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        protected System.Windows.Forms.PropertyGrid propertyGridModule;
        protected internal System.Windows.Forms.PropertyGrid propertyGridPackage;
        private Module module;
        private Package package;
        private Product product;
        protected System.Windows.Forms.PropertyGrid propertyGridProduct;
        private GeneralInformationDesigner.Dependency dependency;




    }
}
