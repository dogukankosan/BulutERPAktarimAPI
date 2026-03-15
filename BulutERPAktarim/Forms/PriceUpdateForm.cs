using BulutERPAktarim.Classes;
using BulutERPAktarim.Models;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public partial class PriceUpdateForm : XtraForm
    {
        // ─── Sabitler ────────────────────────────────────────────────────────
        // Excel sütun sırası (0-based)
        private const int COL_MALZEME_KODU = 0;
        private const int COL_MALZEME_ACIKLAMA = 1;
        private const int COL_FIYAT_KODU = 2;
        private const int COL_ONEM_DERECESI = 3;
        private const int COL_YENI_FIYAT = 4;
        private const int EXCEL_MIN_COL = 5;
        // ─── Alanlar ─────────────────────────────────────────────────────────
        private static readonly HttpClient _http = new HttpClient();
        private List<FiyatGuncelleRow> _tumListe = new List<FiyatGuncelleRow>();
        private List<FiyatGuncelleRow> _filtreliListe = new List<FiyatGuncelleRow>();
        private CancellationTokenSource _cts;
        private bool _islemDevam = false;
        // ─── Constructor ─────────────────────────────────────────────────────
        public PriceUpdateForm()
        {
            InitializeComponent();
            InitializeForm();
        }
        private void InitializeForm()
        {
            checkEditSadecHata.Checked = false;
            checkEditSadecDegisiklik.Checked = false;
            btnGuncelle.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            progressBarControl.EditValue = 0;
            ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
            SetDurum("Hazır", false);
        }
        // ─── Form Olayları ────────────────────────────────────────────────────
        private void PriceUpdateForm_Load(object sender, EventArgs e)
        {
            gridView.RowCellStyle += GridView_RowCellStyle;
            gridView.ShowingEditor += GridView_ShowingEditor;
            gridView.CustomRowCellEdit += GridView_CustomRowCellEdit;
            gridView.CustomRowCellEditForEditing += GridView_CustomRowCellEdit;
            gridView.CellValueChanging += GridView_CellValueChanging;
            gridView.SelectionChanged += GridView_SelectionChanged;
        }
        private bool IsKilitliSatir(FiyatGuncelleRow row)
        {
            return row.Durum == "Güncellendi" ||
                   row.Durum == "Değişiklik Yok" ||
                   row.Durum == "Hata" ||
                   row.Durum == "Format Hatası" ||
                   row.Durum == "Fiyat Kartı Yok" ||
                   row.Durum == "Okuma Hatası";
        }
        private void GridView_CustomRowCellEdit(object sender,CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName == "Sec")
            {
                if (gridView.GetRow(e.RowHandle) is FiyatGuncelleRow row && IsKilitliSatir(row))
                {
                    RepositoryItemCheckEdit readOnly = new RepositoryItemCheckEdit();
                    readOnly.ReadOnly = true;
                    e.RepositoryItem = readOnly;
                }
            }
        }
        private void GridView_CellValueChanging(object sender,CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Sec")
            {
                if (gridView.GetRow(e.RowHandle) is FiyatGuncelleRow row && IsKilitliSatir(row))
                    gridView.SetRowCellValue(e.RowHandle, e.Column, false);
            }
        }
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            for (int i = 0; i < gridView.RowCount; i++)
            {
                if (gridView.IsRowSelected(i))
                {
                    if (gridView.GetRow(i) is FiyatGuncelleRow row && IsKilitliSatir(row))
                    {
                        gridView.UnselectRow(i);
                        row.Sec = false;
                    }
                }
            }
        }
        private void PriceUpdateForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }
        // ─── Dosya Seç / Yükle ───────────────────────────────────────────────
        private void BtnDosyaSec_Click(object sender, EventArgs e)
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
        private void YukleExcel(string dosyaYolu)
        {
            try
            {
                SetDurum("Excel yükleniyor...", false);
                _tumListe.Clear();
                if (!File.Exists(dosyaYolu))
                {
                    XtraMessageBox.Show("Dosya bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int satirNo = 0;
                int hataliSatir = 0;
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
                    // Şablon algılama (1. satır başlık açıklaması içeriyor mu)
                    string ilkHucre = GetCell(ws, 1, 1);
                    bool sablon = ilkHucre.Length > 20 || ilkHucre.Contains("Zorunlu") || ilkHucre.Contains("zorunlu");
                    int veriBaslangic = sablon ? 3 : 2;
                    if (colCount < EXCEL_MIN_COL)
                    {
                        XtraMessageBox.Show(
                            $"Excel formatı hatalı! Beklenen minimum sütun: {EXCEL_MIN_COL}\n\n" +
                            "Beklenen format:\nMalzeme Kodu | Malzeme Açıklaması | Fiyat Kart Kodu | Önem Derecesi | Yeni Fiyat",
                            "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    satirNo = 0;
                    hataliSatir = 0;
                   for (int row = veriBaslangic; row <= rowCount; row++)
                    {
                        string malzemeKodu = GetCell(ws, row, COL_MALZEME_KODU + 1);
                        if (string.IsNullOrWhiteSpace(malzemeKodu)) continue;
                        satirNo++;
                        FiyatGuncelleRow item = new FiyatGuncelleRow
                        {
                            SatirNo = satirNo,
                            MalzemeKodu = malzemeKodu.Trim(),
                            MalzemeAciklamasi = GetCell(ws, row, COL_MALZEME_ACIKLAMA + 1),
                            FiyatKodu = GetCell(ws, row, COL_FIYAT_KODU + 1),
                            OnemDerecesi = (int)ParseDecimal(ws, row, COL_ONEM_DERECESI + 1),
                            YeniFiyat = ParseDecimal(ws, row, COL_YENI_FIYAT + 1),
                            Durum = "Bekliyor",
                            Sec = true
                        };
                        // Validasyon
                        List<string> hatalar = new List<string>();
                        if (string.IsNullOrWhiteSpace(item.MalzemeKodu)) hatalar.Add("Malzeme kodu boş");
                        if (string.IsNullOrWhiteSpace(item.FiyatKodu)) hatalar.Add("Fiyat kart kodu boş");
                        if (item.YeniFiyat <= 0) hatalar.Add("Fiyat 0 veya negatif");
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
                // Logo'dan mevcut fiyatları arka planda çek
                _ = LogoMevcutFiyatCekAsync(_tumListe.ToList());
                GuncelleSayaclar();
                btnGuncelle.Enabled = _tumListe.Any(x => x.IsValid);
                btnTumunuSec.Enabled = true;
                btnSecimiTemizle.Enabled = true;
                string mesaj = $"Excel yüklendi: {_tumListe.Count} satır ({hataliSatir} hatalı)" +
                               (false ? " [Şablon formatı]" : "");
                SetDurum(mesaj, false);
                if (hataliSatir > 0)
                    XtraMessageBox.Show(
                        $"{_tumListe.Count} satırdan {hataliSatir} tanesi format hatası içeriyor.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Excel yükleme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetDurum("Excel yükleme hatası!", true);
                _ = TextLog.LogToSQLiteAsync($"❌ PriceUpdateForm Excel yükleme: {ex}");
            }
        }
        // ─── Logo'dan Mevcut Fiyat Çekme (GET) ───────────────────────────────
        private async Task LogoMevcutFiyatCekAsync(List<FiyatGuncelleRow> liste)
        {
            try
            {
                SetDurum("Logo'dan mevcut fiyatlar okunuyor...", false);
                progressBarControl.Properties.Maximum = liste.Count;
                progressBarControl.EditValue = 0;
                btnGuncelle.Enabled = false;
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
                        var (ok, mevcutFiyat, mevcutOncelik, hata) =
                            await ReadSalesPriceAsync(settings, token, row.MalzemeKodu, row.FiyatKodu);
                        if (ok)
                        {
                            row.MevcutFiyat = mevcutFiyat;
                            row.MevcutOncelik = mevcutOncelik;
                            row.LogoKontrolEdildi = true;
                            // Fiyat değişmemişse işaretle
                            if (row.YeniFiyat == mevcutFiyat && row.OnemDerecesi == mevcutOncelik)
                                row.Durum = "Değişiklik Yok";
                        }
                        else
                        {
                            // Fiyat kartı bulunamadı ya da hata
                            row.HataMesaji = hata;
                            row.Durum = string.IsNullOrWhiteSpace(hata) ? "Fiyat Kartı Yok" : "Okuma Hatası";
                            row.Sec = false;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        int done = Interlocked.Increment(ref tamamlanan);
                        Invoke(new Action(() =>
                        {
                            progressBarControl.EditValue = done;
                            SetDurum($"Logo okunuyor... {done}/{liste.Count}", false);
                            gridView.RefreshData();
                            GuncelleSayaclar();
                        }));
                    }
                });
                await Task.WhenAll(tasks);
                progressBarControl.EditValue = 0;
                int degisiklikVar = liste.Count(x => x.LogoKontrolEdildi && x.Durum == "Bekliyor");
                int degisiklikYok = liste.Count(x => x.Durum == "Değişiklik Yok");
                int kartYok = liste.Count(x => x.Durum == "Fiyat Kartı Yok");
                SetDurum($"Logo kontrolü tamamlandı — Güncellenecek: {degisiklikVar} | Değişiklik Yok: {degisiklikYok} | Kart Yok: {kartYok}", false);
            }
            catch (Exception ex)
            {
                SetDurum($"Logo okuma hatası: {ex.Message}", true);
                await TextLog.LogToSQLiteAsync($"❌ LogoMevcutFiyatCekAsync: {ex}");
            }
            finally
            {
                btnGuncelle.Enabled = _tumListe.Any(x => x.IsValid && x.Sec);
            }
        }
        // ─── GET salesprice ───────────────────────────────────────────────────
        private async Task<(bool Success, decimal UnitPrice, int Priority, string ErrorMessage)>
            ReadSalesPriceAsync(BulutERPSettings settings, string accessToken, string mmCode, string priceCode)
        {
            try
            {
                string url = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}" +
                             $"/logo/restservices/rest/v2.0/salesprice" +
                             $"?mmCode={Uri.EscapeDataString(mmCode)}" +
                             $"&priceCode={Uri.EscapeDataString(priceCode)}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("access-token", accessToken);
                request.Headers.Add("firm", settings.FirmNr.Trim());
                request.Headers.Add("lang", "TRTR");
                request.Headers.Add("Accept", "application/json");
                HttpResponseMessage response = await _http.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, 0, 0, $"HTTP {(int)response.StatusCode}: {json}");
                var obj = JObject.Parse(json);
                decimal fiyat = obj["unitPrice"]?.Value<decimal>() ?? 0;
                int oncelik = obj["priority"]?.Value<int>() ?? 0;
                return (true, fiyat, oncelik, null);
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
        }
        // ─── Güncelle Butonu ─────────────────────────────────────────────────
        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (_islemDevam)
            {
                _cts?.Cancel();
                btnGuncelle.Text = "💰  Seçilenlerin Fiyatını Güncelle";
                SetDurum("İptal ediliyor...", false);
                return;
            }
           var secilenler = _tumListe
                .Where(x => x.Sec && x.IsValid && x.Durum != "Değişiklik Yok")
                .ToList();
            if (!secilenler.Any())
            {
                XtraMessageBox.Show("Güncellenecek seçili satır bulunamadı!\n\nLütfen önce satırları seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (XtraMessageBox.Show(
                    $"{secilenler.Count} adet fiyat kartı güncellenecek.\n\nDevam edilsin mi?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _ = GuncelleAsync(secilenler);
        }
        private async Task GuncelleAsync(List<FiyatGuncelleRow> secilenler)
        {
            _islemDevam = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            btnGuncelle.Text = "⛔  İptal Et";
            btnDosyaSec.Enabled = false;
            progressBarControl.Properties.Maximum = secilenler.Count;
            progressBarControl.EditValue = 0;
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
                BulutERPSettings settings = settingsResult.Settings;
                string token = tokenResult.AccessToken;
                for (int i = 0; i < secilenler.Count; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    var row = secilenler[i];
                    row.Durum = "Güncelleniyor...";
                    row.HataMesaji = "";
                    RefreshRow(row);
                    SetDurum($"[{i + 1}/{secilenler.Count}] {row.MalzemeKodu} / {row.FiyatKodu} güncelleniyor...", false);
                   try
                    {
                        var (ok, hata) = await UpdateSalesPriceAsync(settings, token, row);
                        if (ok)
                        {
                            row.Durum = "Güncellendi";
                            row.HataMesaji = "";
                            row.MevcutFiyat = row.YeniFiyat;
                            row.MevcutOncelik = row.OnemDerecesi;
                            basarili++;
                        }
                        else
                        {
                            row.Durum = "Hata";
                            row.HataMesaji = hata;
                            hatali++;
                        }
                    }
                    catch (Exception ex)
                    {
                        row.Durum = "Hata";
                        row.HataMesaji = ex.Message;
                        hatali++;
                        await TextLog.LogToSQLiteAsync($"❌ Güncelleme hatası ({row.MalzemeKodu}/{row.FiyatKodu}): {ex}");
                    }
                    RefreshRow(row);
                    progressBarControl.EditValue = i + 1;
                    GuncelleSayaclar();
                    // Her 5 satırda token yenile
                    if ((i + 1) % 5 == 0)
                    {
                        var yeniToken = await BulutERPService.EnsureValidTokenAsync();
                        if (yeniToken.Success) token = yeniToken.AccessToken;
                    }
                }
                SetDurum($"Tamamlandı — ✅ Başarılı: {basarili} | ❌ Hatalı: {hatali}", false);
                XtraMessageBox.Show(
                    $"Güncelleme tamamlandı!\n\n✅ Başarılı: {basarili}\n❌ Hatalı: {hatali}",
                    "Sonuç", MessageBoxButtons.OK,
                    hatali > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                SetDurum($"İptal edildi. Başarılı: {basarili} | Hatalı: {hatali}", false);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await TextLog.LogToSQLiteAsync($"❌ GuncelleAsync: {ex}");
            }
            finally
            {
                _islemDevam = false;
                btnGuncelle.Text = "💰  Seçilenlerin Fiyatını Güncelle";
                btnDosyaSec.Enabled = true;
                GuncelleSayaclar();
            }
        }
        // ─── PUT salesprice ───────────────────────────────────────────────────
        private async Task<(bool Success, string ErrorMessage)>
            UpdateSalesPriceAsync(BulutERPSettings settings, string accessToken, FiyatGuncelleRow row)
        {
            try
            {
                // Sadece fiyat ve öncelik gönderiliyor; diğer alanlar API tarafında korunur
                var body = new
                {
                    unitPrice = row.YeniFiyat,
                    priority = row.OnemDerecesi
                };
                string url = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}" +
                             $"/logo/restservices/rest/v2.0/salesprice" +
                             $"?mmCode={Uri.EscapeDataString(row.MalzemeKodu)}" +
                             $"&priceCode={Uri.EscapeDataString(row.FiyatKodu)}";

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Headers.Add("access-token", accessToken);
                request.Headers.Add("firm", settings.FirmNr.Trim());
                request.Headers.Add("lang", "TRTR");
                request.Headers.Add("Accept", "application/json");
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _http.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ PUT salesprice hatası\n" +
                        $"   Malzeme : {row.MalzemeKodu}\n" +
                        $"   FiyatKod: {row.FiyatKodu}\n" +
                        $"   HTTP    : {(int)response.StatusCode}\n" +
                        $"   Yanıt   : {json}");
                    return (false, $"[{(int)response.StatusCode}] {json}");
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        // ─── Grid / Filtre ────────────────────────────────────────────────────
        private void UygulaFiltre()
        {
            bool sadecHata = checkEditSadecHata.Checked;
            bool sadecDegisiklik = checkEditSadecDegisiklik.Checked;
            _filtreliListe = _tumListe.Where(x =>
            {
                bool hataUygun = !sadecHata || x.Durum == "Format Hatası" || x.Durum == "Hata";
                bool degUygun = !sadecDegisiklik || (x.Durum != "Değişiklik Yok" && x.Durum != "Güncellendi");
                return hataUygun && degUygun;
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
            int hata = _tumListe.Count(x => x.Durum == "Format Hatası" || x.Durum == "Hata");
            labelControlToplam.Text = $"Toplam: {toplam}";
            labelControlSecili.Text = $"Seçili: {secili}";
            labelControlHata.Text = $"Hata: {hata}";
            labelControlHata.Appearance.ForeColor = hata > 0 ? Color.Red : Color.Green;
        }
        private void GridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (gridView.GetRow(e.RowHandle) is FiyatGuncelleRow row)
            {
                switch (row.Durum)
                {
                    case "Güncellendi":
                        e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                        e.Appearance.ForeColor = Color.DarkGreen;
                        break;
                    case "Hata":
                    case "Format Hatası":
                    case "Okuma Hatası":
                        e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                        e.Appearance.ForeColor = Color.DarkRed;
                        break;
                    case "Güncelleniyor...":
                        e.Appearance.BackColor = Color.FromArgb(255, 255, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                    case "Değişiklik Yok":
                        e.Appearance.BackColor = Color.FromArgb(220, 220, 255);
                        e.Appearance.ForeColor = Color.DarkBlue;
                        break;
                    case "Fiyat Kartı Yok":
                        e.Appearance.BackColor = Color.FromArgb(255, 235, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                }
            }
        }
        private void GridView_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridView.GetFocusedRow() is FiyatGuncelleRow row && IsKilitliSatir(row))
                e.Cancel = true;
        }
        // ─── Tümünü Seç / Temizle ─────────────────────────────────────────────
        private void BtnTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (FiyatGuncelleRow row in _filtreliListe.Where(x => x.IsValid && !IsKilitliSatir(x)))
                row.Sec = true;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BtnSecimiTemizle_Click(object sender, EventArgs e)
        {
            foreach (FiyatGuncelleRow row in _tumListe)
                row.Sec = false;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BarItemTemizle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_tumListe.Count == 0) return;
            if (XtraMessageBox.Show("Liste temizlensin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _tumListe.Clear();
            _filtreliListe.Clear();
            gridControl.DataSource = null;
            textEditDosyaYolu.Text = "";
            btnGuncelle.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            progressBarControl.EditValue = 0;
            GuncelleSayaclar();
            SetDurum("Liste temizlendi.", false);
        }
        // ─── Excel Şablon İndir ───────────────────────────────────────────────
        private void BarItemExcelSablon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Excel Şablonu Kaydet";
                dlg.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                dlg.FileName = "FiyatGuncelleme_Sablonu.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.Add("Fiyat");
                        // Satır 1: Açıklama
                        string[] aciklamalar = {
                            "Malzeme Kodu\n✅ Zorunlu\nÖrn: KOL-001",
                            "Malzeme Açıklaması\n⬜ Opsiyonel\nBilgi amaçlı",
                            "Fiyat Kart Kodu\n✅ Zorunlu\nÖrn: KOL-001-I",
                            "Önem Derecesi\n✅ Zorunlu\nSayısal (0-99)\nÖrn: 1",
                            "Yeni Fiyat\n✅ Zorunlu\nSayısal\nÖrn: 299.90"
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
                        // Satır 2: Başlıklar
                        string[] basliklar = { "Malzeme Kodu", "Malzeme Açıklaması", "Fiyat Kart Kodu", "Önem Derecesi", "Yeni Fiyat" };
                        for (int c = 1; c <= basliklar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[2, c];
                            cell.Value = basliklar[c - 1];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 10;
                            cell.Style.Font.Color.SetColor(Color.White);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            bool zorunlu = c != 2;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu ? Color.FromArgb(192, 0, 0) : Color.FromArgb(68, 84, 106));
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            SetBorder(cell);
                        }
                        ws.Row(2).Height = 22;
                        // Örnek satırlar
                        object[][] ornekler = {
                            new object[] { "KOL-001", "Ahşap Sandalye Koli", "KOL-001-I", 3, 299.90m },
                            new object[] { "KOL-001", "Ahşap Sandalye Koli", "KOL-001-T", 2, 250.00m },
                            new object[] { "KOL-001", "Ahşap Sandalye Koli", "KOL-001-M", 1, 349.90m },
                        };
                        for (int r = 0; r < ornekler.Length; r++)
                        {
                            int excelRow = r + 3;
                            for (int c = 1; c <= ornekler[r].Length; c++)
                            {
                                var cell = ws.Cells[excelRow, c];
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
                        // Boş satırlar (6-105)
                        for (int r = 6; r <= 105; r++)
                        {
                            for (int c = 1; c <= basliklar.Length; c++)
                            {
                                var cell = ws.Cells[r, c];
                                cell.Style.Font.Name = "Segoe UI";
                                cell.Style.Font.Size = 10;
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(
                                    r % 2 == 0 ? Color.White : Color.FromArgb(249, 249, 249));
                                SetBorder(cell);
                            }
                            ws.Row(r).Height = 18;
                        }
                        // Kolon genişlikleri
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
        // ─── Helpers ─────────────────────────────────────────────────────────
        private void RefreshRow(FiyatGuncelleRow row)
        {
            if (InvokeRequired) { Invoke(new Action(() => RefreshRow(row))); return; }
            int index = _filtreliListe.IndexOf(row);
            if (index >= 0) gridView.RefreshRow(index);
        }
        private void SetDurum(string mesaj, bool hata)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetDurum(mesaj, hata))); return; }
            labelControlDurum.Text = mesaj;
            labelControlDurum.Appearance.ForeColor = hata ? Color.Red : Color.Black;
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
                if (decimal.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r))
                    return r;
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
    }
}