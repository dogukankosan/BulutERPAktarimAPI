using BulutERPAktarim.Classes;
using BulutERPAktarim.Models;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulutERPAktarim.Forms
{
    public partial class PriceAddForm : XtraForm
    {
        // ─── Sabitler ─────────────────────────────────────────────────────────
        private const int COL_MALZEME_KODU = 0;
        private const int COL_MALZEME_ACIKLAMA = 1;
        private const int COL_FIYAT_KODU = 2;
        private const int COL_ONEM_DERECESI = 3;
        private const int COL_FIYAT = 4;
        private const int EXCEL_MIN_COL = 5;
        // ─── Alanlar ──────────────────────────────────────────────────────────
        private static readonly HttpClient _http = new HttpClient();
        private List<FiyatEkleRow> _tumListe = new List<FiyatEkleRow>();
        private List<FiyatEkleRow> _filtreliListe = new List<FiyatEkleRow>();
        private CancellationTokenSource _cts;
        private bool _islemDevam = false;
        // ─── Constructor ──────────────────────────────────────────────────────
        public PriceAddForm()
        {
            InitializeComponent();
            InitializeForm();
        }
        private void InitializeForm()
        {
            checkEditSadecHata.Checked = false;
            checkEditSadecEklenecek.Checked = false;
            btnEkle.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            progressBar.EditValue = 0;
            ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
            SetDurum("Hazır", false);
        }
        // ─── Form Olayları ─────────────────────────────────────────────────────
        private void PriceAddForm_Load(object sender, EventArgs e)
        {
            gridView.RowCellStyle += GridView_RowCellStyle;
            gridView.ShowingEditor += GridView_ShowingEditor;
        }
        private void PriceAddForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }
        // ─── Dosya Seç ────────────────────────────────────────────────────────
        private void BarItemDosyaSec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Excel Dosyası Seç";
                dlg.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|Tüm Dosyalar (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textEditDosyaYolu.Text = dlg.FileName;
                    SetDurum($"Dosya seçildi: {Path.GetFileName(dlg.FileName)}", false);
                    YukleExcel(dlg.FileName);
                }
            }
        }
        // ─── Excel Yükle ──────────────────────────────────────────────────────
        private void YukleExcel(string dosyaYolu)
        {
            try
            {
                SetDurum("Excel yükleniyor...", false);
                _tumListe.Clear();
                int hataliSatir = 0;
                if (!File.Exists(dosyaYolu))
                {
                    XtraMessageBox.Show("Dosya bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (ExcelPackage package = new ExcelPackage(new FileInfo(dosyaYolu)))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.FirstOrDefault();
                    if (ws == null)
                    {
                        XtraMessageBox.Show("Excel dosyasında sayfa bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int rowCount = ws.Dimension?.Rows ?? 0;
                    int colCount = ws.Dimension?.Columns ?? 0;
                    // Şablon sabit format: 1. satır açıklama, 2. satır başlık, 3. satırdan veri
                    int veriBaslangic = 3;
                    if (colCount < EXCEL_MIN_COL)
                    {
                        XtraMessageBox.Show(
                            $"Excel formatı hatalı! Beklenen minimum sütun: {EXCEL_MIN_COL}\n\n" +
                            "Beklenen format:\nMalzeme Kodu | Malzeme Açıklaması | Fiyat Kart Kodu | Önem Derecesi | Fiyat",
                            "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int satirNo = 0;
                    for (int row = veriBaslangic; row <= rowCount; row++)
                    {
                        string malzemeKodu = GetCell(ws, row, COL_MALZEME_KODU + 1);
                        if (string.IsNullOrWhiteSpace(malzemeKodu)) continue;
                        satirNo++;
                        FiyatEkleRow item = new FiyatEkleRow
                        {
                            SatirNo = satirNo,
                            MalzemeKodu = malzemeKodu.Trim(),
                            MalzemeAciklamasi = GetCell(ws, row, COL_MALZEME_ACIKLAMA + 1),
                            FiyatKodu = GetCell(ws, row, COL_FIYAT_KODU + 1),
                            OnemDerecesi = (int)ParseDecimal(ws, row, COL_ONEM_DERECESI + 1),
                            Fiyat = ParseDecimal(ws, row, COL_FIYAT + 1),
                            Durum = "Bekliyor",
                            Sec = true
                        };
                        List<string> hatalar = new List<string>();
                        if (string.IsNullOrWhiteSpace(item.MalzemeKodu)) hatalar.Add("Malzeme kodu boş");
                        if (string.IsNullOrWhiteSpace(item.FiyatKodu)) hatalar.Add("Fiyat kart kodu boş");
                        if (item.Fiyat <= 0) hatalar.Add("Fiyat 0 veya negatif");
                        if (hatalar.Any())
                        {
                            item.HataMesaji = string.Join(" | ", hatalar);
                            item.Durum = "Format Hatası";
                            item.Sec = false;
                            hataliSatir++;
                        }
                        _tumListe.Add(item);
                    }
                }
                UygulaFiltre();
                _ = LogoKontrolAsync(_tumListe.ToList());
                GuncelleSayaclar();
                btnEkle.Enabled = false;
                btnTumunuSec.Enabled = true;
                btnSecimiTemizle.Enabled = true;
                SetDurum($"Excel yüklendi: {_tumListe.Count} satır", false);
                if (hataliSatir > 0)
                    XtraMessageBox.Show(
                        $"{_tumListe.Count} satırdan {hataliSatir} tanesi format hatası içeriyor.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Excel yükleme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetDurum("Excel yükleme hatası!", true);
                _ = TextLog.LogToSQLiteAsync($"❌ PriceAddForm Excel yükleme: {ex.Message}");
            }
        }
        // ─── Logo Kontrol ─────────────────────────────────────────────────────
        private async Task LogoKontrolAsync(List<FiyatEkleRow> liste)
        {
            try
            {
                SetDurum("Logo'dan kontrol ediliyor...", false);
                progressBar.Properties.Maximum = liste.Count;
                progressBar.EditValue = 0;
                btnEkle.Enabled = false;
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    SetDurum($"Token alınamadı: {tokenResult.ErrorMessage}", true);
                    return;
                }
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success) return;
                var settings = settingsResult.Settings;
                string token = tokenResult.AccessToken;
                SemaphoreSlim semaphore = new SemaphoreSlim(5, 5);
                int tamamlanan = 0;
                var tasks = liste.Select(async row =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        // 1. Malzeme var mı + LOGICALREF çek
                        var (malzemeOk, malzemeRef, malzemeErr) =
                            await GetMalzemeRefAsync(token, row.MalzemeKodu);
                        if (!malzemeOk)
                        {
                            row.Durum = "Kontrol Hatası";
                            row.HataMesaji = malzemeErr;
                            row.Sec = false;
                            return;
                        }
                        if (malzemeRef <= 0)
                        {
                            row.Durum = "Malzeme Yok";
                            row.HataMesaji = $"'{row.MalzemeKodu}' Logo'da bulunamadı";
                            row.Sec = false;
                            return;
                        }
                        row.MalzemeRef = malzemeRef;
                        row.MalzemeVarMi = true;
                        // 2. Fiyat kartı var mı?
                        var (kartOk, kartVar, kartErr) =
                            await CheckFiyatKartiVarMiAsync(settings, token, row.MalzemeKodu, row.FiyatKodu);
                        if (!kartOk)
                        {
                            row.Durum = "Kontrol Hatası";
                            row.HataMesaji = kartErr;
                            row.Sec = false;
                            return;
                        }
                        // LogoKontrolAsync içinde kartVar kontrolü
                        if (kartVar)
                        {
                            row.KartZatenVar = true;
                            row.Durum = "Kart Zaten Var";
                            row.HataMesaji = $"'{row.FiyatKodu}' fiyat kodu zaten kayıtlı";
                            row.Sec = false;
                            return;
                        }
                        row.LogoKontrolEdildi = true;
                        row.Durum = "Eklenecek";
                    }
                    catch (Exception ex)
                    {
                        row.Durum = "Kontrol Hatası";
                        row.HataMesaji = ex.Message;
                        row.Sec = false;
                        await TextLog.LogToSQLiteAsync($"❌ Kontrol hatası | {row.MalzemeKodu} | {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                        int done = Interlocked.Increment(ref tamamlanan);
                        Invoke(new Action(() =>
                        {
                            progressBar.EditValue = done;
                            SetDurum($"Logo kontrolü... {done}/{liste.Count}", false);
                            gridView.RefreshData();
                            GuncelleSayaclar();
                        }));
                    }
                });
                await Task.WhenAll(tasks);
                progressBar.EditValue = 0;
                int eklenecek = liste.Count(x => x.Durum == "Eklenecek");
                int malzemeYok = liste.Count(x => x.Durum == "Malzeme Yok");
                int kartZatenVar = liste.Count(x => x.Durum == "Kart Zaten Var");
                SetDurum($"Kontrol tamamlandı — Eklenecek: {eklenecek} | Malzeme Yok: {malzemeYok} | Kart Var: {kartZatenVar}", false);
            }
            catch (Exception ex)
            {
                SetDurum($"Logo kontrol hatası: {ex.Message}", true);
                await TextLog.LogToSQLiteAsync($"❌ LogoKontrol hatası | {ex.Message}");
            }
            finally
            {
                btnEkle.Enabled = _tumListe.Any(x => x.IsValid && x.Sec);
            }
        }
        // ─── Malzeme LOGICALREF çek ───────────────────────────────────────────
        private async Task<(bool Success, int MalzemeRef, string ErrorMessage)> GetMalzemeRefAsync(
            string token, string malzemeKodu)
        {
            try
            {
                string sql = $"SELECT LOGICALREF FROM U_$V(firm)_ITEMS WHERE CODE = '{malzemeKodu.Replace("'", "''")}'";
                var result = await BulutERPService.ExecuteSelectQueryAsync(sql, token, 1);
                if (!result.Success)
                    return (false, 0, result.ErrorMessage);
                if (result.Data == null || result.Data.Count == 0)
                    return (true, 0, null);
                int malzemeRef = Convert.ToInt32(result.Data[0]["LOGICALREF"]);
                return (true, malzemeRef, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ MalzemeRef hatası | {malzemeKodu} | {ex.Message}");
                return (false, 0, ex.Message);
            }
        }
        // ─── Fiyat kartı var mı kontrol ───────────────────────────────────────
        private async Task<(bool Success, bool Exists, string ErrorMessage)> CheckFiyatKartiVarMiAsync(
      BulutERPSettings settings, string token, string malzemeKodu, string fiyatKodu)
        {
            try
            {
                // Fiyat kodu tüm malzemelerde tekil — sadece CODE kontrolü yeterli
                string sql = $"SELECT LOGICALREF FROM U_$V(firm)_PRICES " +
                             $"WHERE CODE = '{fiyatKodu.Replace("'", "''")}'";
                var result = await BulutERPService.ExecuteSelectQueryAsync(sql, token, 1);
                if (!result.Success)
                    return (false, false, result.ErrorMessage);
                bool exists = result.Data != null && result.Data.Count > 0;
                return (true, exists, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ FiyatKartı kontrol hatası | {malzemeKodu}/{fiyatKodu} | {ex.Message}");
                return (false, false, ex.Message);
            }
        }
        // ─── Ekle Butonu ──────────────────────────────────────────────────────
        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (_islemDevam)
            {
                _cts?.Cancel();
                btnEkle.Text = "➕  Seçilenleri Ekle";
                SetDurum("İptal ediliyor...", false);
                return;
            }
            var secilenler = _tumListe
                .Where(x => x.Sec && x.IsValid)
                .ToList();
            if (!secilenler.Any())
            {
                XtraMessageBox.Show("Eklenecek seçili satır bulunamadı!\n\nLütfen önce satırları seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (XtraMessageBox.Show(
                    $"{secilenler.Count} adet fiyat kartı eklenecek.\n\nDevam edilsin mi?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _ = EkleAsync(secilenler);
        }
        private async Task EkleAsync(List<FiyatEkleRow> secilenler)
        {
            _islemDevam = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            btnEkle.Text = "⛔  İptal Et";
            barItemDosyaSec.Enabled = false;
            progressBar.Properties.Maximum = secilenler.Count;
            progressBar.EditValue = 0;
            int basarili = 0, hatali = 0;
            try
            {
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    XtraMessageBox.Show($"Token alınamadı:\n{tokenResult.ErrorMessage}",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success) return;
                var settings = settingsResult.Settings;
                string token = tokenResult.AccessToken;
                for (int i = 0; i < secilenler.Count; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    FiyatEkleRow row = secilenler[i];
                    row.Durum = "Ekleniyor...";
                    row.HataMesaji = "";
                    RefreshRow(row);
                    SetDurum($"[{i + 1}/{secilenler.Count}] {row.MalzemeKodu} / {row.FiyatKodu} ekleniyor...", false);
                    try
                    {
                        var (ok, hata) = await CreateSalesPriceAsync(settings, token, row);
                        if (ok)
                        {
                            row.Durum = "Eklendi";
                            row.HataMesaji = "";
                            basarili++;
                        }
                        else
                        {
                            row.Durum = "Hata";
                            row.HataMesaji = hata;
                            hatali++;
                            await TextLog.LogToSQLiteAsync($"❌ Eklenemedi | {row.MalzemeKodu}/{row.FiyatKodu} | {hata}");
                        }
                    }
                    catch (Exception ex)
                    {
                        row.Durum = "Hata";
                        row.HataMesaji = ex.Message;
                        hatali++;
                        await TextLog.LogToSQLiteAsync($"❌ Ekleme exception | {row.MalzemeKodu}/{row.FiyatKodu} | {ex.Message}");
                    }
                    RefreshRow(row);
                    progressBar.EditValue = i + 1;
                    GuncelleSayaclar();
                    if ((i + 1) % 5 == 0)
                    {
                        var yeniToken = await BulutERPService.EnsureValidTokenAsync();
                        if (yeniToken.Success) token = yeniToken.AccessToken;
                    }
                }
                SetDurum($"Tamamlandı — ✅ Başarılı: {basarili} | ❌ Hatalı: {hatali}", false);
                XtraMessageBox.Show(
                    $"Ekleme tamamlandı!\n\n✅ Başarılı: {basarili}\n❌ Hatalı: {hatali}",
                    "Sonuç", MessageBoxButtons.OK,
                    hatali > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                SetDurum($"İptal edildi. Başarılı: {basarili} | Hatalı: {hatali}", false);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ EkleAsync hatası | {ex.Message}");
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _islemDevam = false;
                btnEkle.Text = "➕  Seçilenleri Ekle";
                barItemDosyaSec.Enabled = true;
                GuncelleSayaclar();
            }
        }
        // ─── POST salesprice/ref ──────────────────────────────────────────────
        private async Task<(bool Success, string ErrorMessage)> CreateSalesPriceAsync(
            BulutERPSettings settings,
            string accessToken,
            FiyatEkleRow row)
        {
            try
            {
                var body = new
                {
                    priceCode = row.FiyatKodu,
                    unitPrice = row.Fiyat,
                    priority = row.OnemDerecesi
                };
                string url = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}" +
                             $"/logo/restservices/rest/v2.0/salesprice/ref" +
                             $"?logicalRef={row.MalzemeRef}";
                string bodyJson = JsonConvert.SerializeObject(body);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("access-token", accessToken);
                request.Headers.Add("firm", settings.FirmNr.Trim());
                request.Headers.Add("lang", "TRTR");
                request.Headers.Add("Accept", "application/json");
                request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _http.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, $"[{(int)response.StatusCode}] {json}");
                return (true, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateSalesPrice hatası | {row.MalzemeKodu}/{row.FiyatKodu} | {ex.Message}");
                return (false, ex.Message);
            }
        }
        // ─── Grid / Filtre ─────────────────────────────────────────────────────
        private void UygulaFiltre()
        {
            bool sadecHata = checkEditSadecHata.Checked;
            bool sadecEklenecek = checkEditSadecEklenecek.Checked;
            _filtreliListe = _tumListe.Where(x =>
            {
                bool hataUygun = !sadecHata || x.Durum == "Format Hatası" || x.Durum == "Hata"
                                             || x.Durum == "Malzeme Yok" || x.Durum == "Kart Zaten Var"
                                             || x.Durum == "Kontrol Hatası";
                bool ekUygun = !sadecEklenecek || x.Durum == "Eklenecek" || x.Durum == "Bekliyor";
                return hataUygun && ekUygun;
            }).ToList();
            gridControl.DataSource = null;
            gridControl.DataSource = _filtreliListe;
            GuncelleSayaclar();
        }
        private void CheckEditFiltre_Changed(object sender, EventArgs e) => UygulaFiltre();
        private void GuncelleSayaclar()
        {
            int toplam = _tumListe.Count;
            int secili = _tumListe.Count(x => x.Sec);
            int hata = _tumListe.Count(x =>
              x.Durum == "Format Hatası" || x.Durum == "Hata" ||
              x.Durum == "Malzeme Yok" || x.Durum == "Kart Zaten Var" ||
              x.Durum == "Kontrol Hatası");
            labelToplam.Text = $"Toplam: {toplam}";
            labelSecili.Text = $"Seçili: {secili}";
            labelHata.Text = $"Hata: {hata}";
            labelHata.Appearance.ForeColor = hata > 0 ? Color.Red : Color.Green;
        }
        private void GridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (gridView.GetRow(e.RowHandle) is FiyatEkleRow row)
            {
                switch (row.Durum)
                {
                    case "Eklendi":
                        e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                        e.Appearance.ForeColor = Color.DarkGreen;
                        break;
                    case "Hata":
                    case "Format Hatası":
                    case "Kontrol Hatası":
                        e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                        e.Appearance.ForeColor = Color.DarkRed;
                        break;
                    case "Malzeme Yok":
                        e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                        e.Appearance.ForeColor = Color.DarkRed;
                        break;
                    case "Kart Zaten Var":
                        e.Appearance.BackColor = Color.FromArgb(255, 235, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                    case "Ekleniyor...":
                        e.Appearance.BackColor = Color.FromArgb(255, 255, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                    case "Eklenecek":
                        e.Appearance.BackColor = Color.FromArgb(220, 220, 255);
                        e.Appearance.ForeColor = Color.DarkBlue;
                        break;
                }
            }
        }
        private void GridView_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridView.GetFocusedRow() is FiyatEkleRow row)
            {
                if (row.Durum == "Eklendi" || row.Durum == "Hata" ||
                    row.Durum == "Malzeme Yok" || row.Durum == "Kart Zaten Var")
                    e.Cancel = true;
            }
        }
        // ─── Tümünü Seç / Temizle ──────────────────────────────────────────────
        private void BtnTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (FiyatEkleRow row in _filtreliListe.Where(x => x.IsValid))
                row.Sec = true;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BtnSecimiTemizle_Click(object sender, EventArgs e)
        {
            foreach (FiyatEkleRow row in _tumListe)
                row.Sec = false;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
       // ─── Listeyi Temizle ───────────────────────────────────────────────────
        private void BarItemTemizle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_tumListe.Count == 0) return;
            if (XtraMessageBox.Show("Liste temizlensin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _tumListe.Clear();
            _filtreliListe.Clear();
            gridControl.DataSource = null;
            textEditDosyaYolu.Text = "";
            btnEkle.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            progressBar.EditValue = 0;
            GuncelleSayaclar();
            SetDurum("Liste temizlendi.", false);
        }
        // ─── Excel Şablon İndir ────────────────────────────────────────────────
        private void BarItemSablon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Excel Şablonu Kaydet";
                dlg.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                dlg.FileName = "FiyatEkleme_Sablonu.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.Add("FiyatEkle");
                        string[] aciklamalar = {
                            "Malzeme Kodu\n✅ Zorunlu\nÖrn: KOL-001",
                            "Malzeme Açıklaması\n⬜ Opsiyonel\nBilgi / açıklama",
                            "Fiyat Kart Kodu\n✅ Zorunlu\nÖrn: KOL-001-I",
                            "Önem Derecesi\n✅ Zorunlu\nSayısal (0-99)",
                            "Fiyat\n✅ Zorunlu\nSayısal\nÖrn: 299.90"
                        };
                        for (int c = 1; c <= aciklamalar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[1, c];
                            cell.Value = aciklamalar[c - 1];
                            cell.Style.WrapText = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 9;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            bool zorunlu = c != 2;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu ? Color.FromArgb(255, 255, 153) : Color.FromArgb(242, 242, 242));
                            SetBorder(cell);
                        }
                        ws.Row(1).Height = 80;
                        string[] basliklar = { "Malzeme Kodu", "Malzeme Açıklaması", "Fiyat Kart Kodu", "Önem Derecesi", "Fiyat" };
                        for (int c = 1; c <= basliklar.Length; c++)
                        {
                            var cell = ws.Cells[2, c];
                            cell.Value = basliklar[c - 1];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 10;
                            cell.Style.Font.Color.SetColor(Color.White);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            bool zorunlu = c != 2;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu ? Color.FromArgb(0, 112, 192) : Color.FromArgb(68, 84, 106));
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            SetBorder(cell);
                        }
                        ws.Row(2).Height = 22;
                        object[][] ornekler = {
                            new object[] { "KOL-001", "Ahşap Sandalye Koli", "KOL-001-I", 3, 299.90m },
                            new object[] { "KOL-001", "Ahşap Sandalye Koli", "KOL-001-T", 2, 250.00m },
                            new object[] { "KOL-002", "Metal Masa",          "KOL-002-I", 3, 499.90m },
                        };
                        for (int r = 0; r < ornekler.Length; r++)
                        {
                            int excelRow = r + 3;
                            for (int c = 1; c <= ornekler[r].Length; c++)
                            {
                                ExcelRange cell = ws.Cells[excelRow, c];
                                cell.Value = ornekler[r][c - 1];
                                cell.Style.Font.Name = "Segoe UI";
                                cell.Style.Font.Size = 10;
                                cell.Style.Font.Italic = true;
                                cell.Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 242, 204));
                                cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                SetBorder(cell);
                            }
                            ws.Row(excelRow).Height = 18;
                        }
                        for (int r = 6; r <= 105; r++)
                        {
                            for (int c = 1; c <= basliklar.Length; c++)
                            {
                                ExcelRange cell = ws.Cells[r, c];
                                cell.Style.Font.Name = "Segoe UI";
                                cell.Style.Font.Size = 10;
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(
                                    r % 2 == 0 ? Color.White : Color.FromArgb(249, 249, 249));
                                SetBorder(cell);
                            }
                            ws.Row(r).Height = 18;
                        }
                        ws.Column(1).Width = 20;
                        ws.Column(2).Width = 30;
                        ws.Column(3).Width = 22;
                        ws.Column(4).Width = 16;
                        ws.Column(5).Width = 16;
                        ws.View.FreezePanes(3, 1);
                        package.SaveAs(new FileInfo(dlg.FileName));
                    }
                    if (XtraMessageBox.Show("Şablon oluşturuldu! Açmak ister misiniz?",
                        "Başarılı", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = dlg.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"Şablon oluşturma hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // ─── Helpers ──────────────────────────────────────────────────────────
        private void RefreshRow(FiyatEkleRow row)
        {
            if (InvokeRequired) { Invoke(new Action(() => RefreshRow(row))); return; }
            int index = _filtreliListe.IndexOf(row);
            if (index >= 0) gridView.RefreshRow(index);
        }
        private void SetDurum(string mesaj, bool hata)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetDurum(mesaj, hata))); return; }
            labelDurum.Text = mesaj;
            labelDurum.Appearance.ForeColor = hata ? Color.Red : Color.Black;
        }
        private string GetCell(OfficeOpenXml.ExcelWorksheet ws, int row, int col)
        {
            try { return ws.Cells[row, col].Value?.ToString()?.Trim() ?? ""; }
            catch { return ""; }
        }
        private decimal ParseDecimal(OfficeOpenXml.ExcelWorksheet ws, int row, int col)
        {
            try
            {
                object val = ws.Cells[row, col].Value;
                if (val == null) return 0;
                if (val is double d) return (decimal)d;
                if (val is int i) return i;
                if (val is decimal dc) return dc;
                string s = val.ToString().Trim();
                if (decimal.TryParse(s.Replace(",", "."), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal r)) return r;
                return 0;
            }
            catch { return 0; }
        }
        private void SetBorder(OfficeOpenXml.ExcelRange range)
        {
            var b = range.Style.Border;
            b.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Top.Color.SetColor(Color.FromArgb(189, 189, 189));
            b.Bottom.Color.SetColor(Color.FromArgb(189, 189, 189));
            b.Left.Color.SetColor(Color.FromArgb(189, 189, 189));
            b.Right.Color.SetColor(Color.FromArgb(189, 189, 189));
        }
        private void btnDosyaSec_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Excel Dosyası Seç";
                dlg.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|Tüm Dosyalar (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textEditDosyaYolu.Text = dlg.FileName;
                    SetDurum($"Dosya seçildi: {Path.GetFileName(dlg.FileName)}", false);
                    YukleExcel(dlg.FileName);
                }
            }
        }
    }
}