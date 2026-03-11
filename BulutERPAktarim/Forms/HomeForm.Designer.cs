// HomeForm.Designer.cs
namespace BulutERPAktarim.Forms
{
    partial class HomeForm
    {
        private System.ComponentModel.IContainer components = null;

        // FluentDesign
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer fluentDesignFormContainer1;
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl fluentDesignFormControl1;

        // Accordion
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementOperations;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementMaterialTransfer;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionSeparatorTop;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementSettings;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementCloudERPSettings;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementThemeSettings;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementErrorLogs;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementSQLiteSettings;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionSeparatorBottom;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionElementAbout;

        // Main panel
        private DevExpress.XtraEditors.PanelControl panelControlMain;

        // Bar
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarStaticItem barItemVersion;
        private DevExpress.XtraBars.PopupMenu popupMenuTheme;
        private DevExpress.XtraBars.SkinBarSubItem skinBarSubItemTheme;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeForm));
            this.fluentDesignFormContainer1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer();
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.accordionControl1 = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.accordionElementOperations = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionElementMaterialTransfer = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionSeparatorTop = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            this.accordionElementSettings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionElementCloudERPSettings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionElementThemeSettings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionElementErrorLogs = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionElementSQLiteSettings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionSeparatorBottom = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            this.accordionElementAbout = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.fluentDesignFormControl1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl();
            this.barItemVersion = new DevExpress.XtraBars.BarStaticItem();
            this.popupMenuTheme = new DevExpress.XtraBars.PopupMenu(this.components);
            this.skinBarSubItemTheme = new DevExpress.XtraBars.SkinBarSubItem();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.fluentDesignFormContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenuTheme)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // fluentDesignFormContainer1
            // 
            this.fluentDesignFormContainer1.Controls.Add(this.panelControlMain);
            this.fluentDesignFormContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fluentDesignFormContainer1.Location = new System.Drawing.Point(260, 31);
            this.fluentDesignFormContainer1.Name = "fluentDesignFormContainer1";
            this.fluentDesignFormContainer1.Size = new System.Drawing.Size(940, 669);
            this.fluentDesignFormContainer1.TabIndex = 0;
            // 
            // panelControlMain
            // 
            this.panelControlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 0);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(940, 669);
            this.panelControlMain.TabIndex = 0;
            // 
            // accordionControl1
            // 
            this.accordionControl1.Appearance.AccordionControl.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.accordionControl1.Appearance.AccordionControl.Options.UseFont = true;
            this.accordionControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl1.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.accordionElementOperations,
            this.accordionSeparatorTop,
            this.accordionElementSettings,
            this.accordionSeparatorBottom,
            this.accordionElementAbout});
            this.accordionControl1.Location = new System.Drawing.Point(0, 31);
            this.accordionControl1.Name = "accordionControl1";
            this.accordionControl1.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Touch;
            this.accordionControl1.Size = new System.Drawing.Size(260, 669);
            this.accordionControl1.TabIndex = 1;
            this.accordionControl1.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            // 
            // accordionElementOperations
            // 
            this.accordionElementOperations.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.accordionElementMaterialTransfer});
            this.accordionElementOperations.Expanded = true;
            this.accordionElementOperations.Name = "accordionElementOperations";
            this.accordionElementOperations.Text = "İşlemler";
            // 
            // accordionElementMaterialTransfer
            // 
            this.accordionElementMaterialTransfer.Name = "accordionElementMaterialTransfer";
            this.accordionElementMaterialTransfer.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementMaterialTransfer.Text = "📦  Malzeme Aktarımı";
            this.accordionElementMaterialTransfer.Click += new System.EventHandler(this.AccordionElementMaterialTransfer_Click);
            // 
            // accordionSeparatorTop
            // 
            this.accordionSeparatorTop.Name = "accordionSeparatorTop";
            // 
            // accordionElementSettings
            // 
            this.accordionElementSettings.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.accordionElementCloudERPSettings,
            this.accordionElementThemeSettings,
            this.accordionElementErrorLogs,
            this.accordionElementSQLiteSettings});
            this.accordionElementSettings.Expanded = true;
            this.accordionElementSettings.Name = "accordionElementSettings";
            this.accordionElementSettings.Text = "Ayarlar";
            // 
            // accordionElementCloudERPSettings
            // 
            this.accordionElementCloudERPSettings.Name = "accordionElementCloudERPSettings";
            this.accordionElementCloudERPSettings.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementCloudERPSettings.Text = "⚙️  Bulut ERP Ayarları";
            this.accordionElementCloudERPSettings.Click += new System.EventHandler(this.AccordionElementCloudERPSettings_Click);
            // 
            // accordionElementThemeSettings
            // 
            this.accordionElementThemeSettings.Name = "accordionElementThemeSettings";
            this.accordionElementThemeSettings.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementThemeSettings.Text = "🎨  Tema Ayarları";
            this.accordionElementThemeSettings.Click += new System.EventHandler(this.AccordionElementThemeSettings_Click);
            // 
            // accordionElementErrorLogs
            // 
            this.accordionElementErrorLogs.Name = "accordionElementErrorLogs";
            this.accordionElementErrorLogs.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementErrorLogs.Text = "🔴  Hata Kayıtları";
            this.accordionElementErrorLogs.Click += new System.EventHandler(this.AccordionElementErrorLogs_Click);
            // 
            // accordionElementSQLiteSettings
            // 
            this.accordionElementSQLiteSettings.Name = "accordionElementSQLiteSettings";
            this.accordionElementSQLiteSettings.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementSQLiteSettings.Text = "🗄️  SQLite Ayarları";
            this.accordionElementSQLiteSettings.Click += new System.EventHandler(this.AccordionElementSQLiteSettings_Click);
            // 
            // accordionSeparatorBottom
            // 
            this.accordionSeparatorBottom.Name = "accordionSeparatorBottom";
            // 
            // accordionElementAbout
            // 
            this.accordionElementAbout.Name = "accordionElementAbout";
            this.accordionElementAbout.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionElementAbout.Text = "ℹ️  Hakkımızda";
            this.accordionElementAbout.Click += new System.EventHandler(this.AccordionElementAbout_Click);
            // 
            // fluentDesignFormControl1
            // 
            this.fluentDesignFormControl1.FluentDesignForm = this;
            this.fluentDesignFormControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barItemVersion});
            this.fluentDesignFormControl1.Location = new System.Drawing.Point(0, 0);
            this.fluentDesignFormControl1.Name = "fluentDesignFormControl1";
            this.fluentDesignFormControl1.Size = new System.Drawing.Size(1200, 31);
            this.fluentDesignFormControl1.TabIndex = 2;
            this.fluentDesignFormControl1.TabStop = false;
            this.fluentDesignFormControl1.TitleItemLinks.Add(this.barItemVersion);
            // 
            // barItemVersion
            // 
            this.barItemVersion.Caption = "v1.0.0";
            this.barItemVersion.Id = 0;
            this.barItemVersion.Name = "barItemVersion";
            // 
            // popupMenuTheme
            // 
            this.popupMenuTheme.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.skinBarSubItemTheme)});
            this.popupMenuTheme.Manager = this.barManager1;
            this.popupMenuTheme.Name = "popupMenuTheme";
            // 
            // skinBarSubItemTheme
            // 
            this.skinBarSubItemTheme.Caption = "Tema Seç";
            this.skinBarSubItemTheme.Id = 1;
            this.skinBarSubItemTheme.Name = "skinBarSubItemTheme";
            // 
            // barManager1
            // 
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.skinBarSubItemTheme});
            this.barManager1.MaxItemId = 2;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 31);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1200, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 700);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1200, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 31);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 669);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1200, 31);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 669);
            // 
            // HomeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.ControlContainer = this.fluentDesignFormContainer1;
            this.Controls.Add(this.fluentDesignFormContainer1);
            this.Controls.Add(this.accordionControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Controls.Add(this.fluentDesignFormControl1);
            this.FluentDesignFormControl = this.fluentDesignFormControl1;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("HomeForm.IconOptions.Image")));
            this.Name = "HomeForm";
            this.NavigationControl = this.accordionControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mutlu Yazılım - Bulut ERP Aktarım";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HomeForm_FormClosing);
            this.Load += new System.EventHandler(this.HomeForm_Load);
            this.fluentDesignFormContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenuTheme)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}