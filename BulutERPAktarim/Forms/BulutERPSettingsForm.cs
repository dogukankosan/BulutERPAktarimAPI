using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BulutERPAktarim.Classes;
using BulutERPAktarim.Business;

namespace BulutERPAktarim.Forms
{
    public partial class BulutERPSettingsForm : XtraForm
    {
        public BulutERPSettingsForm()
        {
            InitializeComponent();
        }
        // ── Form Load ────────────────────────────────────────────────
        private async void BulutERPSettingsForm_Load(object sender, EventArgs e)
        {
            await LoadExistingSettingsAsync();
        }
        /// <summary>
        /// Mevcut ayarları yükle
        /// </summary>
        private async Task LoadExistingSettingsAsync()
        {
            try
            {
                var result = await BulutERPConnectionTest.GetSettingsAsync();
                if (result.Success && result.Settings != null)
                {
                    textEditClientId.Text = result.Settings.ClientId;
                    textEditClientSecret.Text = result.Settings.ClientSecret;
                    textEditUsername.Text = result.Settings.Username;
                    textEditPassword.Text = result.Settings.Password;
                    textEditFirmNr.Text = result.Settings.FirmNr;
                    textEditServerUrl.Text = result.Settings.ServerUrl;
                    textEditMachineId.Text = result.Settings.MachineID;
                    SetStatus("✓ Ayarlar kayıtlı - Değişiklik yapmak için test edin", Color.Green);
                }
                else
                {
                    SetStatus("⚠ Ayar bulunamadı - Yeni ayar giriniz", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"BulutERPSettings Form Load hatası: {ex.Message}");
                XtraMessageBox.Show($"Ayarlar yüklenirken hata oluştu:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ── Bağlantıyı Test Et ───────────────────────────────────────
        private async void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                btnTest.Enabled = false;
                btnTest.Text = "Test ediliyor...";
                string clientId = textEditClientId.Text.Trim();
                string clientSecret = textEditClientSecret.Text.Trim();
                string username = textEditUsername.Text.Trim();
                string password = textEditPassword.Text.Trim();
                string firmNr = textEditFirmNr.Text.Trim();
                string serverUrl = textEditServerUrl.Text.Trim();
                string machineId = textEditMachineId.Text.Trim();
                // 1. VALİDASYON
                var validation = BulutERPValidator.ValidateSettings(
                    clientId, clientSecret, username, password, firmNr, serverUrl, machineId);
                if (!validation.IsValid)
                {
                    ResetTestButton();
                    SetStatus("✗ Validasyon hatası", Color.Red);
                    XtraMessageBox.Show(validation.ErrorMessage, "Validasyon Hatası",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // 2. GEÇİCİ KAYDET
                btnTest.Text = "Kaydediliyor...";
                var saveResult = await BulutERPConnectionTest.SaveSettingsAsync(
                    clientId, clientSecret, username, password, firmNr, serverUrl, machineId);
                if (!saveResult.Success)
                {
                    ResetTestButton();
                    SetStatus("✗ Kaydetme hatası", Color.Red);
                    XtraMessageBox.Show($"Ayarlar kaydedilemedi:\n{saveResult.ErrorMessage}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // 3. BAĞLANTIYI TEST ET
                btnTest.Text = "Bağlantı test ediliyor...";
                var testResult = await BulutERPConnectionTest.TestConnectionAsync();
                ResetTestButton();
                if (testResult.Success)
                {
                    SetStatus("✓ Bağlantı başarılı - Ayarlar kaydedildi", Color.Green);
                    XtraMessageBox.Show(
                        "✓ Bağlantı testi başarılı!\n✓ Token alındı ve kaydedildi!\n✓ Test sorgusu çalıştı!\n✓ Ayarlar şifreli olarak kaydedildi!",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SetStatus("✗ Bağlantı başarısız", Color.Red);
                    XtraMessageBox.Show(
                        $"Bağlantı testi başarısız!\n\n{testResult.ErrorMessage}\n\nAyarlar kaydedilmedi. Lütfen bilgileri kontrol edip tekrar test edin.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ResetTestButton();
                await TextLog.LogToSQLiteAsync($"BulutERPSettings Test hatası: {ex.Message}");
                XtraMessageBox.Show($"Test hatası:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ── İptal ────────────────────────────────────────────────────
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // ── Alan Değişikliği ─────────────────────────────────────────
        private void OnTextChanged(object sender, EventArgs e)
        {
            SetStatus("⚠ Değişiklikler test edilmedi", Color.Orange);
        }
        // ── Helpers ──────────────────────────────────────────────────
        private void SetStatus(string message, Color color)
        {
            labelControlStatus.Text = message;
            labelControlStatus.Appearance.ForeColor = color;
        }
        private void ResetTestButton()
        {
            btnTest.Enabled = true;
            btnTest.Text = "Bağlantıyı Test Et";
        }
    }
}