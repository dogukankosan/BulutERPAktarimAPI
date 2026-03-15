using System.ComponentModel;

namespace BulutERPAktarim.Models
{
    /// <summary>
    /// Fiyat ekleme formunda grid'e bağlanan satır modeli
    /// </summary>
    public class FiyatEkleRow : INotifyPropertyChanged
    {
        private bool _sec;
        private string _durum = "Bekliyor";
        private string _hataMesaji = "";
        private decimal _fiyat;
        private int _onemDerecesi;

        // ─── Excel'den okunan alanlar ────────────────────────────────────────
        public int SatirNo { get; set; }
        public string MalzemeKodu { get; set; }
        public string MalzemeAciklamasi { get; set; }
        public string FiyatKodu { get; set; }

        public int OnemDerecesi
        {
            get => _onemDerecesi;
            set { _onemDerecesi = value; OnPropertyChanged(nameof(OnemDerecesi)); OnPropertyChanged(nameof(IsValid)); }
        }

        public decimal Fiyat
        {
            get => _fiyat;
            set { _fiyat = value; OnPropertyChanged(nameof(Fiyat)); OnPropertyChanged(nameof(IsValid)); }
        }

        // ─── Logo'dan çekilen alanlar ────────────────────────────────────────
        public int MalzemeRef { get; set; }
        public bool MalzemeVarMi { get; set; }
        public bool KartZatenVar { get; set; }
        public bool LogoKontrolEdildi { get; set; }

        // ─── Kilitli mi? ─────────────────────────────────────────────────────
        /// <summary>
        /// Kilitli satırlar seçilip gönderilemez.
        /// </summary>
        public bool IsKilitli =>
            string.IsNullOrWhiteSpace(MalzemeKodu) ||
            string.IsNullOrWhiteSpace(FiyatKodu) ||
            Fiyat <= 0 ||
            Durum == "Eklendi" ||
            Durum == "Format Hatası" ||
            Durum == "Malzeme Yok" ||
            Durum == "Kart Zaten Var" ||
            Durum == "Kontrol Hatası" ||
            Durum == "Hata";

        // ─── Durum / UI ──────────────────────────────────────────────────────
        public bool Sec
        {
            get => _sec;
            set
            {
                // Kilitli satıra true set etmeye çalışıyorsa engelle
                if (value && IsKilitli) return;
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
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(IsKilitli));
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

        // ─── Validasyon ──────────────────────────────────────────────────────
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(MalzemeKodu) &&
            !string.IsNullOrWhiteSpace(FiyatKodu) &&
            Fiyat > 0 &&
            Durum != "Format Hatası" &&
            Durum != "Malzeme Yok" &&
            Durum != "Kart Zaten Var" &&
            Durum != "Kontrol Hatası" &&
            Durum != "Hata" &&
            Durum != "Eklendi";

        // ─── INotifyPropertyChanged ──────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}