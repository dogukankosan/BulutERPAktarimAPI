// LoginForm.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BulutERPAktarim.Classes;

namespace BulutERPAktarim.Forms
{
    public partial class LoginForm : XtraForm
    {
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private static LicenseResult _cachedLicense = null;
        public LoginForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            AttachDragEvents();
            txtKullaniciAdi.Focus();
        }
        // ── Form Events ──────────────────────────────────────────────
        private async void LoginForm_Load(object sender, EventArgs e)
        {
            txtKullaniciAdi.Focus();
            await CheckLicenseOnStartupAsync();
        }
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        // ── Lisans Kontrolü ──────────────────────────────────────────
        private async Task CheckLicenseOnStartupAsync()
        {
            try
            {
                SetFormEnabled(false);
                SetLicenseLabel("🔄 Lisans doğrulanıyor...", Color.FromArgb(127, 140, 141));
                if (_cachedLicense != null && _cachedLicense.IsValid
                    && _cachedLicense.ExpiryDate.HasValue
                    && _cachedLicense.ExpiryDate.Value > DateTime.UtcNow)
                {
                    ShowLicenseStatus(_cachedLicense);
                    SetFormEnabled(true);
                    txtKullaniciAdi.Focus();
                    return;
                }
                LicenseResult result = await LicenseHelper.CheckLicenseAsync();
                _cachedLicense = result;
                if (!result.IsValid)
                {
                    SetLicenseLabel($"⛔ {result.Message}", Color.FromArgb(192, 57, 43));
                    string hwid = HwidHelper.GetHwid();
                    string detay = result.Message.Contains("bulunamadı")
                        ? $"⛔ Lisans Hatası\n\n{result.Message}\n\n" +
                          $"Cihaz Kimliği (HWID):\n{hwid}\n\n" +
                          $"Bu kodu yetkili ile paylaşarak lisans aktivasyonu yaptırınız."
                        : $"⛔ Lisans Hatası\n\n{result.Message}\n\nLütfen yetkili ile iletişime geçin.";
                    if (result.Message.Contains("bulunamadı"))
                        Clipboard.SetText(hwid);
                    XtraMessageBox.Show(detay, "Lisans Geçersiz", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
                ShowLicenseStatus(result);
                SetFormEnabled(true);
                txtKullaniciAdi.Focus();
                if (result.ExpiryDate.HasValue)
                {
                    int daysLeft = (result.ExpiryDate.Value - DateTime.UtcNow).Days;
                    if (daysLeft <= 30)
                    {
                        XtraMessageBox.Show(
                            $"⚠ Lisansınızın bitmesine {daysLeft} gün kaldı.\nLütfen yenileme işlemini yapınız.",
                            "Lisans Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Lisans kontrol hatası: {ex.Message}");
                SetLicenseLabel("⛔ Lisans kontrol hatası!", Color.FromArgb(192, 57, 43));
                XtraMessageBox.Show($"Lisans kontrol edilirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private void ShowLicenseStatus(LicenseResult result)
        {
            if (!result.IsValid)
            {
                SetLicenseLabel($"⛔ {result.Message}", Color.FromArgb(192, 57, 43));
                return;
            }
            if (result.ExpiryDate.HasValue)
            {
                int daysLeft = (result.ExpiryDate.Value - DateTime.UtcNow).Days;
                if (daysLeft <= 7)
                    SetLicenseLabel($"⚠ Lisans {daysLeft} gün sonra bitiyor! ({result.ExpiryDate.Value:dd.MM.yyyy})", Color.FromArgb(192, 57, 43));
                else if (daysLeft <= 30)
                    SetLicenseLabel($"⚠ Lisans {daysLeft} gün sonra bitiyor ({result.ExpiryDate.Value:dd.MM.yyyy})", Color.FromArgb(211, 84, 0));
                else
                    SetLicenseLabel($"✔ Lisans geçerli — {result.ExpiryDate.Value:dd.MM.yyyy} tarihine kadar", Color.FromArgb(39, 174, 96));
            }
            else
            {
                SetLicenseLabel("✔ Lisans geçerli", Color.FromArgb(39, 174, 96));
            }
        }
        private void SetLicenseLabel(string text, Color color)
        {
            lblLicenseStatus.Text = text;
            lblLicenseStatus.Appearance.ForeColor = color;
            lblLicenseStatus.Appearance.Options.UseForeColor = true;
            lblLicenseStatus.Visible = true;
        }
        private void SetFormEnabled(bool enabled)
        {
            btnGiris.Enabled = enabled;
            txtKullaniciAdi.Enabled = enabled;
            txtSifre.Enabled = enabled;
        } 
        private void btnGiris_Click(object sender, EventArgs e) => GirisYap();
        private async void GirisYap()
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text.Trim();
            if (string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                XtraMessageBox.Show("Lütfen kullanıcı adı giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKullaniciAdi.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(sifre))
            {
                XtraMessageBox.Show("Lütfen şifre giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSifre.Focus();
                return;
            }
           string encryptedUsername = EncryptionHelper.Encrypt(kullaniciAdi);
            try
            {
                btnGiris.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                string query = "SELECT Password FROM Users WHERE Username = @username LIMIT 1";
                Dictionary<string, object> parameters = new Dictionary<string, object> { { "@username", encryptedUsername } };
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSifre.Text = "";
                    txtSifre.Focus();
                    return;
                }
                string decryptedPassword = EncryptionHelper.Decrypt(dt.Rows[0]["Password"].ToString());
                if (decryptedPassword == sifre)
                {
                    this.Hide();
                    HomeForm homeForm = new HomeForm();
                    homeForm.Show();
                }
                else
                {
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSifre.Text = "";
                    txtSifre.Focus();
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Login hatası: {ex.Message}");
                XtraMessageBox.Show($"Giriş sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGiris.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
        // ── Form Sürükleme ───────────────────────────────────────────
        private void AttachDragEvents()
        {
            panelControl1.MouseDown += Form_MouseDown;
            panelControl1.MouseMove += Form_MouseMove;
            panelControl1.MouseUp += Form_MouseUp;
            panelControl2.MouseDown += Form_MouseDown;
            panelControl2.MouseMove += Form_MouseMove;
            panelControl2.MouseUp += Form_MouseUp;
            if (panelTop != null)
            {
                panelTop.MouseDown += Form_MouseDown;
                panelTop.MouseMove += Form_MouseMove;
                panelTop.MouseUp += Form_MouseUp;
            }
            foreach (Control ctrl in panelControl1.Controls)
            {
                if (ctrl is LabelControl)
                {
                    ctrl.MouseDown += Form_MouseDown;
                    ctrl.MouseMove += Form_MouseMove;
                    ctrl.MouseUp += Form_MouseUp;
                }
            }
        }
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { dragging = true; dragCursorPoint = Cursor.Position; dragFormPoint = this.Location; }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging) this.Location = Point.Add(dragFormPoint, new Size(Point.Subtract(Cursor.Position, new Size(dragCursorPoint))));
        }
        private void Form_MouseUp(object sender, MouseEventArgs e) => dragging = false;
        private void hyperlinkLabelControl1_Click(object sender, EventArgs e)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "https://mutluyazilim.com.tr/", UseShellExecute = true }); } catch { }
        }
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) GirisYap();
            else if (e.KeyCode == Keys.Escape) Application.Exit();
        }
        private void btn_Close_Click(object sender, EventArgs e) => Application.Exit();
        private void btn_Hide_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
    }
}