using System;

namespace BulutERPAktarim.Models
{
    /// <summary>
    /// Excel'den okunan malzeme satırı modeli
    /// Sütunlar: Tür | Malzeme Kodu | Malzeme Açıklaması | Özel Kod 1-5 | Marka | Model | Barkod |
    ///           İnternet Fiyatı | Toptan Fiyatı | Perakende Fiyatı | Alış Fiyatı | Miktar | Koli İçin Miktar
    /// </summary>
    public class ExcelMalzemeRow
    {
        /// <summary>MT = Malzeme Takımı (cardType=1, unitSet=06/KOLİ) | TM = Ticari Mal (cardType=1, unitSet=05/ADET)</summary>
        public string Tur { get; set; }
        public string MalzemeKodu { get; set; }
        public string MalzemeAciklamasi { get; set; }
        public string OzelKod1 { get; set; }   // auxCode
        public string OzelKod2 { get; set; }   // auxiliaryCode2
        public string OzelKod3 { get; set; }   // auxiliaryCode3
        public string OzelKod4 { get; set; }   // auxiliaryCode4
        public string OzelKod5 { get; set; }   // auxiliaryCode5
        public string Marka { get; set; }      // brandCode
        public string Model { get; set; }      // modelCode
        public string Barkod { get; set; }
        public decimal InternetFiyati { get; set; }
        public decimal ToptanFiyati { get; set; }
        public decimal PerakendeFiyati { get; set; }
        public decimal AlisFiyati { get; set; }
        public decimal Miktar { get; set; }
        public decimal KoliIcinMiktar { get; set; }

        /// <summary>Logo'da mevcut mu? (Excel yüklenince otomatik kontrol edilir)</summary>
        public bool LogodaVar { get; set; } = false;

        /// <summary>Logo kontrolü yapıldı mı?</summary>
        public bool LogoKontrolEdildi { get; set; } = false;

        /// <summary>Logo kontrol ikonu — grid kolonunda gösterilir</summary>
        public string LogoDurumIkon =>
            !LogoKontrolEdildi ? "⏳" :
             LogodaVar ? "✅ Logo'da Var" : "";

        /// <summary>
        /// Grid gruplama için kullanılır.
        /// MT → kendi MalzemeKodu | TM → üstündeki MT'nin MalzemeKodu
        /// </summary>
        public string GrupKodu { get; set; }

        /// <summary>
        /// Satırın kilitli olup olmadığını belirler.
        /// Kilitli satırlar seçilemez ve tekrar gönderilemez.
        /// </summary>
        public bool IsKilitli =>
            LogodaVar ||
            Durum == "Aktarıldı" ||
            Durum == "Aktarıldı (Fiyat Hatası)" ||
            Durum == "Aktarıldı (Fiş Hatası)" ||
            Durum == "Zaten Var" ||
            Durum == "Hata" ||
            Durum == "Format Hatası";

        // Grid seçim (CheckBox) — kilitli satırlar seçilemiyor
        private bool _sec = false;
        public bool Sec
        {
            get => _sec;
            set
            {
                // Kilitli satıra true set etmeye çalışıyorsa engelle
                if (value && IsKilitli)
                    return;
                _sec = value;
            }
        }

        // Aktarım durumu (UI için)
        private string _durum = "Bekliyor";
        public string Durum
        {
            get => _durum;
            set
            {
                _durum = value;
                // Durum kilitli bir değere geçince seçimi otomatik kaldır
                if (IsKilitli)
                    _sec = false;
            }
        }

        public string HataMesaji { get; set; }
        public int SatirNo { get; set; }

        /// <summary>MT mi? (Malzeme Takımı)</summary>
        public bool IsTakim => Tur?.Trim().ToUpper() == "MT";

        /// <summary>TM mi? (Ticari Mal)</summary>
        public bool IsTicariMal => Tur?.Trim().ToUpper() == "TM";

        /// <summary>Geçerli satır mı?</summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(MalzemeKodu) &&
            !string.IsNullOrWhiteSpace(MalzemeAciklamasi) &&
            (IsTakim || IsTicariMal);
    }
}