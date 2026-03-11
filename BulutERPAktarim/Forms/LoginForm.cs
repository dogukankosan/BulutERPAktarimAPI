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
        // Form sürükleme için değişkenler
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        public LoginForm()
        {
            InitializeComponent(); txtKullaniciAdi.Focus();
            this.KeyPreview = true;
            AttachDragEvents();
            txtKullaniciAdi.Focus();
        }
        private void AttachDragEvents()
        {
            // Sol panel için
            panelControl1.MouseDown += Form_MouseDown;
            panelControl1.MouseMove += Form_MouseMove;
            panelControl1.MouseUp += Form_MouseUp;
            // Sağ panel için
            panelControl2.MouseDown += Form_MouseDown;
            panelControl2.MouseMove += Form_MouseMove;
            panelControl2.MouseUp += Form_MouseUp;
            // Üst panel için
            if (panelTop != null)
            {
                panelTop.MouseDown += Form_MouseDown;
                panelTop.MouseMove += Form_MouseMove;
                panelTop.MouseUp += Form_MouseUp;
            }
            // Label'lar için
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
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtKullaniciAdi.Focus();
        }
        private void btnGiris_Click(object sender, EventArgs e)
        {
            GirisYap();
        }
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
            // Kullanıcı adını şifrele
            string encryptedUsername = EncryptionHelper.Encrypt(kullaniciAdi);
            try
            {
                btnGiris.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                string query = "SELECT Password FROM Users WHERE Username = @username LIMIT 1";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@username", encryptedUsername }
                };
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSifre.Text = "";
                    txtSifre.Focus();
                    return;
                }
                string encryptedPassword = dt.Rows[0]["Password"].ToString();
                string decryptedPassword = EncryptionHelper.Decrypt(encryptedPassword);
                if (decryptedPassword == sifre)
                {
                    // Giriş başarılı - HomeForm aç
                    this.Hide();
                    HomeForm homeForm = new HomeForm();
                    homeForm.ShowDialog();
                    // HomeForm kapandığında LoginForm'u da kapat
                    this.Close();
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
        private void hyperlinkLabelControl1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://mutluyazilim.com.tr/",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore
            }
        }
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                GirisYap();
            else if (e.KeyCode == Keys.Escape)
                Application.Exit();
        }
        private void btn_Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btn_Hide_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}