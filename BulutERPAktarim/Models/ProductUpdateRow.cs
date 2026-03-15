using System.ComponentModel;

namespace BulutERPAktarim.Models
{
    /// <summary>
    /// Excel'den okunan malzeme güncelleme satırı
    /// </summary>
    public class ProductUpdateRow : INotifyPropertyChanged
    {
        private bool _sec;
        private string _durum = "Bekliyor";

        public int SatirNo { get; set; }

        // ─── Excel Alanları ───────────────────────────────────────────────────
        public string MalzemeKodu { get; set; }
        public string Aciklama { get; set; }
        public string Barkod { get; set; }
        public string OzelKod1 { get; set; }
        public string OzelKod2 { get; set; }
        public string OzelKod3 { get; set; }
        public string OzelKod4 { get; set; }
        public string OzelKod5 { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }

        // ─── Logo Kontrol Sonuçları ───────────────────────────────────────────
        public int LogicalRef { get; set; }
        public bool MalzemeVarMi { get; set; }
        public bool LogoKontrolEdildi { get; set; }

        // ─── Kilitli mi? ──────────────────────────────────────────────────────
        /// <summary>
        /// Kilitli satırlar seçilip güncellenemez.
        /// </summary>
        public bool IsKilitli =>
         Durum == "Güncellendi" ||
         Durum == "Hata" ||
         Durum == "Format Hatası" ||
         Durum == "Kontrol Hatası" ||
         Durum == "Malzeme Yok" ||
         Durum == "Barkod Çakışması" ||
         Durum == "Marka Bulunamadı" ||
         Durum == "Model Bulunamadı" ||
         Durum == "Desteklenmiyor";    // ← EKLE

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

        public string HataMesaji { get; set; }

        // ─── Validasyon ───────────────────────────────────────────────────────
        public bool IsValid =>
            MalzemeVarMi &&
            LogoKontrolEdildi &&
            Durum == "Güncellenecek";

        // ─── INotifyPropertyChanged ───────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}