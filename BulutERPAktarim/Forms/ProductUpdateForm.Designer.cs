namespace BulutERPAktarim.Forms
{
    partial class ProductUpdateForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductUpdateForm));
            this.barManager = new DevExpress.XtraBars.BarManager(this.components);
            this.barTop = new DevExpress.XtraBars.Bar();
            this.barItemDosyaSec = new DevExpress.XtraBars.BarButtonItem();
            this.barItemTemizle = new DevExpress.XtraBars.BarButtonItem();
            this.barItemSablon = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.splitContainerControl = new DevExpress.XtraEditors.SplitContainerControl();
            this.panelLeft = new DevExpress.XtraEditors.PanelControl();
            this.groupControlIslemler = new DevExpress.XtraEditors.GroupControl();
            this.btnTumunuSec = new DevExpress.XtraEditors.SimpleButton();
            this.btnSecimiTemizle = new DevExpress.XtraEditors.SimpleButton();
            this.btnGuncelle = new DevExpress.XtraEditors.SimpleButton();
            this.progressBar = new DevExpress.XtraEditors.ProgressBarControl();
            this.labelDurum = new DevExpress.XtraEditors.LabelControl();
            this.groupControlFiltre = new DevExpress.XtraEditors.GroupControl();
            this.checkEditSadecHata = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditSadecGuncellenecek = new DevExpress.XtraEditors.CheckEdit();
            this.labelToplam = new DevExpress.XtraEditors.LabelControl();
            this.labelSecili = new DevExpress.XtraEditors.LabelControl();
            this.labelHata = new DevExpress.XtraEditors.LabelControl();
            this.groupControlDosya = new DevExpress.XtraEditors.GroupControl();
            this.btnDosyaSec = new DevExpress.XtraEditors.SimpleButton();
            this.labelControlDosyaYolu = new DevExpress.XtraEditors.LabelControl();
            this.textEditDosyaYolu = new DevExpress.XtraEditors.TextEdit();
            this.panelRight = new DevExpress.XtraEditors.PanelControl();
            this.gridControl = new DevExpress.XtraGrid.GridControl();
            this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSec = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.colSatirNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMalzemeKodu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAciklama = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBarkod = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colOzelKod1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colOzelKod2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colOzelKod3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colOzelKod4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colOzelKod5 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMarka = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colModel = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDurum = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHataMesaji = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHata = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl)).BeginInit();
            this.splitContainerControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).BeginInit();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlIslemler)).BeginInit();
            this.groupControlIslemler.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlFiltre)).BeginInit();
            this.groupControlFiltre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSadecHata.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSadecGuncellenecek.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlDosya)).BeginInit();
            this.groupControlDosya.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditDosyaYolu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelRight)).BeginInit();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHata)).BeginInit();
            this.SuspendLayout();
            // 
            // barManager
            // 
            this.barManager.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.barTop});
            this.barManager.DockControls.Add(this.barDockControlTop);
            this.barManager.DockControls.Add(this.barDockControlBottom);
            this.barManager.DockControls.Add(this.barDockControlLeft);
            this.barManager.DockControls.Add(this.barDockControlRight);
            this.barManager.Form = this;
            this.barManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barItemDosyaSec,
            this.barItemTemizle,
            this.barItemSablon});
            this.barManager.MaxItemId = 3;
            // 
            // barTop
            // 
            this.barTop.BarName = "Araçlar";
            this.barTop.DockCol = 0;
            this.barTop.DockRow = 0;
            this.barTop.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.barTop.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barItemDosyaSec),
            new DevExpress.XtraBars.LinkPersistInfo(this.barItemTemizle),
            new DevExpress.XtraBars.LinkPersistInfo(this.barItemSablon)});
            this.barTop.Text = "Araçlar";
            // 
            // barItemDosyaSec
            // 
            this.barItemDosyaSec.Caption = "📂  Dosya Seç";
            this.barItemDosyaSec.Id = 0;
            this.barItemDosyaSec.Name = "barItemDosyaSec";
            this.barItemDosyaSec.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            this.barItemDosyaSec.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BarItemDosyaSec_ItemClick);
            // 
            // barItemTemizle
            // 
            this.barItemTemizle.Caption = "|  Listeyi Temizle";
            this.barItemTemizle.Id = 1;
            this.barItemTemizle.Name = "barItemTemizle";
            this.barItemTemizle.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BarItemTemizle_ItemClick);
            // 
            // barItemSablon
            // 
            this.barItemSablon.Caption = "|  Şablonu İndir";
            this.barItemSablon.Id = 2;
            this.barItemSablon.Name = "barItemSablon";
            this.barItemSablon.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BarItemSablon_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager;
            this.barDockControlTop.Size = new System.Drawing.Size(1400, 20);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 700);
            this.barDockControlBottom.Manager = this.barManager;
            this.barDockControlBottom.Size = new System.Drawing.Size(1400, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 20);
            this.barDockControlLeft.Manager = this.barManager;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 680);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1400, 20);
            this.barDockControlRight.Manager = this.barManager;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 680);
            // 
            // splitContainerControl
            // 
            this.splitContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl.Location = new System.Drawing.Point(0, 20);
            this.splitContainerControl.Name = "splitContainerControl";
            this.splitContainerControl.Panel1.Controls.Add(this.panelLeft);
            this.splitContainerControl.Panel1.MinSize = 270;
            this.splitContainerControl.Panel2.Controls.Add(this.panelRight);
            this.splitContainerControl.Size = new System.Drawing.Size(1400, 680);
            this.splitContainerControl.SplitterPosition = 290;
            this.splitContainerControl.TabIndex = 0;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.groupControlIslemler);
            this.panelLeft.Controls.Add(this.groupControlFiltre);
            this.panelLeft.Controls.Add(this.groupControlDosya);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(290, 680);
            this.panelLeft.TabIndex = 0;
            // 
            // groupControlIslemler
            // 
            this.groupControlIslemler.Controls.Add(this.btnTumunuSec);
            this.groupControlIslemler.Controls.Add(this.btnSecimiTemizle);
            this.groupControlIslemler.Controls.Add(this.btnGuncelle);
            this.groupControlIslemler.Controls.Add(this.progressBar);
            this.groupControlIslemler.Controls.Add(this.labelDurum);
            this.groupControlIslemler.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControlIslemler.Location = new System.Drawing.Point(2, 242);
            this.groupControlIslemler.Name = "groupControlIslemler";
            this.groupControlIslemler.Size = new System.Drawing.Size(286, 200);
            this.groupControlIslemler.TabIndex = 0;
            this.groupControlIslemler.Text = "İşlemler";
            // 
            // btnTumunuSec
            // 
            this.btnTumunuSec.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnTumunuSec.Appearance.Options.UseBackColor = true;
            this.btnTumunuSec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTumunuSec.Location = new System.Drawing.Point(10, 30);
            this.btnTumunuSec.Name = "btnTumunuSec";
            this.btnTumunuSec.Size = new System.Drawing.Size(128, 28);
            this.btnTumunuSec.TabIndex = 0;
            this.btnTumunuSec.Text = "✔  Tümünü Seç";
            this.btnTumunuSec.Click += new System.EventHandler(this.BtnTumunuSec_Click);
            // 
            // btnSecimiTemizle
            // 
            this.btnSecimiTemizle.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Danger;
            this.btnSecimiTemizle.Appearance.Options.UseBackColor = true;
            this.btnSecimiTemizle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSecimiTemizle.Location = new System.Drawing.Point(148, 30);
            this.btnSecimiTemizle.Name = "btnSecimiTemizle";
            this.btnSecimiTemizle.Size = new System.Drawing.Size(128, 28);
            this.btnSecimiTemizle.TabIndex = 1;
            this.btnSecimiTemizle.Text = "✖  Seçimi Temizle";
            this.btnSecimiTemizle.Click += new System.EventHandler(this.BtnSecimiTemizle_Click);
            // 
            // btnGuncelle
            // 
            this.btnGuncelle.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnGuncelle.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnGuncelle.Appearance.Options.UseBackColor = true;
            this.btnGuncelle.Appearance.Options.UseForeColor = true;
            this.btnGuncelle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuncelle.Location = new System.Drawing.Point(10, 68);
            this.btnGuncelle.Name = "btnGuncelle";
            this.btnGuncelle.Size = new System.Drawing.Size(266, 34);
            this.btnGuncelle.TabIndex = 2;
            this.btnGuncelle.Text = "🔄  Seçilenleri Güncelle";
            this.btnGuncelle.Click += new System.EventHandler(this.BtnGuncelle_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 114);
            this.progressBar.Name = "progressBar";
            this.progressBar.Properties.ShowTitle = true;
            this.progressBar.Size = new System.Drawing.Size(266, 20);
            this.progressBar.TabIndex = 3;
            // 
            // labelDurum
            // 
            this.labelDurum.Location = new System.Drawing.Point(10, 140);
            this.labelDurum.Name = "labelDurum";
            this.labelDurum.Size = new System.Drawing.Size(24, 13);
            this.labelDurum.TabIndex = 4;
            this.labelDurum.Text = "Hazır";
            // 
            // groupControlFiltre
            // 
            this.groupControlFiltre.Controls.Add(this.checkEditSadecHata);
            this.groupControlFiltre.Controls.Add(this.checkEditSadecGuncellenecek);
            this.groupControlFiltre.Controls.Add(this.labelToplam);
            this.groupControlFiltre.Controls.Add(this.labelSecili);
            this.groupControlFiltre.Controls.Add(this.labelHata);
            this.groupControlFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControlFiltre.Location = new System.Drawing.Point(2, 112);
            this.groupControlFiltre.Name = "groupControlFiltre";
            this.groupControlFiltre.Size = new System.Drawing.Size(286, 130);
            this.groupControlFiltre.TabIndex = 1;
            this.groupControlFiltre.Text = "Filtre  Özet";
            // 
            // checkEditSadecHata
            // 
            this.checkEditSadecHata.Location = new System.Drawing.Point(10, 30);
            this.checkEditSadecHata.Name = "checkEditSadecHata";
            this.checkEditSadecHata.Properties.Caption = "Sadece Hatalıları Göster";
            this.checkEditSadecHata.Size = new System.Drawing.Size(180, 20);
            this.checkEditSadecHata.TabIndex = 0;
            this.checkEditSadecHata.EditValueChanged += new System.EventHandler(this.CheckEditFiltre_Changed);
            // 
            // checkEditSadecGuncellenecek
            // 
            this.checkEditSadecGuncellenecek.Location = new System.Drawing.Point(10, 54);
            this.checkEditSadecGuncellenecek.Name = "checkEditSadecGuncellenecek";
            this.checkEditSadecGuncellenecek.Properties.Caption = "Sadece Güncellenecekleri Göster";
            this.checkEditSadecGuncellenecek.Size = new System.Drawing.Size(200, 20);
            this.checkEditSadecGuncellenecek.TabIndex = 1;
            this.checkEditSadecGuncellenecek.EditValueChanged += new System.EventHandler(this.CheckEditFiltre_Changed);
            // 
            // labelToplam
            // 
            this.labelToplam.Location = new System.Drawing.Point(10, 84);
            this.labelToplam.Name = "labelToplam";
            this.labelToplam.Size = new System.Drawing.Size(47, 13);
            this.labelToplam.TabIndex = 2;
            this.labelToplam.Text = "Toplam: 0";
            // 
            // labelSecili
            // 
            this.labelSecili.Location = new System.Drawing.Point(10, 100);
            this.labelSecili.Name = "labelSecili";
            this.labelSecili.Size = new System.Drawing.Size(36, 13);
            this.labelSecili.TabIndex = 3;
            this.labelSecili.Text = "Seçili: 0";
            // 
            // labelHata
            // 
            this.labelHata.Appearance.ForeColor = System.Drawing.Color.Red;
            this.labelHata.Appearance.Options.UseForeColor = true;
            this.labelHata.Location = new System.Drawing.Point(120, 84);
            this.labelHata.Name = "labelHata";
            this.labelHata.Size = new System.Drawing.Size(36, 13);
            this.labelHata.TabIndex = 4;
            this.labelHata.Text = "Hata: 0";
            // 
            // groupControlDosya
            // 
            this.groupControlDosya.Controls.Add(this.btnDosyaSec);
            this.groupControlDosya.Controls.Add(this.labelControlDosyaYolu);
            this.groupControlDosya.Controls.Add(this.textEditDosyaYolu);
            this.groupControlDosya.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControlDosya.Location = new System.Drawing.Point(2, 2);
            this.groupControlDosya.Name = "groupControlDosya";
            this.groupControlDosya.Size = new System.Drawing.Size(286, 110);
            this.groupControlDosya.TabIndex = 2;
            this.groupControlDosya.Text = "Excel Dosyası";
            // 
            // btnDosyaSec
            // 
            this.btnDosyaSec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDosyaSec.Location = new System.Drawing.Point(10, 70);
            this.btnDosyaSec.Name = "btnDosyaSec";
            this.btnDosyaSec.Size = new System.Drawing.Size(266, 26);
            this.btnDosyaSec.TabIndex = 2;
            this.btnDosyaSec.Text = "📂  Dosya Seç";
            this.btnDosyaSec.Click += new System.EventHandler(this.btnDosyaSec_Click);
            // 
            // labelControlDosyaYolu
            // 
            this.labelControlDosyaYolu.Location = new System.Drawing.Point(10, 28);
            this.labelControlDosyaYolu.Name = "labelControlDosyaYolu";
            this.labelControlDosyaYolu.Size = new System.Drawing.Size(57, 13);
            this.labelControlDosyaYolu.TabIndex = 0;
            this.labelControlDosyaYolu.Text = "Dosya Yolu:";
            // 
            // textEditDosyaYolu
            // 
            this.textEditDosyaYolu.Location = new System.Drawing.Point(10, 44);
            this.textEditDosyaYolu.Name = "textEditDosyaYolu";
            this.textEditDosyaYolu.Properties.ReadOnly = true;
            this.textEditDosyaYolu.Size = new System.Drawing.Size(266, 20);
            this.textEditDosyaYolu.TabIndex = 1;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.gridControl);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(1100, 680);
            this.panelRight.TabIndex = 0;
            // 
            // gridControl
            // 
            this.gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl.Location = new System.Drawing.Point(2, 2);
            this.gridControl.MainView = this.gridView;
            this.gridControl.Name = "gridControl";
            this.gridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit,
            this.repositoryItemHata});
            this.gridControl.Size = new System.Drawing.Size(1096, 676);
            this.gridControl.TabIndex = 0;
            this.gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            // 
            // gridView
            // 
            this.gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSec,
            this.colSatirNo,
            this.colMalzemeKodu,
            this.colAciklama,
            this.colBarkod,
            this.colOzelKod1,
            this.colOzelKod2,
            this.colOzelKod3,
            this.colOzelKod4,
            this.colOzelKod5,
            this.colMarka,
            this.colModel,
            this.colDurum,
            this.colHataMesaji});
            this.gridView.GridControl = this.gridControl;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.AutoSelectAllInEditor = false;
            this.gridView.OptionsSelection.CheckBoxSelectorField = "Sec";
            this.gridView.OptionsSelection.MultiSelect = true;
            this.gridView.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            this.gridView.OptionsView.ShowAutoFilterRow = true;
            this.gridView.OptionsView.ShowFooter = true;
            this.gridView.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridView_RowCellStyle);
            this.gridView.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.GridView_ShowingEditor);
            // 
            // colSec
            // 
            this.colSec.Caption = "Seç";
            this.colSec.ColumnEdit = this.repositoryItemCheckEdit;
            this.colSec.FieldName = "Sec";
            this.colSec.Name = "colSec";
            this.colSec.Visible = true;
            this.colSec.VisibleIndex = 1;
            this.colSec.Width = 40;
            // 
            // repositoryItemCheckEdit
            // 
            this.repositoryItemCheckEdit.Name = "repositoryItemCheckEdit";
            // 
            // colSatirNo
            // 
            this.colSatirNo.Caption = "#";
            this.colSatirNo.FieldName = "SatirNo";
            this.colSatirNo.Name = "colSatirNo";
            this.colSatirNo.OptionsColumn.AllowEdit = false;
            this.colSatirNo.Visible = true;
            this.colSatirNo.VisibleIndex = 2;
            this.colSatirNo.Width = 40;
            // 
            // colMalzemeKodu
            // 
            this.colMalzemeKodu.Caption = "Malzeme Kodu";
            this.colMalzemeKodu.FieldName = "MalzemeKodu";
            this.colMalzemeKodu.Name = "colMalzemeKodu";
            this.colMalzemeKodu.OptionsColumn.AllowEdit = false;
            this.colMalzemeKodu.Visible = true;
            this.colMalzemeKodu.VisibleIndex = 3;
            this.colMalzemeKodu.Width = 140;
            // 
            // colAciklama
            // 
            this.colAciklama.Caption = "Açıklama";
            this.colAciklama.FieldName = "Aciklama";
            this.colAciklama.Name = "colAciklama";
            this.colAciklama.OptionsColumn.AllowEdit = false;
            this.colAciklama.Visible = true;
            this.colAciklama.VisibleIndex = 4;
            this.colAciklama.Width = 200;
            // 
            // colBarkod
            // 
            this.colBarkod.Caption = "Barkod";
            this.colBarkod.FieldName = "Barkod";
            this.colBarkod.Name = "colBarkod";
            this.colBarkod.OptionsColumn.AllowEdit = false;
            this.colBarkod.Visible = true;
            this.colBarkod.VisibleIndex = 5;
            this.colBarkod.Width = 120;
            // 
            // colOzelKod1
            // 
            this.colOzelKod1.Caption = "Özel Kod 1";
            this.colOzelKod1.FieldName = "OzelKod1";
            this.colOzelKod1.Name = "colOzelKod1";
            this.colOzelKod1.OptionsColumn.AllowEdit = false;
            this.colOzelKod1.Visible = true;
            this.colOzelKod1.VisibleIndex = 6;
            this.colOzelKod1.Width = 90;
            // 
            // colOzelKod2
            // 
            this.colOzelKod2.Caption = "Özel Kod 2";
            this.colOzelKod2.FieldName = "OzelKod2";
            this.colOzelKod2.Name = "colOzelKod2";
            this.colOzelKod2.OptionsColumn.AllowEdit = false;
            this.colOzelKod2.Visible = true;
            this.colOzelKod2.VisibleIndex = 7;
            this.colOzelKod2.Width = 90;
            // 
            // colOzelKod3
            // 
            this.colOzelKod3.Caption = "Özel Kod 3";
            this.colOzelKod3.FieldName = "OzelKod3";
            this.colOzelKod3.Name = "colOzelKod3";
            this.colOzelKod3.OptionsColumn.AllowEdit = false;
            this.colOzelKod3.Visible = true;
            this.colOzelKod3.VisibleIndex = 8;
            this.colOzelKod3.Width = 90;
            // 
            // colOzelKod4
            // 
            this.colOzelKod4.Caption = "Özel Kod 4";
            this.colOzelKod4.FieldName = "OzelKod4";
            this.colOzelKod4.Name = "colOzelKod4";
            this.colOzelKod4.OptionsColumn.AllowEdit = false;
            this.colOzelKod4.Visible = true;
            this.colOzelKod4.VisibleIndex = 9;
            this.colOzelKod4.Width = 90;
            // 
            // colOzelKod5
            // 
            this.colOzelKod5.Caption = "Özel Kod 5";
            this.colOzelKod5.FieldName = "OzelKod5";
            this.colOzelKod5.Name = "colOzelKod5";
            this.colOzelKod5.OptionsColumn.AllowEdit = false;
            this.colOzelKod5.Visible = true;
            this.colOzelKod5.VisibleIndex = 10;
            this.colOzelKod5.Width = 90;
            // 
            // colMarka
            // 
            this.colMarka.Caption = "Marka";
            this.colMarka.FieldName = "Marka";
            this.colMarka.Name = "colMarka";
            this.colMarka.OptionsColumn.AllowEdit = false;
            this.colMarka.Visible = true;
            this.colMarka.VisibleIndex = 11;
            this.colMarka.Width = 100;
            // 
            // colModel
            // 
            this.colModel.Caption = "Model";
            this.colModel.FieldName = "Model";
            this.colModel.Name = "colModel";
            this.colModel.OptionsColumn.AllowEdit = false;
            this.colModel.Visible = true;
            this.colModel.VisibleIndex = 12;
            this.colModel.Width = 100;
            // 
            // colDurum
            // 
            this.colDurum.Caption = "Durum";
            this.colDurum.FieldName = "Durum";
            this.colDurum.Name = "colDurum";
            this.colDurum.OptionsColumn.AllowEdit = false;
            this.colDurum.Visible = true;
            this.colDurum.VisibleIndex = 13;
            this.colDurum.Width = 130;
            // 
            // colHataMesaji
            // 
            this.colHataMesaji.Caption = "Hata / Bilgi";
            this.colHataMesaji.ColumnEdit = this.repositoryItemHata;
            this.colHataMesaji.FieldName = "HataMesaji";
            this.colHataMesaji.Name = "colHataMesaji";
            this.colHataMesaji.OptionsColumn.AllowEdit = false;
            this.colHataMesaji.Visible = true;
            this.colHataMesaji.VisibleIndex = 14;
            this.colHataMesaji.Width = 250;
            // 
            // repositoryItemHata
            // 
            this.repositoryItemHata.Name = "repositoryItemHata";
            // 
            // ProductUpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 700);
            this.Controls.Add(this.splitContainerControl);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("ProductUpdateForm.IconOptions.Image")));
            this.KeyPreview = true;
            this.Name = "ProductUpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Malzeme Güncelleme";
            this.Load += new System.EventHandler(this.ProductUpdateForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProductUpdateForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.barManager)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl)).EndInit();
            this.splitContainerControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).EndInit();
            this.panelLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControlIslemler)).EndInit();
            this.groupControlIslemler.ResumeLayout(false);
            this.groupControlIslemler.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlFiltre)).EndInit();
            this.groupControlFiltre.ResumeLayout(false);
            this.groupControlFiltre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSadecHata.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSadecGuncellenecek.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlDosya)).EndInit();
            this.groupControlDosya.ResumeLayout(false);
            this.groupControlDosya.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditDosyaYolu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelRight)).EndInit();
            this.panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHata)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // ─── Controls ─────────────────────────────────────────────────────────
        private DevExpress.XtraBars.BarManager barManager;
        private DevExpress.XtraBars.Bar barTop;
        private DevExpress.XtraBars.BarButtonItem barItemDosyaSec;
        private DevExpress.XtraBars.BarButtonItem barItemTemizle;
        private DevExpress.XtraBars.BarButtonItem barItemSablon;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl;
        private DevExpress.XtraEditors.PanelControl panelLeft;

        private DevExpress.XtraEditors.GroupControl groupControlIslemler;
        private DevExpress.XtraEditors.SimpleButton btnTumunuSec;
        private DevExpress.XtraEditors.SimpleButton btnSecimiTemizle;
        private DevExpress.XtraEditors.SimpleButton btnGuncelle;
        private DevExpress.XtraEditors.ProgressBarControl progressBar;
        private DevExpress.XtraEditors.LabelControl labelDurum;

        private DevExpress.XtraEditors.GroupControl groupControlFiltre;
        private DevExpress.XtraEditors.CheckEdit checkEditSadecHata;
        private DevExpress.XtraEditors.CheckEdit checkEditSadecGuncellenecek;
        private DevExpress.XtraEditors.LabelControl labelToplam;
        private DevExpress.XtraEditors.LabelControl labelSecili;
        private DevExpress.XtraEditors.LabelControl labelHata;

        private DevExpress.XtraEditors.GroupControl groupControlDosya;
        private DevExpress.XtraEditors.LabelControl labelControlDosyaYolu;
        private DevExpress.XtraEditors.TextEdit textEditDosyaYolu;

        private DevExpress.XtraEditors.PanelControl panelRight;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;

        private DevExpress.XtraGrid.Columns.GridColumn colSec;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit;
        private DevExpress.XtraGrid.Columns.GridColumn colSatirNo;
        private DevExpress.XtraGrid.Columns.GridColumn colMalzemeKodu;
        private DevExpress.XtraGrid.Columns.GridColumn colAciklama;
        private DevExpress.XtraGrid.Columns.GridColumn colBarkod;
        private DevExpress.XtraGrid.Columns.GridColumn colOzelKod1;
        private DevExpress.XtraGrid.Columns.GridColumn colOzelKod2;
        private DevExpress.XtraGrid.Columns.GridColumn colOzelKod3;
        private DevExpress.XtraGrid.Columns.GridColumn colOzelKod4;
        private DevExpress.XtraGrid.Columns.GridColumn colOzelKod5;
        private DevExpress.XtraGrid.Columns.GridColumn colMarka;
        private DevExpress.XtraGrid.Columns.GridColumn colModel;
        private DevExpress.XtraGrid.Columns.GridColumn colDurum;
        private DevExpress.XtraGrid.Columns.GridColumn colHataMesaji;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemHata;
        private DevExpress.XtraEditors.SimpleButton btnDosyaSec;
    }
}