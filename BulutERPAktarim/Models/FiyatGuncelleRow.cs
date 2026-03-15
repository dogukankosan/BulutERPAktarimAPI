using System.ComponentModel;

namespace BulutERPAktarim.Models
{
    /// <summary>
    /// Fiyat güncelleme formunda grid'e bağlanan satır modeli
    /// </summary>
    public class FiyatGuncelleRow : INotifyPropertyChanged
    {
        private bool _sec;
        private string _durum = "Bekliyor";
        private string _hataMesaji = "";
        private decimal _yeniFiyat;
        private int _onemDerecesi;

        // ─── Excel'den okunan alanlar ─────────────────────────────────────────
        public int SatirNo { get; set; }
        public string MalzemeKodu { get; set; }
        public string MalzemeAciklamasi { get; set; }
        public string FiyatKodu { get; set; }

        public int OnemDerecesi
        {
            get => _onemDerecesi;
            set { _onemDerecesi = value; OnPropertyChanged(nameof(OnemDerecesi)); }
        }

        public decimal YeniFiyat
        {
            get => _yeniFiyat;
            set { _yeniFiyat = value; OnPropertyChanged(nameof(YeniFiyat)); }
        }

        // ─── Logo'dan çekilen alanlar ─────────────────────────────────────────
        public decimal MevcutFiyat { get; set; }
        public int MevcutOncelik { get; set; }
        public bool LogoKontrolEdildi { get; set; }

        // ─── Kilitli mi? ──────────────────────────────────────────────────────
        /// <summary>
        /// Kilitli satırlar seçilip güncellenemez.
        /// </summary>
        public bool IsKilitli =>
            Durum == "Güncellendi" ||
            Durum == "Değişiklik Yok" ||
            Durum == "Hata" ||
            Durum == "Format Hatası" ||
            Durum == "Fiyat Kartı Yok" ||
            Durum == "Okuma Hatası";

        // ─── Durum / UI ───────────────────────────────────────────────────────
        public bool Sec
        {
            get => _sec;
            set
            {
                // Kilitli satıra true set etmeye çalışıyorsa engelle
                if (value && IsKilitli)
                    return;
                if (_sec == value) return;
                _sec = value;
                OnPropertyChanged(nameof(Sec));
            }
        }

        public string Durum
        {
            get => _durum;
            set
            {
                if (_durum == value) return;
                _durum = value;
                OnPropertyChanged(nameof(Durum));
                // Durum kilitli bir değere geçince seçimi otomatik kaldır
                if (IsKilitli && _sec)
                {
                    _sec = false;
                    OnPropertyChanged(nameof(Sec));
                }
            }
        }

        public string HataMesaji
        {
            get => _hataMesaji;
            set { _hataMesaji = value; OnPropertyChanged(nameof(HataMesaji)); }
        }

        // ─── Validasyon ───────────────────────────────────────────────────────
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(MalzemeKodu) &&
            !string.IsNullOrWhiteSpace(FiyatKodu) &&
            YeniFiyat > 0 &&
            Durum != "Format Hatası";

        // ─── INotifyPropertyChanged ───────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}