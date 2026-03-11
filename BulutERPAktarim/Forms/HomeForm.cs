// HomeForm.cs
using System;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.LookAndFeel;
using BulutERPAktarim.Classes;

namespace BulutERPAktarim.Forms
{
    public partial class HomeForm : FluentDesignForm
    {
        public HomeForm()
        {
            InitializeComponent();
            // Tema değişikliğini dinle — kullanıcı tema seçer seçmez otomatik kaydet
            UserLookAndFeel.Default.StyleChanged += UserLookAndFeel_StyleChanged;
        }
        // ── Form Events ──────────────────────────────────────────────
        private async void HomeForm_Load(object sender, EventArgs e)
        {
            // Versiyon bilgisi
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            barItemVersion.Caption = $"v{version.Major}.{version.Minor}.{version.Build}";
            // Kullanıcının kayıtlı temasını yükle
            await ThemeManager.LoadUserThemeAsync();
        }
        private void HomeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        // ── Tema ─────────────────────────────────────────────────────
        /// <summary>
        /// Kullanıcı tema seçtiğinde otomatik olarak SQLite'a kaydet
        /// </summary>
        private async void UserLookAndFeel_StyleChanged(object sender, EventArgs e)
        {
            try
            {
                string currentTheme = UserLookAndFeel.Default.ActiveSkinName;
                await ThemeManager.SaveUserThemeAsync(currentTheme);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"Tema kaydetme hatası: {ex.Message}");
            }
        }
        // ── Helpers ──────────────────────────────────────────────────
        private void LoadForm(Form form)
        {
            panelControlMain.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelControlMain.Controls.Add(form);
            form.Show();
        }
        // ── İşlemler ─────────────────────────────────────────────────
        private void AccordionElementMaterialTransfer_Click(object sender, EventArgs e)
        {
            ProductSendForm frm = new ProductSendForm();
            frm.ShowDialog();
        }
        // ── Ayarlar ──────────────────────────────────────────────────
        private void AccordionElementCloudERPSettings_Click(object sender, EventArgs e)
        {
            BulutERPSettingsForm frm = new BulutERPSettingsForm();
            frm.ShowDialog();
        }
        private void AccordionElementThemeSettings_Click(object sender, EventArgs e)
        {
            // DevExpress popup tema seçici menüsünü cursor pozisyonunda göster
            popupMenuTheme.ShowPopup(Cursor.Position);
        }
        private void AccordionElementErrorLogs_Click(object sender, EventArgs e)
        {
            ErrorListForm frm = new ErrorListForm();
            frm.ShowDialog();
        }
        private void AccordionElementSQLiteSettings_Click(object sender, EventArgs e)
        {
            SQLiteForm frm = new SQLiteForm();
            frm.ShowDialog();
        }
        // ── Hakkımızda ───────────────────────────────────────────────
        private void AccordionElementAbout_Click(object sender, EventArgs e)
        {
            AboutForm frm = new AboutForm();
            frm.ShowDialog();
        }
    }
}