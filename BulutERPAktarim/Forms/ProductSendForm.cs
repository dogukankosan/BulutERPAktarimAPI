using BulutERPAktarim.Classes;
using BulutERPAktarim.Models;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulutERPAktarim.Forms
{
    public partial class ProductSendForm : XtraForm
    {
        #region Fields
        private List<ExcelMalzemeRow> _tumListe = new List<ExcelMalzemeRow>();
        private List<ExcelMalzemeRow> _filtreliListe = new List<ExcelMalzemeRow>();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isAktarimDevam = false;
        // Excel sütun sırası (0-based index)
        private const int COL_TUR = 0;
        private const int COL_MALZEME_KODU = 1;
        private const int COL_MALZEME_ACIKLAMA = 2;
        private const int COL_OZEL_KOD_1 = 3;
        private const int COL_OZEL_KOD_2 = 4;
        private const int COL_OZEL_KOD_3 = 5;
        private const int COL_OZEL_KOD_4 = 6;
        private const int COL_OZEL_KOD_5 = 7;
        private const int COL_MARKA = 8;
        private const int COL_MODEL = 9;
        private const int COL_BARKOD = 10;
        private const int COL_INTERNET_FIYATI = 11;
        private const int COL_TOPTAN_FIYATI = 12;
        private const int COL_PERAKENDE_FIYATI = 13;
        private const int COL_ALIS_FIYATI = 14;
        private const int COL_MIKTAR = 15;
        private const int COL_KOLI_ICIN_MIKTAR = 16;
        private const int EXCEL_MIN_COLUMN_COUNT = 17;
        #endregion

        #region Constructor
        public ProductSendForm()
        {
            InitializeComponent();
            InitializeForm();
        }
        private void InitializeForm()
        {
            // CheckBox filtreleri varsayılan açık
            checkEditTK.Checked = true;
            checkEditTM.Checked = true;
            checkEditSadecHata.Checked = false;
            // Butonları pasif başlat
            btnAktarSecilenler.Enabled = false;
            btnDisariAktar.Enabled = false;
            btnTumunuSec.Enabled = false;
            btnSecimiTemizle.Enabled = false;
            // ProgressBar sıfırla
            progressBarControl.EditValue = 0;
            // EPPlus lisans
            ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
            // MT satırlarını Tür kolonuna göre grupla (MT üstte, altında TM'ler)
            colGrupKodu.GroupIndex = 0;
            gridView.ExpandAllGroups();
            SetDurum("Hazır", false);
        }
        #endregion

        #region Dosya Seç / Yükle
        private void YukleExcel(string dosyaYolu)
        {
            try
            {
                SetDurum("Excel yükleniyor...", false);
                _tumListe.Clear();
                if (!File.Exists(dosyaYolu))
                {
                    XtraMessageBox.Show("Dosya bulunamadı!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                FileInfo fileInfo = new FileInfo(dosyaYolu);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        XtraMessageBox.Show("Excel dosyasında sayfa bulunamadı!", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int rowCount = worksheet.Dimension?.Rows ?? 0;   // ✅ burada tanımla
                    int colCount = worksheet.Dimension?.Columns ?? 0; // ✅ burada tanımla
                    // Akıllı başlangıç satırı algılama
                    string ilkHucre = GetCellValue(worksheet, 1, 1);
                    bool sablon = ilkHucre.Contains("Zorunlu") || ilkHucre.Contains("zorunlu") ||
                                  ilkHucre.Contains("Kayıt türü") || ilkHucre.Length > 30;
                    int veriBaslangic = sablon ? 3 : 2;
                    if (rowCount < veriBaslangic)
                    {
                        XtraMessageBox.Show("Excel dosyası boş veya yalnızca başlık satırı içeriyor!", "Uyarı",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (colCount < EXCEL_MIN_COLUMN_COUNT)
                    {
                        XtraMessageBox.Show(
                            $"Excel formatı hatalı!\n\nBeklenen sütun sayısı: {EXCEL_MIN_COLUMN_COUNT}\nBulunan sütun sayısı: {colCount}\n\n" +
                            "Beklenen format:\nTür | Malzeme Kodu | Malzeme Açıklaması | Özel Kod 1-5 | Marka | Model | Barkod | İnternet Fiyatı | Toptan | Perakende | Alış | Miktar | Koli İçin Miktar",
                            "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int satirNo = 0;
                    int hataliSatir = 0;
                    for (int row = veriBaslangic; row <= rowCount; row++) // ✅ veriBaslangic
                    {
                        string malzemeKodu = GetCellValue(worksheet, row, COL_MALZEME_KODU + 1);
                        if (string.IsNullOrWhiteSpace(malzemeKodu))
                            continue;
                        satirNo++;
                        string tur = GetCellValue(worksheet, row, COL_TUR + 1)?.Trim().ToUpper();
                        ExcelMalzemeRow malzeme = new ExcelMalzemeRow
                        {
                            SatirNo = satirNo,
                            Tur = tur,
                            MalzemeKodu = malzemeKodu.Trim(),
                            MalzemeAciklamasi = GetCellValue(worksheet, row, COL_MALZEME_ACIKLAMA + 1),
                            OzelKod1 = GetCellValue(worksheet, row, COL_OZEL_KOD_1 + 1),
                            OzelKod2 = GetCellValue(worksheet, row, COL_OZEL_KOD_2 + 1),
                            OzelKod3 = GetCellValue(worksheet, row, COL_OZEL_KOD_3 + 1),
                            OzelKod4 = GetCellValue(worksheet, row, COL_OZEL_KOD_4 + 1),
                            OzelKod5 = GetCellValue(worksheet, row, COL_OZEL_KOD_5 + 1),
                            Marka = GetCellValue(worksheet, row, COL_MARKA + 1),
                            Model = GetCellValue(worksheet, row, COL_MODEL + 1),
                            Barkod = GetCellValue(worksheet, row, COL_BARKOD + 1),
                            InternetFiyati = ParseDecimal(worksheet, row, COL_INTERNET_FIYATI + 1),
                            ToptanFiyati = ParseDecimal(worksheet, row, COL_TOPTAN_FIYATI + 1),
                            PerakendeFiyati = ParseDecimal(worksheet, row, COL_PERAKENDE_FIYATI + 1),
                            AlisFiyati = ParseDecimal(worksheet, row, COL_ALIS_FIYATI + 1),
                            Miktar = ParseDecimal(worksheet, row, COL_MIKTAR + 1),
                            KoliIcinMiktar = ParseDecimal(worksheet, row, COL_KOLI_ICIN_MIKTAR + 1),
                            Durum = "Bekliyor"
                        };
                        if (!malzeme.IsValid)
                        {
                            hataliSatir++;
                            List<string> hatalar = new List<string>();
                            if (string.IsNullOrWhiteSpace(malzeme.MalzemeKodu))
                                hatalar.Add("Malzeme kodu boş");
                            if (string.IsNullOrWhiteSpace(malzeme.MalzemeAciklamasi))
                                hatalar.Add("Malzeme açıklaması boş");
                            if (!malzeme.IsTakim && !malzeme.IsTicariMal)
                                hatalar.Add($"Geçersiz tür: '{malzeme.Tur}' (MT veya TM olmalı)");
                            malzeme.HataMesaji = string.Join(" | ", hatalar);
                            malzeme.Durum = "Format Hatası";
                        }
                        _tumListe.Add(malzeme);
                    }
                    // GrupKodu ata
                    string aktifMtKrupKodu = null;
                    foreach (ExcelMalzemeRow m in _tumListe)
                    {
                        if (m.IsTakim)
                            aktifMtKrupKodu = m.MalzemeKodu;
                        m.GrupKodu = aktifMtKrupKodu ?? m.MalzemeKodu;
                    }
                    UygulaMeFiltre();
                    _ = LogoKontrolAsync(_tumListe.ToList());
                    GuncelleSayaclar();
                    btnAktarSecilenler.Enabled = _tumListe.Any(x => x.IsValid);
                    btnDisariAktar.Enabled = _tumListe.Any(x => x.IsTakim);
                    btnTumunuSec.Enabled = true;
                    btnSecimiTemizle.Enabled = true;
                    colGrupKodu.GroupIndex = 0;
                    gridView.ExpandAllGroups();
                    string mesaj = $"Excel yüklendi: {satirNo} satır ({hataliSatir} hatalı)" +
                                   (sablon ? " [Şablon formatı]" : " [Standart format]");
                    SetDurum(mesaj, false);
                    if (hataliSatir > 0)
                    {
                        XtraMessageBox.Show(
                            $"{satirNo} satırdan {hataliSatir} tanesi format hatası içeriyor.\nHatalı satırlar kırmızı ile gösterilmektedir.",
                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Excel yükleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetDurum("Excel yükleme hatası!", true);
                _ = TextLog.LogToSQLiteAsync($"❌ Excel yükleme hatası: {ex}");
            }
        }
        #endregion

        #region Logo Kontrol (Excel Yüklenince)
        private async Task LogoKontrolAsync(List<ExcelMalzemeRow> liste)
        {
            try
            {
                SetDurum("Logo'daki mevcut malzemeler kontrol ediliyor...", false);
                progressBarControl.Properties.Maximum = liste.Count;
                progressBarControl.EditValue = 0;
                btnAktarSecilenler.Enabled = false;
                btnTumunuSec.Enabled = false;
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    SetDurum($"Logo bağlantısı kurulamadı: {tokenResult.ErrorMessage}", true);
                    return;
                }
                string accessToken = tokenResult.AccessToken;
                SemaphoreSlim semaphore = new SemaphoreSlim(5, 5);
                int tamamlanan = 0;
                var tasks = liste.Select(async row =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        // Malzeme kodu kontrolü
                        var result = await BulutERPService.CheckMalzemeExistsAsync(row.MalzemeKodu, accessToken);
                        if (result.Success)
                        {
                            row.LogodaVar = result.Exists;
                            row.LogoKontrolEdildi = true;
                            if (result.Exists)
                                row.Sec = false;
                        }
                        else
                           row.LogoKontrolEdildi = false;
                        // Barkod kontrolü — sadece barkod dolu ve malzeme Logo'da yoksa kontrol et
                        if (!string.IsNullOrWhiteSpace(row.Barkod) && !row.LogodaVar)
                        {
                            var barkodResult = await BulutERPService.CheckBarkodExistsAsync(row.Barkod, accessToken);
                            if (barkodResult.Success && barkodResult.Exists)
                            {
                                row.Durum = "Hata";
                                row.HataMesaji = $"Barkod '{row.Barkod}' zaten kayıtlı → {barkodResult.ExistingCode}";
                                row.Sec = false;
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        int done = Interlocked.Increment(ref tamamlanan);
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                progressBarControl.EditValue = done;
                                SetDurum($"Logo kontrol ediliyor... {done}/{liste.Count}", false);
                                gridView.RefreshData();
                                GuncelleSayaclar();
                            }));
                        }
                        else
                        {
                            progressBarControl.EditValue = done;
                            SetDurum($"Logo kontrol ediliyor... {done}/{liste.Count}", false);
                            gridView.RefreshData();
                            GuncelleSayaclar();
                        }
                    }
                });
                await Task.WhenAll(tasks);
                int varSayisi = liste.Count(x => x.LogodaVar);
                int yokSayisi = liste.Count(x => x.LogoKontrolEdildi && !x.LogodaVar);
                int barkodHata = liste.Count(x => x.Durum == "Hata" && x.HataMesaji.Contains("Barkod"));
                progressBarControl.EditValue = 0;
                string sonMesaj = $"Logo kontrolü tamamlandı — ✅ Mevcut: {varSayisi} | 🆕 Yeni: {yokSayisi}";
                if (barkodHata > 0)
                    sonMesaj += $" | ⚠ Barkod Çakışması: {barkodHata}";
                SetDurum(sonMesaj, barkodHata > 0);
            }
            catch (Exception ex)
            {
                progressBarControl.EditValue = 0;
                SetDurum($"Logo kontrol hatası: {ex.Message}", true);
                await TextLog.LogToSQLiteAsync($"❌ LogoKontrolAsync hatası: {ex}");
            }
            finally
            {
                btnAktarSecilenler.Enabled = _tumListe.Any(x => x.IsValid && !x.LogodaVar);
                btnTumunuSec.Enabled = true;
            }
        }
        #endregion

        #region Grid / Filtre
        private void UygulaMeFiltre()
        {
            bool mtAktif = checkEditTK.Checked;
            bool tmAktif = checkEditTM.Checked;
            bool sadecHata = checkEditSadecHata.Checked;
            _filtreliListe = _tumListe.Where(x =>
            {
                bool turUygun = (mtAktif && x.IsTakim) || (tmAktif && x.IsTicariMal) ||
                                (!x.IsTakim && !x.IsTicariMal);
                bool hataUygun = !sadecHata || x.Durum == "Format Hatası" || x.Durum == "Hata";
                return turUygun && hataUygun;
            }).ToList();
            gridControl.DataSource = null;
            gridControl.DataSource = _filtreliListe;
            // Gruplama her filtre sonrası yeniden uygula
            colGrupKodu.GroupIndex = 0;
            gridView.ExpandAllGroups();
            GuncelleSayaclar();
        }
        private void CheckEditFiltre_Changed(object sender, EventArgs e)
        {
            UygulaMeFiltre();
        }
        private void GuncelleSayaclar()
        {
            int toplam = _tumListe.Count;
            int secili = _tumListe.Count(x => x.Sec);
            int hata = _tumListe.Count(x => x.Durum == "Format Hatası" || x.Durum == "Hata");
            labelControlToplam.Text = $"Toplam: {toplam}";
            labelControlSecili.Text = $"Seçili: {secili}";
            labelControlHata.Text = $"Hata: {hata}";
            labelControlHata.Appearance.ForeColor = hata > 0 ? Color.Red : Color.Green;
        }// 5) Yardımcı metod — tek yerde tanımla, 4 metod da bunu kullansın
        private bool IsKilitliSatir(ExcelMalzemeRow row)
        {
            return row.LogodaVar ||
                   row.Durum == "Hata" ||
                   row.Durum == "Format Hatası" ||
                   row.Durum == "Aktarıldı" ||
                   row.Durum == "Aktarıldı (Fiyat Hatası)" ||
                   row.Durum == "Aktarıldı (Fiş Hatası)" ||
                   row.Durum == "Zaten Var";
        }
        private void GridView_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName == "Sec")
            {
                if (gridView.GetRow(e.RowHandle) is ExcelMalzemeRow row && IsKilitliSatir(row))
                {
                    var readOnly = new RepositoryItemCheckEdit();
                    readOnly.ReadOnly = true;
                    e.RepositoryItem = readOnly;
                }
            }
        }
        private void GridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (gridView.GetRow(e.RowHandle) is ExcelMalzemeRow row)
            {
                // LogodaVar kontrolü — her durumun üzerinde öncelikli
                if (row.LogodaVar)
                {
                    e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                    e.Appearance.ForeColor = Color.DarkGreen;
                    return; // diğer case'lere gitme
                }
                switch (row.Durum)
                {
                    case "Aktarıldı":
                        e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                        e.Appearance.ForeColor = Color.DarkGreen;
                        break;
                    case "Hata":
                    case "Format Hatası":
                        e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                        e.Appearance.ForeColor = Color.DarkRed;
                        break;
                    case "Aktarılıyor...":
                        e.Appearance.BackColor = Color.FromArgb(255, 255, 200);
                        e.Appearance.ForeColor = Color.DarkOrange;
                        break;
                    case "Zaten Var":
                        e.Appearance.BackColor = Color.FromArgb(220, 220, 255);
                        e.Appearance.ForeColor = Color.DarkBlue;
                        break;
                }
            }
        }
        #endregion

        #region Seç / Temizle
        private void BtnTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ExcelMalzemeRow row in _filtreliListe.Where(x => x.IsValid && !IsKilitliSatir(x)))
                row.Sec = true;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BtnSecimiTemizle_Click(object sender, EventArgs e)
        {
            foreach (ExcelMalzemeRow row in _tumListe)
                row.Sec = false;
            gridView.RefreshData();
            GuncelleSayaclar();
        }
        private void BarItemTemizle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_tumListe.Count == 0) return;
            if (XtraMessageBox.Show("Liste temizlensin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _tumListe.Clear();
                _filtreliListe.Clear();
                gridControl.DataSource = null;
                textEditDosyaYolu.Text = "";
                btnAktarSecilenler.Enabled = false;
                btnDisariAktar.Enabled = false;
                btnTumunuSec.Enabled = false;
                btnSecimiTemizle.Enabled = false;
                progressBarControl.EditValue = 0;
                GuncelleSayaclar();
                SetDurum("Liste temizlendi.", false);
            }
        }
        #endregion

        #region Aktarım
        private void BtnAktarSecilenler_Click(object sender, EventArgs e)
        {
            if (_isAktarimDevam)
            {
                _cancellationTokenSource?.Cancel();
                btnAktarSecilenler.Text = "🚀  Seçilenleri Logo'ya Aktar";
                SetDurum("İptal ediliyor...", false);
                return;
            }
            // LogodaVar olanları seçimden düşür
            foreach (ExcelMalzemeRow row in _tumListe.Where(x => x.LogodaVar))
                row.Sec = false;
            List<ExcelMalzemeRow> secilenler = _tumListe.Where(x => x.Sec && x.IsValid && !x.LogodaVar).ToList();
            if (secilenler.Count == 0)
            {
                XtraMessageBox.Show("Aktarılacak geçerli satır seçilmedi!\n\nLütfen önce satırları seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gridView.RefreshData();
                return;
            }
            // Kaç tanesi Logo'da vardı, kaçı gönderilecek bilgisi
            int logodaVarSayisi = _tumListe.Count(x => x.LogodaVar);
            string mesaj = logodaVarSayisi > 0
                ? $"{secilenler.Count} adet malzeme Logo'ya aktarılacak.\n⚠ {logodaVarSayisi} adet Logo'da mevcut olduğu için atlandı.\n\nDevam edilsin mi?"
                : $"{secilenler.Count} adet malzeme Logo'ya aktarılacak.\n\nDevam edilsin mi?";
            if (XtraMessageBox.Show(mesaj, "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            gridView.RefreshData();
            _ = AktarAsync(secilenler);
        }
        private async Task AktarAsync(List<ExcelMalzemeRow> secilenler)
        {
            _isAktarimDevam = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            btnAktarSecilenler.Text = "⛔  Aktarımı İptal Et";
            btnDosyaSec.Enabled = false;
            btnDisariAktar.Enabled = false;
            progressBarControl.Properties.Maximum = secilenler.Count;
            progressBarControl.EditValue = 0;
            int basarili = 0, hatali = 0, zatenVar = 0;
            try
            {
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    XtraMessageBox.Show($"Token alınamadı:\n{tokenResult.ErrorMessage}",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string accessToken = tokenResult.AccessToken;
                for (int i = 0; i < secilenler.Count; i++)
                {
                    if (token.IsCancellationRequested) break;
                    ExcelMalzemeRow row = secilenler[i];
                    row.Durum = "Aktarılıyor...";
                    row.HataMesaji = "";
                    RefreshRow(row);
                    SetDurum($"[{i + 1}/{secilenler.Count}] {row.MalzemeKodu} aktarılıyor...", false);
                    try
                    {
                        var existsResult = await BulutERPService.CheckMalzemeExistsAsync(row.MalzemeKodu, accessToken);
                        if (!existsResult.Success)
                        {
                            row.Durum = "Hata";
                            row.HataMesaji = $"Kontrol hatası: {existsResult.ErrorMessage}";
                            hatali++;
                            RefreshRow(row);
                            progressBarControl.EditValue = i + 1;
                            continue;
                        }
                        if (existsResult.Exists)
                        {
                            if (row.IsTakim)
                            {
                                var altExists = await BulutERPService.CheckMalzemeExistsAsync(row.MalzemeKodu + "-ADET", accessToken);
                                if (altExists.Success && altExists.Exists)
                                {
                                    row.Durum = "Zaten Var";
                                    row.HataMesaji = "Logo'da zaten mevcut, atlandı.";
                                    zatenVar++;
                                }
                                else
                                {
                                    row.Durum = "Hata";
                                    row.HataMesaji = $"Üst malzeme ({row.MalzemeKodu}) mevcut fakat alt malzeme ({row.MalzemeKodu}-ADET) eksik! Elle kontrol edin.";
                                    hatali++;
                                }
                            }
                            else
                            {
                                row.Durum = "Zaten Var";
                                row.HataMesaji = "Logo'da zaten mevcut, atlandı.";
                                zatenVar++;
                            }
                            RefreshRow(row);
                            progressBarControl.EditValue = i + 1;
                            continue;
                        }
                        var malzemeResult = await BulutERPService.CreateMalzemeAsync(row, accessToken);

                        if (!malzemeResult.Success)
                        {
                            row.Durum = "Hata";
                            row.HataMesaji = malzemeResult.ErrorMessage;
                            hatali++;
                            RefreshRow(row);
                            progressBarControl.EditValue = i + 1;
                            continue;
                        }
                        string fiyatKodu = row.MalzemeKodu;
                        var fiyatResult = await BulutERPService.CreateFiyatKartlariAsync(
                            fiyatKodu,
                            row.AlisFiyati,
                            row.InternetFiyati,
                            row.ToptanFiyati,
                            row.PerakendeFiyati,
                            accessToken,
                            uomRef: row.IsTakim ? 25 : 23
                        );
                        if (!fiyatResult.Success)
                        {
                            row.Durum = "Aktarıldı (Fiyat Hatası)";
                            row.HataMesaji = $"Malzeme oluşturuldu fakat fiyat hatası: {fiyatResult.ErrorMessage}";
                        }
                        else
                        {
                            row.Durum = "Aktarıldı";
                            row.LogodaVar = true;  // ← BU SATIRI EKLE
                            row.Sec = false;        // ← BU SATIRI EKLE
                            row.HataMesaji = "";
                        }
                        basarili++;
                    }
                    catch (Exception ex)
                    {
                        row.Durum = "Hata";
                        row.HataMesaji = ex.Message;
                        hatali++;
                        await TextLog.LogToSQLiteAsync($"❌ Aktarım hatası ({row.MalzemeKodu}): {ex}"); // log'a full stack
                    }
                    RefreshRow(row);
                    progressBarControl.EditValue = i + 1;
                    GuncelleSayaclar();
                    if ((i + 1) % 5 == 0)
                    {
                        var yeniToken = await BulutERPService.EnsureValidTokenAsync();
                        if (yeniToken.Success)
                            accessToken = yeniToken.AccessToken;
                    }
                }
                // Üretim fişi
                var fisRows = secilenler.Where(r => r.Miktar > 0 && r.Durum == "Aktarıldı").ToList();
                if (fisRows.Count > 0)
                {
                    SetDurum("Üretim fişi oluşturuluyor...", false);
                    var fisResult = await BulutERPService.CreateUretimdenGirisFisiAsync(fisRows, accessToken);
                    if (!fisResult.Success)
                    {
                        await TextLog.LogToSQLiteAsync($"⚠ Üretim fişi hatası: {fisResult.ErrorMessage}");
                        foreach (ExcelMalzemeRow r in fisRows)
                        {
                            r.Durum = "Aktarıldı (Fiş Hatası)";
                            r.HataMesaji = $"Fiş hatası: {fisResult.ErrorMessage}";
                            RefreshRow(r);
                        }
                    }
                    else
                    {
                        foreach (ExcelMalzemeRow r in fisRows)
                        {
                            r.HataMesaji = $"Fiş: {fisResult.FisNo}";
                            RefreshRow(r);
                        }
                    }
                }
                string sonucMesaj = $"Aktarım tamamlandı!\n\n✅ Başarılı: {basarili}\n⚠ Zaten Var: {zatenVar}\n❌ Hatalı: {hatali}";
                SetDurum($"Tamamlandı — Başarılı: {basarili} | Zaten Var: {zatenVar} | Hatalı: {hatali}", false);
                XtraMessageBox.Show(sonucMesaj, "Aktarım Sonucu",
                    MessageBoxButtons.OK, hatali > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);         
            }
            catch (OperationCanceledException)
            {
                SetDurum($"Aktarım iptal edildi. Başarılı: {basarili} | Hatalı: {hatali}", false);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                await TextLog.LogToSQLiteAsync($"❌ AktarAsync exception: {ex}");
            }
            finally
            {
                _isAktarimDevam = false;
                btnAktarSecilenler.Text = "🚀  Seçilenleri Logo'ya Aktar";
                btnDosyaSec.Enabled = true;
                btnDisariAktar.Enabled = _tumListe.Any(x => x.IsTakim);
                GuncelleSayaclar();
            }
        }
        #endregion

        #region Dışarı Aktar (Logo Aktarım Excel)
        private void BtnDisariAktar_Click(object sender, EventArgs e)
        {
            if (!_tumListe.Any(x => x.IsTakim))
            {
                XtraMessageBox.Show("Listede hiç MT (Malzeme Takımı) bulunamadı!\nÖnce Excel yükleyin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Logo Aktarım Excel - Kayıt Yeri Seç";
                dlg.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                dlg.FileName = "LogoAktarim_Malzeme_Takimleri.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK)
                   return;
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
                    int toplamSatir = 0;
                    using (var package = new ExcelPackage())
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.Add("Malzeme");
                        // Satır 1: Açıklama başlıkları
                        ws.Cells[1, 1].Value =
                            "Kayıt türü (1: Ticari mal, 3: Depozitolu mal, 4: Varlık, 10: Hammadde, " +
                            "11: Yarı Mamul, 12: Mamul, 13: Tüketim malı, 16: Malzeme takımı, " +
                            "20: Malzeme Sınıfı, 30: Hizmet)\n" +
                            "Tür: Sayısal Değer, Örnek: 1,2,3,4,...\n" +
                            "*Doldurulması zorunlu alandır";
                        ws.Cells[1, 2].Value =
                            "Kayıt kodu (Tekil alandır)\nTür: Metin\n* Doldurulması zorunlu alandır";
                        ws.Cells[1, 3].Value =
                            "Birim Seti\nKayıt kodu (Tekil alandır)\nTür: Metin\n* Doldurulması zorunlu alandır";
                        ws.Cells[1, 4].Value =
                            "Kayıt kodu (Tekil alandır)\nTür: Metin\n* Doldurulması zorunlu alandır";
                        ws.Cells[1, 5].Value = "Doldurulması zorunlu alandır";
                        using (ExcelRange r = ws.Cells[1, 1, 1, 5])
                        {
                            r.Style.WrapText = true;
                            r.Style.Font.Size = 9;
                            r.Style.Font.Name = "Arial";
                            r.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 204));
                            SetBorder(r);
                        }
                        ws.Row(1).Height = 70;
                        // Satır 2: Kolon başlıkları
                        string[] kolonlar = { "Card_Type", "Code", "UnitSetInfo.Code", "SetItemAssgs.Quantity", "SetItemAssgs.ItemCodeName.Code" };
                        for (int c = 1; c <= kolonlar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[2, c];
                            cell.Value = kolonlar[c - 1];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Name = "Arial";
                            cell.Style.Font.Size = 10;
                            cell.Style.Font.Color.SetColor(Color.White);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(192, 0, 0));
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            SetBorder(ws.Cells[2, c, 2, c]);
                        }
                        ws.Row(2).Height = 20;
                        // Veri satırları: MT altındaki TM'leri eşleştir
                        int excelSatir = 3;
                        string aktifMtKodu = null;
                        foreach (ExcelMalzemeRow row in _tumListe.OrderBy(x => x.SatirNo))
                        {
                            if (row.IsTakim)
                                aktifMtKodu = row.MalzemeKodu;
                            else if (row.IsTicariMal && aktifMtKodu != null)
                            {
                                decimal quantity = row.KoliIcinMiktar > 0 ? row.KoliIcinMiktar : row.Miktar;
                                ws.Cells[excelSatir, 1].Value = 16;
                                ws.Cells[excelSatir, 2].Value = aktifMtKodu;
                                ws.Cells[excelSatir, 3].Value = "06";
                                ws.Cells[excelSatir, 4].Value = quantity;
                                ws.Cells[excelSatir, 5].Value = row.MalzemeKodu;
                                using (ExcelRange r = ws.Cells[excelSatir, 1, excelSatir, 5])
                                {
                                    r.Style.Font.Name = "Arial";
                                    r.Style.Font.Size = 10;
                                    r.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                    if (excelSatir % 2 == 0)
                                    {
                                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                                    }
                                    SetBorder(r);
                                }
                                ws.Row(excelSatir).Height = 18;
                                excelSatir++;
                                toplamSatir++;
                            }
                        }
                        ws.Column(1).Width = 14;
                        ws.Column(2).Width = 22;
                        ws.Column(3).Width = 20;
                        ws.Column(4).Width = 24;
                        ws.Column(5).Width = 32;
                        package.SaveAs(new FileInfo(dlg.FileName));
                    }
                    // YENİ - bununla değiştir:
                    if (XtraMessageBox.Show(
                        $"Logo aktarım Excel'i oluşturuldu!\n\n✅ Toplam satır: {toplamSatir}\n📄 Dosya: {dlg.FileName}\n\nDosyayı açmak ister misiniz?",
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
                    XtraMessageBox.Show($"Dosya oluşturma hatası:\n{ex.Message}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Helpers
        private void RefreshRow(ExcelMalzemeRow row)
        {
            if (InvokeRequired) { Invoke(new Action(() => RefreshRow(row))); return; }
            int index = _filtreliListe.IndexOf(row);
            if (index >= 0)
                gridView.RefreshRow(index);
        }
        private void SetDurum(string mesaj, bool hata)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetDurum(mesaj, hata))); return; }
            labelControlDurum.Text = mesaj;
            labelControlDurum.Appearance.ForeColor = hata ? Color.Red : Color.Black;
        }
        private string GetCellValue(ExcelWorksheet ws, int row, int col)
        {
            try { return ws.Cells[row, col].Value?.ToString()?.Trim() ?? ""; }
            catch { return ""; }
        }
        private decimal ParseDecimal(ExcelWorksheet ws, int row, int col)
        {
            try
            {
                object val = ws.Cells[row, col].Value;
                if (val == null) return 0;
                // EPPlus sayısal değerleri double döndürür — direkt cast et
                if (val is double d) return (decimal)d;
                if (val is int i) return i;
                if (val is decimal dec) return dec;
                // String olarak gelirse parse et
                string strVal = val.ToString().Trim();
                if (string.IsNullOrWhiteSpace(strVal)) return 0;
                if (decimal.TryParse(strVal.Replace(",", "."),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal result))
                    return result;
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
        }
        #endregion

        private void ProductSendForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Escape)
                this.Close();
        }
        private void GridView_ShowingEditor(object sender, CancelEventArgs e)
        {
            if (gridView.FocusedColumn?.FieldName == "Sec")
            {
                if (gridView.GetFocusedRow() is ExcelMalzemeRow row && IsKilitliSatir(row))
                    e.Cancel = true;
            }
        }
        private void GridView_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Sec")
            {
                if (gridView.GetRow(e.RowHandle) is ExcelMalzemeRow row && IsKilitliSatir(row))
                    gridView.SetRowCellValue(e.RowHandle, e.Column, false);
            }
        }
        private void GridView_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            for (int i = 0; i < gridView.RowCount; i++)
            {
                if (gridView.IsRowSelected(i))
                {
                    if (gridView.GetRow(i) is ExcelMalzemeRow row && IsKilitliSatir(row))
                    {
                        gridView.UnselectRow(i);
                        row.Sec = false;
                    }
                }
            }
        }
        private void ProductSendForm_Load(object sender, EventArgs e)
        {
            gridView.CustomRowCellEdit += GridView_CustomRowCellEdit;
            gridView.CustomRowCellEditForEditing += GridView_CustomRowCellEdit;
            gridView.ShowingEditor += GridView_ShowingEditor;
            gridView.CellValueChanging += GridView_CellValueChanging; // ekle
            gridView.SelectionChanged += GridView_SelectionChanged;
        }
        private void btn_Group_Click(object sender, EventArgs e)
        {
            if (colGrupKodu.GroupIndex >= 0)
            {
                // Şu an gruplu — grubu çöz
                colGrupKodu.GroupIndex = -1;
                btn_Group.Text = "🗂 Grupla";
            }
            else
            {
                // Şu an grupsuz — grupla
                colGrupKodu.GroupIndex = 0;
                gridView.ExpandAllGroups();
                btn_Group.Text = "🔓 Grubu Çöz";
            }
        }
        private void btn_ExcelSablon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Excel Şablonu Kaydet";
                dlg.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                dlg.FileName = "Malzeme_Aktarim_Sablonu.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("BulutERPAktarim");
                    using (var package = new ExcelPackage())
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.Add("Malzeme");
                        // ── Açıklama satırı (Satır 1) ────────────────────────────
                        string[] aciklamalar = new[]
                        {
                    "Kayıt türü\n✅ Zorunlu\n\nMT = Malzeme Takımı (Koli)\nTM = Ticari Mal (Adet)\n\nSadece MT veya TM yazılmalıdır.",
                    "Malzeme kodu\n✅ Zorunlu\n\nTekil olmalıdır.\nÖrn: ABC-001",
                    "Malzeme açıklaması\n✅ Zorunlu\n\nÜrün adı veya açıklaması.\nÖrn: Ahşap Sandalye",
                    "Özel Kod 1\n⬜ Opsiyonel\n\nSerbest kullanım alanı.\nÖrn: Kategori kodu",
                    "Özel Kod 2\n⬜ Opsiyonel\n\nSerbest kullanım alanı.",
                    "Özel Kod 3\n⬜ Opsiyonel\n\nSerbest kullanım alanı.",
                    "Özel Kod 4\n⬜ Opsiyonel\n\nSerbest kullanım alanı.",
                    "Özel Kod 5\n⬜ Opsiyonel\n\nSerbest kullanım alanı.",
                    "Marka\n⬜ Opsiyonel\n\nMarka kodu.\nYoksa boş bırakın.\nÖrn: SAMSUNG",
                    "Model\n⬜ Opsiyonel\n\nModel kodu.\nYoksa boş bırakın.\nÖrn: GALAXY-S24",
                    "Barkod\n⬜ Opsiyonel\n\nÜrün barkod numarası.\nÖrn: 8690000123456",
                    "İnternet Fiyatı\n⬜ Opsiyonel\n\nSayısal değer giriniz.\nÖrn: 299.90",
                    "Toptan Fiyatı\n⬜ Opsiyonel\n\nSayısal değer giriniz.\nÖrn: 250.00",
                    "Perakende Fiyatı\n⬜ Opsiyonel\n\nSayısal değer giriniz.\nÖrn: 349.90",
                    "Alış Fiyatı\n⬜ Opsiyonel\n\nSayısal değer giriniz.\nÖrn: 180.00",
                    "Fiili Stok\n⬜ Opsiyonel\n\nÜretimden giriş fişi için\nkullanılır. Sayısal değer.\nÖrn: 100",
                    "Koli İçi Miktar\n⬜ Opsiyonel\n\nSadece MT satırları için.\nKoli içindeki ürün adedi.\nÖrn: 12"
                };
                        for (int c = 1; c <= aciklamalar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[1, c];
                            cell.Value = aciklamalar[c - 1];
                            cell.Style.WrapText = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 9;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            // Zorunlu alanlar sarı, opsiyonel açık gri
                            bool zorunlu = c <= 3;
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu
                                    ? System.Drawing.Color.FromArgb(255, 255, 153)   // sarı
                                    : System.Drawing.Color.FromArgb(242, 242, 242)); // açık gri
                            SetSablonBorder(cell);
                        }
                        ws.Row(1).Height = 90;
                        // ── Başlık satırı (Satır 2) ──────────────────────────────
                        string[] basliklar = new[]
                        {
                    "Tür", "Malzeme Kodu", "Malzeme Açıklaması",
                    "Özel Kod 1", "Özel Kod 2", "Özel Kod 3", "Özel Kod 4", "Özel Kod 5",
                    "Marka", "Model", "Barkod",
                    "İnternet Fiyatı", "Toptan Fiyatı", "Perakende Fiyatı", "Alış Fiyatı",
                    "Fiili Stok", "Koli İçi Miktar"
                };

                        for (int c = 1; c <= basliklar.Length; c++)
                        {
                            ExcelRange cell = ws.Cells[2, c];
                            cell.Value = basliklar[c - 1];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Name = "Segoe UI";
                            cell.Style.Font.Size = 10;
                            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            // Zorunlu = koyu kırmızı, opsiyonel = koyu gri
                            bool zorunlu = c <= 3;
                            cell.Style.Fill.BackgroundColor.SetColor(
                                zorunlu
                                    ? System.Drawing.Color.FromArgb(192, 0, 0)      // koyu kırmızı
                                    : System.Drawing.Color.FromArgb(68, 84, 106));  // koyu gri-mavi
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            SetSablonBorder(cell);
                        }
                        ws.Row(2).Height = 22;
                        // ── Örnek veri satırları (Satır 3-6) ─────────────────────
                        object[][] ornekler = new[]
                        {
                    new object[] { "MT", "KOL-001", "Ahşap Sandalye Koli", "EV", "", "", "", "", "AHSAP", "SANDALYE-A", "8690000000001", 299.90, 250.00, 349.90, 180.00, 50, 4 },
                    new object[] { "TM", "TM-001",  "Ahşap Sandalye Adet", "EV", "", "", "", "", "AHSAP", "SANDALYE-A", "8690000000002", 89.90,  75.00,  99.90,  45.00, 0,  0 },
                    new object[] { "MT", "KOL-002", "Metal Masa Koli",     "OF", "", "", "", "", "METAL", "MASA-B",     "8690000000003", 599.00, 520.00, 699.00, 380.00, 10, 2 },
                    new object[] { "TM", "TM-002",  "Metal Masa Adet",     "OF", "", "", "", "", "METAL", "MASA-B",     "8690000000004", 310.00, 270.00, 360.00, 195.00, 0,  0 },
                };
                        for (int r = 0; r < ornekler.Length; r++)
                        {
                            int excelRow = r + 3;
                            bool isMT = ornekler[r][0].ToString() == "MT";
                            for (int c = 1; c <= ornekler[r].Length; c++)
                            {
                                ExcelRange cell = ws.Cells[excelRow, c];
                                cell.Value = ornekler[r][c - 1];
                                cell.Style.Font.Name = "Segoe UI";
                                cell.Style.Font.Size = 10;
                                cell.Style.Font.Italic = true;
                                cell.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(89, 89, 89));
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(
                                    isMT
                                        ? System.Drawing.Color.FromArgb(255, 242, 204)  // MT → açık turuncu
                                        : System.Drawing.Color.FromArgb(226, 239, 218)); // TM → açık yeşil
                                cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                SetSablonBorder(cell);
                            }
                            ws.Row(excelRow).Height = 18;
                        }
                        // ── Boş veri satırları (Satır 7-106) ─────────────────────
                        for (int r = 7; r <= 106; r++)
                        {
                            for (int c = 1; c <= basliklar.Length; c++)
                            {
                                ExcelRange cell = ws.Cells[r, c];
                                cell.Style.Font.Name = "Segoe UI";
                                cell.Style.Font.Size = 10;
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(
                                    r % 2 == 0
                                        ? System.Drawing.Color.White
                                        : System.Drawing.Color.FromArgb(249, 249, 249));
                                SetSablonBorder(cell);
                            }
                            ws.Row(r).Height = 18;
                        }
                        // ── Tür kolonuna veri doğrulama (A7:A106) ────────────────
                        var validation = ws.DataValidations.AddListValidation("A7:A106");
                        validation.ShowErrorMessage = true;
                        validation.ErrorTitle = "Geçersiz Değer";
                        validation.Error = "Sadece MT veya TM girebilirsiniz.";
                        validation.Formula.Values.Add("MT");
                        validation.Formula.Values.Add("TM");
                        // ── Kolon genişlikleri ────────────────────────────────────
                        int[] genislikler = { 8, 20, 30, 14, 14, 14, 14, 14, 14, 14, 18, 16, 16, 16, 14, 12, 16 };
                        for (int c = 1; c <= genislikler.Length; c++)
                            ws.Column(c).Width = genislikler[c - 1];
                        // ── Satır 2'yi dondur (başlık sabit kalsın) ──────────────
                        ws.View.FreezePanes(3, 1);
                        package.SaveAs(new System.IO.FileInfo(dlg.FileName));
                    }
                    if (XtraMessageBox.Show(
                            "Excel şablonu başarıyla oluşturuldu!\n\nDosyayı açmak ister misiniz?",
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
                    XtraMessageBox.Show($"Şablon oluşturma hatası:\n{ex}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void SetSablonBorder(ExcelRange range)
        {
            var b = range.Style.Border;
            b.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            b.Top.Color.SetColor(System.Drawing.Color.FromArgb(189, 189, 189));
            b.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(189, 189, 189));
            b.Left.Color.SetColor(System.Drawing.Color.FromArgb(189, 189, 189));
            b.Right.Color.SetColor(System.Drawing.Color.FromArgb(189, 189, 189));
        }
        private void btnDosyaSec_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Excel Dosyası Seç";
                dlg.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|Tüm Dosyalar (*.*)|*.*";
                dlg.FilterIndex = 1;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textEditDosyaYolu.Text = dlg.FileName;
                    SetDurum($"Dosya seçildi: {Path.GetFileName(dlg.FileName)}", false);
                    YukleExcel(textEditDosyaYolu.Text);
                }
            }
        }
    }
}