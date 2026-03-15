using BulutERPAktarim.Classes;
using BulutERPAktarim.Models;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class ProductUpdateForm : XtraForm
    {
        // ─── Excel Sütun İndeksleri (0 tabanlı) ──────────────────────────────
        private const int COL_MALZEME_KODU = 0;
        private const int COL_ACIKLAMA = 1;
        private const int COL_BARKOD = 2;
        private const int COL_OZEL_KOD1 = 3;
        private const int COL_OZEL_KOD2 = 4;
        private const int COL_OZEL_KOD3 = 5;
        private const int COL_OZEL_KOD4 = 6;
        private const int COL_OZEL_KOD5 = 7;
        private const int COL_MARKA = 8;
        private const int COL_MODEL = 9;
        private const int EXCEL_MIN_COL = 2; // En az Malzeme Kodu + Açıklama

        // ─── Alanlar ──────────────────────────────────────────────────────────
        private static readonly HttpClient _http = new HttpClient();
        private List<ProductUpdateRow> _tumListe = new List<ProductUpdateRow>();
        private List<ProductUpdateRow> _filtreliListe = new List<ProductUpdateRow>();
        private CancellationTokenSource _cts;
        private bool _islemDevam = false;

        // ─── Constructor ──────────────────────────────────────────────────────
        public ProductUpdateForm()
        {
            InitializeComponent();
            InitializeForm();
        }
        private void InitializeForm()
        {
            checkEditSadecHata.Checked = false;
            checkEditSadecGuncellenecek.Checked = false;
            btnGuncelle.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            progressBar.EditValue = 0;
            ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
            SetDurum("Hazır", false);
        }
        // ─── Form Olayları ─────────────────────────────────────────────────────
        private void ProductUpdateForm_Load(object sender, EventArgs e)
        {
            gridView.RowCellStyle += GridView_RowCellStyle;
            gridView.ShowingEditor += GridView_ShowingEditor;
        }
        private void ProductUpdateForm_KeyDown(object sender, KeyEventArgs e)
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
                        XtraMessageBox.Show("Excel dosyasında sayfa bulunamadı!", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int rowCount = ws.Dimension?.Rows ?? 0;
                    int colCount = ws.Dimension?.Columns ?? 0;
                    // Şablon: 1. satır açıklama, 2. satır başlık, 3. satırdan itibaren veri
                    int veriBaslangic = 3;
                    if (colCount < EXCEL_MIN_COL)
                    {
                        XtraMessageBox.Show(
                            $"Excel formatı hatalı! Minimum {EXCEL_MIN_COL} sütun bekleniyor.\n\n" +
                            "Beklenen format:\nMalzeme Kodu | Açıklama | Barkod(opsiyonel) | Özel Kod 1-5(opsiyonel) | Marka(opsiyonel) | Model(opsiyonel)",
                            "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int satirNo = 0;
                    for (int row = veriBaslangic; row <= rowCount; row++)
                    {
                        string malzemeKodu = GetCell(ws, row, COL_MALZEME_KODU + 1);
                        if (string.IsNullOrWhiteSpace(malzemeKodu)) continue;
                       satirNo++;
                        ProductUpdateRow item = new ProductUpdateRow
                        {
                            SatirNo = satirNo,
                            MalzemeKodu = malzemeKodu.Trim(),
                            Aciklama = GetCell(ws, row, COL_ACIKLAMA + 1),
                            Barkod = GetCell(ws, row, COL_BARKOD + 1),
                            OzelKod1 = GetCell(ws, row, COL_OZEL_KOD1 + 1),
                            OzelKod2 = GetCell(ws, row, COL_OZEL_KOD2 + 1),
                            OzelKod3 = GetCell(ws, row, COL_OZEL_KOD3 + 1),
                            OzelKod4 = GetCell(ws, row, COL_OZEL_KOD4 + 1),
                            OzelKod5 = GetCell(ws, row, COL_OZEL_KOD5 + 1),
                            Marka = GetCell(ws, row, COL_MARKA + 1),
                            Model = GetCell(ws, row, COL_MODEL + 1),
                            Durum = "Bekliyor",
                            Sec = true
                        };
                        // Format validasyonu — sadece açıklama zorunlu
                        List<string> hatalar = new List<string>();
                        if (string.IsNullOrWhiteSpace(item.Aciklama))
                            hatalar.Add("Açıklama boş olamaz");
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
                btnGuncelle.Enabled = false;
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
                XtraMessageBox.Show($"Excel yükleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetDurum("Excel yükleme hatası!", true);
                _ = TextLog.LogToSQLiteAsync($"❌ ProductUpdateForm Excel yükleme: {ex.Message}");
            }
        }
        // ─── Logo Kontrol ─────────────────────────────────────────────────────
        private async Task LogoKontrolAsync(List<ProductUpdateRow> liste)
        {
            try
            {
                SetDurum("Logo'dan kontrol ediliyor...", false);
                progressBar.Properties.Maximum = liste.Count;
                progressBar.EditValue = 0;
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
                        // Format hatası varsa kontrol etme
                        if (row.Durum == "Format Hatası") return;
                        // 1. Malzeme var mı? LOGICALREF çek
                        // YENİ:
                        string sqlMalzeme = $"SELECT LOGICALREF, CARDTYPE FROM U_$V(firm)_ITEMS " +
                                            $"WHERE BOSTATUS<>1 AND CODE='{row.MalzemeKodu.Replace("'", "''")}'";
                        var malzemeResult = await BulutERPService.ExecuteSelectQueryAsync(sqlMalzeme, token, 1);
                        if (!malzemeResult.Success)
                        {
                            row.Durum = "Kontrol Hatası";
                            row.HataMesaji = malzemeResult.ErrorMessage;
                            row.Sec = false;
                            return;
                        }
                        if (malzemeResult.Data == null || malzemeResult.Data.Count == 0)
                        {
                            row.Durum = "Malzeme Yok";
                            row.HataMesaji = $"'{row.MalzemeKodu}' Logo'da bulunamadı";
                            row.Sec = false;
                            return;
                        }
                        int cardType = Convert.ToInt32(malzemeResult.Data[0]["CARDTYPE"]);
                        if (cardType == 16)
                        {
                            row.Durum = "Desteklenmiyor";
                            row.HataMesaji = "Malzeme Takımı Logo API tarafından güncellenemiyor";
                            row.Sec = false;
                            return;
                        }
                        row.LogicalRef = Convert.ToInt32(malzemeResult.Data[0]["LOGICALREF"]);
                        row.MalzemeVarMi = true;
                        // 2. Barkod yazıldıysa — başka malzemede var mı?
                        if (!string.IsNullOrWhiteSpace(row.Barkod))
                        {
                            string sqlBarkod = $"SELECT I.CODE FROM U_$V(firm)_UNITBARCODE UB " +
                                               $"INNER JOIN U_$V(firm)_ITEMS I ON I.LOGICALREF = UB.ITEMREF " +
                                               $"WHERE UB.BARCODE='{row.Barkod.Replace("'", "''")}' " +
                                               $"AND I.BOSTATUS<>1 " +
                                               $"AND I.LOGICALREF<>{row.LogicalRef}"; // kendi barkoduysa sorun yok
                            var barkodResult = await BulutERPService.ExecuteSelectQueryAsync(sqlBarkod, token, 1);
                            if (!barkodResult.Success)
                            {
                                row.Durum = "Kontrol Hatası";
                                row.HataMesaji = $"Barkod kontrol hatası: {barkodResult.ErrorMessage}";
                                row.Sec = false;
                                return;
                            }
                            if (barkodResult.Data != null && barkodResult.Data.Count > 0)
                            {
                                string existingCode = barkodResult.Data[0]["CODE"]?.ToString();
                                row.Durum = "Barkod Çakışması";
                                row.HataMesaji = $"'{row.Barkod}' barkodu '{existingCode}' malzemesinde kayıtlı";
                                row.Sec = false;
                                return;
                            }
                        }
                       // 3. Marka yazıldıysa — sistemde var mı?
                        if (!string.IsNullOrWhiteSpace(row.Marka))
                        {
                            string sqlMarka = $"SELECT LOGICALREF FROM U_$V(firm)_BRANDS " +
                                              $"WHERE CODE='{row.Marka.Replace("'", "''")}'";
                            var markaResult = await BulutERPService.ExecuteSelectQueryAsync(sqlMarka, token, 1);
                            if (!markaResult.Success)
                            {
                                row.Durum = "Kontrol Hatası";
                                row.HataMesaji = $"Marka kontrol hatası: {markaResult.ErrorMessage}";
                                row.Sec = false;
                                return;
                            }
                            if (markaResult.Data == null || markaResult.Data.Count == 0)
                            {
                                row.Durum = "Marka Bulunamadı";
                                row.HataMesaji = $"'{row.Marka}' markası Logo'da bulunamadı";
                                row.Sec = false;
                                return;
                            }
                        }
                        // 4. Model yazıldıysa — sistemde var mı?
                        if (!string.IsNullOrWhiteSpace(row.Model))
                        {
                            string markaFilter = string.IsNullOrWhiteSpace(row.Marka)
                                ? ""
                                : $" AND BRANDREF=(SELECT LOGICALREF FROM U_$V(firm)_BRANDS WHERE CODE='{row.Marka.Replace("'", "''")}')";
                            string sqlModel = $"SELECT LOGICALREF FROM U_$V(firm)_BRANDMODELS " +
                                              $"WHERE CODE='{row.Model.Replace("'", "''")}'{markaFilter}";
                            var modelResult = await BulutERPService.ExecuteSelectQueryAsync(sqlModel, token, 1);
                            if (!modelResult.Success)
                            {
                                row.Durum = "Kontrol Hatası";
                                row.HataMesaji = $"Model kontrol hatası: {modelResult.ErrorMessage}";
                                row.Sec = false;
                                return;
                            }
                            if (modelResult.Data == null || modelResult.Data.Count == 0)
                            {
                                row.Durum = "Model Bulunamadı";
                                row.HataMesaji = $"'{row.Model}' modeli Logo'da bulunamadı";
                                row.Sec = false;
                                return;
                            }
                        }
                        // Tüm kontroller geçti
                        row.LogoKontrolEdildi = true;
                        row.Durum = "Güncellenecek";
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
                int guncellenecek = liste.Count(x => x.Durum == "Güncellenecek");
                int malzemeYok = liste.Count(x => x.Durum == "Malzeme Yok");
                int barkodCakisma = liste.Count(x => x.Durum == "Barkod Çakışması");
                SetDurum($"Kontrol tamamlandı — Güncellenecek: {guncellenecek} | Malzeme Yok: {malzemeYok} | Barkod Çakışma: {barkodCakisma}", false);
            }
            catch (Exception ex)
            {
                SetDurum($"Logo kontrol hatası: {ex.Message}", true);
                await TextLog.LogToSQLiteAsync($"❌ LogoKontrol hatası | {ex.Message}");
            }
            finally
            {
                btnGuncelle.Enabled = _tumListe.Any(x => x.IsValid && x.Sec);
            }
        }
        // ─── Güncelle Butonu ──────────────────────────────────────────────────
        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (_islemDevam)
            {
                _cts?.Cancel();
                btnGuncelle.Text = "🔄  Seçilenleri Güncelle";
                SetDurum("İptal ediliyor...", false);
                return;
            }
            var secilenler = _tumListe.Where(x => x.Sec && x.IsValid).ToList();
            if (!secilenler.Any())
            {
                XtraMessageBox.Show("Güncellenecek seçili satır bulunamadı!\n\nLütfen önce satırları seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show(
                    $"{secilenler.Count} adet malzeme güncellenecek.\n\nDevam edilsin mi?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _ = GuncelleAsync(secilenler);
        }
        private async Task GuncelleAsync(List<ProductUpdateRow> secilenler)
        {
            _islemDevam = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            btnGuncelle.Text = "⛔  İptal Et";
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

                    ProductUpdateRow row = secilenler[i];
                    row.Durum = "Güncelleniyor...";
                    row.HataMesaji = "";
                    RefreshRow(row);
                    SetDurum($"[{i + 1}/{secilenler.Count}] {row.MalzemeKodu} güncelleniyor...", false);
                   try
                    {
                        var (ok, hata) = await UpdateMalzemeAsync(settings, token, row);
                        if (ok)
                        {
                            row.Durum = "Güncellendi";
                            row.HataMesaji = "";
                            basarili++;
                        }
                        else
                        {
                            row.Durum = "Hata";
                            row.HataMesaji = hata;
                            hatali++;
                            await TextLog.LogToSQLiteAsync($"❌ Güncellenemedi | {row.MalzemeKodu} | {hata}");
                        }
                    }
                    catch (Exception ex)
                    {
                        row.Durum = "Hata";
                        row.HataMesaji = ex.Message;
                        hatali++;
                        await TextLog.LogToSQLiteAsync($"❌ Güncelleme exception | {row.MalzemeKodu} | {ex.Message}");
                    }
                    RefreshRow(row);
                    progressBar.EditValue = i + 1;
                    GuncelleSayaclar();
                    // Her 5 kayıtta token yenile
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
                await TextLog.LogToSQLiteAsync($"❌ GuncelleAsync hatası | {ex.Message}");
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _islemDevam = false;
                btnGuncelle.Text = "🔄  Seçilenleri Güncelle";
                barItemDosyaSec.Enabled = true;
                GuncelleSayaclar();
            }
        }
        // ─── PUT /v2.0/items/ref ──────────────────────────────────────────────
        private async Task<(bool Success, string ErrorMessage)> UpdateMalzemeAsync(
            BulutERPSettings settings,
            string accessToken,
            ProductUpdateRow row)
        {
            try
            {
                // 1. Önce mevcut kartı GET ile çek — unitAssignments ve diğer alanları korumak için
                string getUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}" +
                                $"/logo/restservices/rest/v2.0/items/ref?logicalRef={row.LogicalRef}";
                HttpRequestMessage getReq = new HttpRequestMessage(HttpMethod.Get, getUrl);
                getReq.Headers.Add("access-token", accessToken);
                getReq.Headers.Add("firm", settings.FirmNr.Trim());
                getReq.Headers.Add("lang", "TRTR");
                getReq.Headers.Add("Accept", "application/json");
                HttpResponseMessage getResponse = await _http.SendAsync(getReq);
                string getBody = await getResponse.Content.ReadAsStringAsync();
                if (!getResponse.IsSuccessStatusCode)
                    return (false, $"GET hatası [{(int)getResponse.StatusCode}]: {getBody}");
                // GET response'u logla — marka/model yapısını görmek için
                try
                {
                    string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSONLog");
                    if (!System.IO.Directory.Exists(logPath)) System.IO.Directory.CreateDirectory(logPath);
                    string logFile = System.IO.Path.Combine(logPath, $"GET_ITEMS_{row.MalzemeKodu}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json");
                    System.IO.File.WriteAllText(logFile, getBody);
                }
                catch { }
                JObject mevcut = JObject.Parse(getBody);
                // 2. Sadece ilgili alanları güncelle, geri kalanı olduğu gibi bırak
                mevcut["description"] = row.Aciklama;
                mevcut["auxCode"] = row.OzelKod1 ?? "";
                mevcut["auxiliaryCode2"] = row.OzelKod2 ?? "";
                mevcut["auxiliaryCode3"] = row.OzelKod3 ?? "";
                mevcut["auxiliaryCode4"] = row.OzelKod4 ?? "";
                mevcut["auxiliaryCode5"] = row.OzelKod5 ?? "";
                // Marka: boşsa "" gönder (null, JValue.CreateNull hepsi denendi — "" ile dene)
                mevcut["brandCode"] = row.Marka?.Trim() ?? "";
                mevcut["brandDescription"] = row.Marka?.Trim() ?? "";
                mevcut["modelCode"] = row.Model?.Trim() ?? "";
                mevcut["modelDescription"] = row.Model?.Trim() ?? "";
                // 3. Barkod güncelleme — unitAssignments içinde
                // Barkod yazıldıysa set et, boşsa temizle (ikisi de gönderilir)
                JArray unitAssignments = mevcut["unitAssignments"] as JArray;
                if (unitAssignments != null && unitAssignments.Count > 0)
                {
                    JObject firstUnit = unitAssignments[0] as JObject;
                    if (firstUnit != null)
                    {
                        JArray barcodes = firstUnit["barcodes"] as JArray;
                        if (barcodes == null)
                        {
                            barcodes = new JArray();
                            firstUnit["barcodes"] = barcodes;
                        }
                       barcodes.Clear(); // Her durumda önce temizle
                        if (!string.IsNullOrWhiteSpace(row.Barkod))
                        {
                            // Barkod yazıldıysa ekle
                            barcodes.Add(new JObject
                            {
                                ["number"] = 1,
                                ["barcode"] = row.Barkod.Trim()
                            });
                        }
                        // Boşsa temizlenmiş haliyle gönderilir
                    }
                }
                // 4. PUT ile gönder
                string putUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}" +
                                $"/logo/restservices/rest/v2.0/items/ref?logicalRef={row.LogicalRef}";
                string putJson = JsonConvert.SerializeObject(mevcut);
                HttpRequestMessage putReq = new HttpRequestMessage(HttpMethod.Put, putUrl);
                putReq.Headers.Add("access-token", accessToken);
                putReq.Headers.Add("firm", settings.FirmNr.Trim());
                putReq.Headers.Add("lang", "TRTR");
                putReq.Headers.Add("Accept", "application/json");
                putReq.Content = new StringContent(putJson, Encoding.UTF8, "application/json");
                HttpResponseMessage putResponse = await _http.SendAsync(putReq);
                string putBody = await putResponse.Content.ReadAsStringAsync();
               if (!putResponse.IsSuccessStatusCode)
                    return (false, $"[{(int)putResponse.StatusCode}] {putBody}");
                return (true, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ UpdateMalzeme hatası | {row.MalzemeKodu} | {ex.Message}");
                return (false, ex.Message);
            }
        }
        // ─── Grid / Filtre ─────────────────────────────────────────────────────
        private void UygulaFiltre()
        {
            bool sadecHata = checkEditSadecHata.Checked;
            bool sadecGuncellenecek = checkEditSadecGuncellenecek.Checked;
            _filtreliListe = _tumListe.Where(x =>
            {
                bool hataUygun = !sadecHata ||
                    x.Durum == "Format Hatası" ||
                    x.Durum == "Hata" ||
                    x.Durum == "Malzeme Yok" ||
                    x.Durum == "Barkod Çakışması" ||
                    x.Durum == "Marka Bulunamadı" ||
                    x.Durum == "Model Bulunamadı" ||
                    x.Durum == "Kontrol Hatası";
                bool gunUygun = !sadecGuncellenecek ||
                    x.Durum == "Güncellenecek" ||
                    x.Durum == "Bekliyor";
                return hataUygun && gunUygun;
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
                x.Durum == "Format Hatası" ||
                x.Durum == "Hata" ||
                x.Durum == "Malzeme Yok" ||
                x.Durum == "Barkod Çakışması" ||
                x.Durum == "Marka Bulunamadı" ||
                x.Durum == "Model Bulunamadı" ||
                x.Durum == "Kontrol Hatası" ||
                x.Durum == "Desteklenmiyor"); // ← EKLE
            labelToplam.Text = $"Toplam: {toplam}";
            labelSecili.Text = $"Seçili: {secili}";
            labelHata.Text = $"Hata: {hata}";
            labelHata.Appearance.ForeColor = hata > 0 ? Color.Red : Color.Green;
        }
        private void GridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (gridView.GetRow(e.RowHandle) is ProductUpdateRow row)
            {
                switch (row.Durum)
                {
                    case "Güncellendi":
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
                    case "Barkod Çakışması":
                    case "Marka Bulunamadı":
                    case "Model Bulunamadı":
                        e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                        e.Appearance.ForeColor = Color.DarkRed;
                        break;
                    case "Güncelleniyor...":
                        e.Appearance.BackColor = Color.FromArgb(255, 255, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                    case "Desteklenmiyor":
                        e.Appearance.BackColor = Color.FromArgb(230, 230, 230);
                        e.Appearance.ForeColor = Color.DimGray;
                        break;
                    case "Güncellenecek":
                        e.Appearance.BackColor = Color.FromArgb(220, 220, 255);
                        e.Appearance.ForeColor = Color.DarkBlue;
                        break;
                }
            }
        }
        private void GridView_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridView.GetFocusedRow() is ProductUpdateRow row && row.IsKilitli)
                e.Cancel = true;
        }
        // ─── Tümünü Seç / Temizle ─────────────────────────────────────────────
        private void BtnTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ProductUpdateRow row in _filtreliListe.Where(x => x.IsValid && !x.IsKilitli))
                row.Sec = true;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BtnSecimiTemizle_Click(object sender, EventArgs e)
        {
            foreach (ProductUpdateRow row in _tumListe)
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
            btnGuncelle.Enabled = false;
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
                dlg.FileName = "MalzemeGuncelleme_Sablonu.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.Add("MalzemeGuncelle");

                        string[] aciklamalar = {
                            "Malzeme Kodu\n✅ Zorunlu\nÖrn: KOL-001",
                            "Açıklama\n✅ Zorunlu\nMalzeme adı/açıklaması",
                            "Barkod\n⬜ Opsiyonel\nYazılırsa gönderilir\nBaşka malzemede varsa HATA",
                            "Özel Kod 1\n⬜ Opsiyonel",
                            "Özel Kod 2\n⬜ Opsiyonel",
                            "Özel Kod 3\n⬜ Opsiyonel",
                            "Özel Kod 4\n⬜ Opsiyonel",
                            "Özel Kod 5\n⬜ Opsiyonel",
                            "Marka\n⬜ Opsiyonel\nYazılırsa sistemde\nvar olmalı",
                            "Model\n⬜ Opsiyonel\nYazılırsa sistemde\nvar olmalı"
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
                            bool zorunlu = c <= 2;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu ? Color.FromArgb(255, 255, 153) : Color.FromArgb(242, 242, 242));
                            SetBorder(cell);
                        }
                        ws.Row(1).Height = 80;
                        string[] basliklar = {
                            "Malzeme Kodu", "Açıklama", "Barkod",
                            "Özel Kod 1", "Özel Kod 2", "Özel Kod 3", "Özel Kod 4", "Özel Kod 5",
                            "Marka", "Model"
                        };
                        for (int c = 1; c <= basliklar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[2, c];
                            cell.Value = basliklar[c - 1];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 10;
                            cell.Style.Font.Color.SetColor(Color.White);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            bool zorunlu = c <= 2;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu ? Color.FromArgb(0, 112, 192) : Color.FromArgb(68, 84, 106));
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            SetBorder(cell);
                        }
                        ws.Row(2).Height = 22;
                        // Örnek satırlar
                        object[][] ornekler = {
                            new object[] { "KOL-001", "Ahşap Sandalye Kolisi", "1234567890123", "A", "", "", "", "", "AHŞAP", "MODERN" },
                            new object[] { "KOL-002", "Metal Masa Kolisi",     "",              "B", "", "", "", "", "",       ""       },
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
                 // Veri alanı
                        for (int r = 5; r <= 105; r++)
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
                        // Sütun genişlikleri
                        ws.Column(1).Width = 20;  // Malzeme Kodu
                        ws.Column(2).Width = 30;  // Açıklama
                        ws.Column(3).Width = 18;  // Barkod
                        ws.Column(4).Width = 14;  // Özel Kod 1
                        ws.Column(5).Width = 14;
                        ws.Column(6).Width = 14;
                        ws.Column(7).Width = 14;
                        ws.Column(8).Width = 14;
                        ws.Column(9).Width = 16;  // Marka
                        ws.Column(10).Width = 16;  // Model
                        ws.View.FreezePanes(3, 1);
                        package.SaveAs(new FileInfo(dlg.FileName));
                    }
                    if (XtraMessageBox.Show("Şablon oluşturuldu! Açmak ister misiniz?",
                        "Başarılı", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = dlg.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"Şablon oluşturma hatası:\n{ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // ─── Helpers ──────────────────────────────────────────────────────────
        private void RefreshRow(ProductUpdateRow row)
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
        private string GetCell(ExcelWorksheet ws, int row, int col)
        {
            try { return ws.Cells[row, col].Value?.ToString()?.Trim() ?? ""; }
            catch { return ""; }
        }
        private void SetBorder(ExcelRange range)
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