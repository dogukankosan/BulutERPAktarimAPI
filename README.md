# 📦 BulutERPAktarimAPI



![License](https://img.shields.io/github/license/dogukankosan/BulutERPAktarimAPI)
![Stars](https://img.shields.io/github/stars/dogukankosan/BulutERPAktarimAPI)
![Issues](https://img.shields.io/github/issues/dogukankosan/BulutERPAktarimAPI)
![Last Commit](https://img.shields.io/github/last-commit/dogukankosan/BulutERPAktarimAPI)

> **BulutERPAktarimAPI**, Excel dosyasındaki malzeme verilerini Logo Bulut ERP sistemine otomatik olarak aktaran; marka/model yönetimi, fiyat kartı oluşturma ve üretimden giriş fişi işlemlerini REST API üzerinden gerçekleştiren bir masaüstü C#/.NET uygulamasıdır.

---

## 🚀 Özellikler

- 🔗 Logo Bulut ERP ile tam REST API entegrasyonu
- 📊 Excel'den toplu malzeme aktarımı (MT/TM türleri)
- 🔄 Otomatik token yönetimi ve yenileme (5 dk erken yenileme)
- 🏷️ Marka ve Model otomatik oluşturma (upsert mantığı)
- 💰 4 tip fiyat kartı oluşturma (Alış / İnternet / Toptan / Mağaza)
- 📋 Üretimden giriş fişi oluşturma ve otomatik onaylama
- ✅ Logo'da malzeme ve barkod tekrar kontrolü (duplicate check)
- 🔍 Aktarım öncesi kontrol (Mevcut / Yeni ayrımı)
- 💾 SQLite ile yerel ayar ve log yönetimi
- 📝 Detaylı JSON log sistemi (başarılı ve hatalı işlemler)
- 🎨 DevExpress FluentDesign UI & tema sistemi
- 🖥️ Gruplu malzeme listesi görünümü

---

## 🗂 Proje Yapısı

```
BulutERPAktarimAPI/
├── Classes/
│   ├── BulutERPService.cs          # Ana ERP servisi, token ve API yönetimi
│   ├── BulutERPConnectionTest.cs   # Bağlantı testi ve ayar okuma
│   ├── ThemeManager.cs             # Tema kayıt/yükleme yöneticisi
│   ├── TextLog.cs                  # SQLite loglama sınıfı
│   └── ExcelReader.cs              # Excel dosyası okuma
├── Models/
│   ├── ExcelMalzemeRow.cs          # Excel satırı veri modeli
│   └── BulutERPSettings.cs         # ERP bağlantı ayarları modeli
├── Forms/
│   ├── HomeForm.cs                 # Ana pencere (FluentDesignForm)
│   ├── ProductSendForm.cs          # Malzeme aktarım ekranı
│   ├── BulutERPSettingsForm.cs     # ERP bağlantı ayarları ekranı
│   ├── ErrorListForm.cs            # Hata kayıtları ekranı
│   ├── SQLiteForm.cs               # SQLite ayarları ekranı
│   └── AboutForm.cs                # Hakkımızda ekranı
├── Database/
│   └── Settings.db                 # SQLite veritabanı
└── JSONLog/                        # API istek/yanıt JSON logları
```

---

## 🛠️ Kurulum & Çalıştırma

### Gereksinimler

- .NET Framework 4.7.2 veya üzeri
- Visual Studio 2019+
- Logo Bulut ERP hesabı ve API erişimi
- DevExpress WinForms bileşenleri

### Kurulum

1. **Projeyi Klonla:**
```bash
git clone https://github.com/dogukankosan/BulutERPAktarimAPI.git
cd BulutERPAktarimAPI
```

2. **Visual Studio ile Aç ve Derle**

3. **İlk Çalıştırma:**
   - Bulut ERP Ayarları ekranından bağlantı bilgilerini girin
   - Bağlantıyı test edin
   - Excel şablonunu indirip doldurun
   - Aktarımı başlatın

---

## 🔧 Yapılandırma

### Bulut ERP Ayarları

```csharp
// BulutERPSettings modeli
{
    "ServerUrl":      "https://your-erp-server.com",
    "MachineID":      "your-machine-id",
    "FirmNr":         "001",
    "Username":       "api-user",
    "Password":       "your-password",
    "AccessToken":    "...",
    "TokenExpireDate":"2025-01-01T12:00:00"
}
```

---

## 📡 Ana Servisler

### 1. BulutERPService

| Metod | Açıklama |
|-------|----------|
| `EnsureValidTokenAsync()` | Token kontrolü ve otomatik yenileme |
| `ExecuteSelectQueryAsync()` | ERP üzerinde SQL sorgusu çalıştırma |
| `EnsureBrandAsync()` | Marka kontrolü, yoksa oluşturma |
| `EnsureModelAsync()` | Model kontrolü, yoksa oluşturma |
| `CreateMalzemeAsync()` | Excel satırından malzeme kartı oluşturma |
| `CreateFiyatKartlariAsync()` | 4 tip fiyat kartı oluşturma |
| `CheckMalzemeExistsAsync()` | Malzeme Logo'da var mı kontrolü |
| `CheckBarkodExistsAsync()` | Barkod başka malzemede kayıtlı mı kontrolü |
| `CreateUretimdenGirisFisiAsync()` | Çoklu satırdan üretim fişi oluşturma |

**Örnek Kullanım:**

```csharp
// Malzeme varlık kontrolü
var existsResult = await BulutERPService.CheckMalzemeExistsAsync("MAL001");

if (!existsResult.Exists)
{
    // Malzeme oluştur
    var createResult = await BulutERPService.CreateMalzemeAsync(excelRow);

    if (createResult.Success)
    {
        // Fiyat kartlarını oluştur
        await BulutERPService.CreateFiyatKartlariAsync(
            malzemeKodu:    createResult.MalzemeKodu,
            alisFiyati:     100.00m,
            internetFiyati: 150.00m,
            toptanFiyati:   130.00m,
            magazaFiyati:   160.00m
        );
    }
}
```

```csharp
// Üretimden giriş fişi oluşturma (çoklu satır)
var fisResult = await BulutERPService.CreateUretimdenGirisFisiAsync(aktifRows);

if (fisResult.Success)
    Console.WriteLine($"Fiş No: {fisResult.FisNo}");
```

---

## 🏷️ Malzeme Türleri

| Tür | cardType | unitSet | UomRef | Açıklama |
|-----|----------|---------|--------|----------|
| **MT** (Malzeme Takımı) | 16 | 06 | 25 | Koli bazlı ürün |
| **TM** (Ticari Mal) | 1 | 05 | 23 | Adet bazlı ürün |

---

## 💰 Fiyat Kartları

| Tür | Kod Suffix | Endpoint | Öncelik |
|-----|-----------|----------|---------|
| Alış Fiyatı | `-A` | `purchaseprice/createWmm` | 0 |
| İnternet Fiyatı | `-I` | `salesprice/createWmm` | 3 |
| Toptan Fiyatı | `-T` | `salesprice/createWmm` | 2 |
| Mağaza Fiyatı | `-M` | `salesprice/createWmm` | 1 |

- Para birimi: **TRL (160)**
- Tarih aralığı: **Bugün → Yıl sonu**

---

## 🔄 Token Yönetimi Akışı

```
1. API İsteği Tetiklendi
   ↓
2. Token Kontrol
   ├─ Boş/Yok        → Token Al
   ├─ Süresi dolmuş  → Token Al
   ├─ 5dk'dan az kaldı → Token Al
   └─ Geçerli        → Kullan
   ↓
3. API İsteği Gönder
   ↓
4. Başarılı mı?
   ├─ Evet → Veriyi Döndür
   └─ Hayır → Hata Logla + JSON Kaydet
```

---

## ✅ Aktarım Öncesi Kontrol Akışı

```
Excel Satırları Yüklendi
   ↓
Her Satır İçin:
   ├─ Malzeme kodu Logo'da var mı?  → Mevcut / Yeni ayrımı
   ├─ Barkod başka malzemede var mı? → Uyarı
   └─ Tür (MT/TM) doğru mu?
   ↓
Kullanıcıya Özet Göster
(Mevcut: X | Yeni: Y | Hata: Z)
   ↓
"Seçilenleri Logo'ya Aktar" butonu
```

---

## 📊 Excel Şablonu Kolonları

| Kolon | Açıklama |
|-------|----------|
| Malzeme Kodu | Benzersiz ürün kodu |
| Malzeme Açıklaması | Ürün adı/açıklaması |
| Tür | MT (Koli) veya TM (Adet) |
| Özel Kod 1-5 | Serbest sınıflandırma alanları |
| Marka | Marka kodu (otomatik oluşturulur) |
| Model | Model kodu (otomatik oluşturulur) |
| Barkod | Ürün barkodu |
| Alış Fiyatı | TRL alış fiyatı |
| İnternet Fiyatı | TRL internet satış fiyatı |
| Toptan Fiyatı | TRL toptan satış fiyatı |
| Mağaza Fiyatı | TRL mağaza satış fiyatı |
| Miktar | Üretim giriş miktarı |

---

## 🗄 Veritabanı Yapısı

### SQLite Tabloları

| Tablo | Açıklama |
|-------|----------|
| `BulutERPSettings` | ERP bağlantı bilgileri ve token |
| `ThemeSettings` | Kullanıcı tema tercihi |
| `Logs` | İşlem logları ve hatalar |

---

## 📝 Loglama Sistemi

### SQLite Logları

```csharp
await TextLog.LogToSQLiteAsync("✅ Malzeme başarıyla oluşturuldu: MAL001");
await TextLog.LogToSQLiteAsync("❌ Barkod başka malzemede kayıtlı: 8690000000001");
```

### JSON Log Sistemi

Her API isteği ve yanıtı `JSONLog/` klasörüne kaydedilir:

**Başarılı:**
```
JSONLog/
└── 20250311_143052_MALZEME_MAL001_CT1.json
```

```json
{
  "istek": {
    "code": "MAL001",
    "description": "Örnek Malzeme",
    "cardType": 1,
    "unitSet": "05",
    ...
  },
  "yanit": {
    "logicalRef": 12345,
    "successful": true
  }
}
```

**Hatalı:**
```
JSONLog/
└── 20250311_143052_MALZEME_MAL001_CT1_HATA.json
```

---

## 🚦 API Endpoint'leri

### Logo Bulut ERP

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/restservices/rest/dataQuery/executeSelectQuery` | POST | SQL sorgusu çalıştırma |
| `/restservices/rest/v2.0/items?cardType={type}` | POST | Malzeme kartı oluşturma |
| `/restservices/rest/v2.0/brands` | POST | Marka oluşturma |
| `/restservices/rest/v2.0/brandmodels` | POST | Model oluşturma |
| `/restservices/rest/salesprice/createWmm` | POST | Satış fiyatı oluşturma |
| `/restservices/rest/purchaseprice/createWmm` | POST | Alış fiyatı oluşturma |
| `/restservices/rest/v2.0/itemslips/inputfromProdSlip` | POST | Üretimden giriş fişi |
| `/restservices/rest/v2.0/itemslips/status/controlflag/ref` | PUT | Fiş onaylama |

---

## ⚙️ Kullanılan SQL Sorguları

### Malzeme Kontrol

```sql
SELECT LOGICALREF 
FROM U_$V(firm)_ITEMS 
WHERE BOSTATUS<>1 
  AND CODE = 'MAL001'
```

### Barkod Kontrol

```sql
SELECT UB.BARCODE, I.CODE 
FROM U_$V(firm)_UNITBARCODE UB
INNER JOIN U_$V(firm)_ITEMS I ON I.LOGICALREF = UB.ITEMREF
WHERE UB.BARCODE = '8690000000001'
  AND I.BOSTATUS <> 1
```

### Marka Kontrol

```sql
SELECT LOGICALREF 
FROM U_$V(firm)_BRANDS 
WHERE CODE = 'DEEPCOOL'
```

### Model Kontrol

```sql
SELECT LOGICALREF 
FROM U_$V(firm)_BRANDMODELS 
WHERE CODE = 'MODEL001'
  AND BRANDREF = (
      SELECT LOGICALREF FROM U_$V(firm)_BRANDS WHERE CODE = 'DEEPCOOL'
  )
```

> **Not:** `$V(firm)` parametresi otomatik olarak firma numarası ile değiştirilir.

---

## 🛡️ Hata Yönetimi

```csharp
// Barkod kontrolü ile güvenli aktarım
var barkodResult = await BulutERPService.CheckBarkodExistsAsync(row.Barkod);

if (barkodResult.Exists)
{
    await TextLog.LogToSQLiteAsync(
        $"⚠ Barkod zaten mevcut: {row.Barkod} → {barkodResult.ExistingCode}"
    );
    // Kullanıcıya uyarı göster, aktarımı atla
    continue;
}

// Marka/Model upsert
var brandResult = await BulutERPService.EnsureBrandAsync(row.Marka, row.Marka, token);

if (!brandResult.Success)
{
    await TextLog.LogToSQLiteAsync($"❌ Marka oluşturulamadı: {brandResult.ErrorMessage}");
    return;
}
```

---

## 📦 NuGet Paketleri

```xml
<PackageReference Include="Newtonsoft.Json"         Version="13.0.3" />
<PackageReference Include="System.Data.SQLite"      Version="1.0.118" />
<PackageReference Include="DevExpress.WinForms"     Version="24.x" />
<PackageReference Include="ClosedXML"               Version="0.102.x" />
```

---

## 🎯 Kullanım İpuçları

1. **Token Süresi:** Token'lar otomatik yönetilir, 5 dakika önceden yenilenir
2. **Excel Şablonu:** "Şablonu İndir" butonu ile doğru formatı indirin
3. **MT/TM Türü:** Koli ürünler MT, adet ürünler TM olarak işaretleyin
4. **Toplu Aktarım:** "Tümünü Seç" ile tüm satırları tek seferde aktarabilirsiniz
5. **Grup Görünümü:** Malzemeler kod prefix'ine göre otomatik gruplanır
6. **JSON Logları:** Hata durumunda `JSONLog/` klasörünü inceleyin
7. **Barkod:** Aynı barkod birden fazla malzemede kullanılamaz, kontrol otomatik yapılır
8. **Fiyat:** Sıfır fiyatlar atlanır, sadece 0'dan büyük fiyatlar için kart oluşturulur

---

## 🤝 Katkı

Katkı sağlamak için projeyi forklayabilir ve pull request gönderebilirsiniz.

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Commit yapın (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'e push edin (`git push origin feature/YeniOzellik`)
5. Pull Request açın

---

## 📄 Lisans

MIT License

---

## 📬 İletişim

- 👨‍💻 Geliştirici: [@dogukankosan](https://github.com/dogukankosan)
- 🐞 Öneri veya sorunlar: [Issues sekmesi](https://github.com/dogukankosan/BulutERPAktarimAPI/issues)

---

## 🎉 Güncellemeler

### v1.0 (Mart 2025)
- **Yeni:** Excel'den toplu malzeme aktarımı
- **Yeni:** MT/TM türü otomatik ayrıştırma
- **Yeni:** Marka ve Model upsert yönetimi
- **Yeni:** 4 tip fiyat kartı otomatik oluşturma
- **Yeni:** Üretimden giriş fişi + otomatik onay
- **Yeni:** Malzeme ve barkod duplicate kontrolü
- **Yeni:** JSON log sistemi (başarılı/hatalı)
- **Yeni:** DevExpress FluentDesign arayüzü
- **Yeni:** SQLite tabanlı ayar ve log yönetimi
- **Yeni:** Tema kayıt/yükleme sistemi

---

<p align="center">
  <img src="https://img.shields.io/badge/.NET-Framework%204.7.2+-purple?logo=dotnet" alt="dotnet" />
  <img src="https://img.shields.io/badge/Logo-Bulut%20ERP-green" alt="logo erp" />
  <img src="https://img.shields.io/badge/DevExpress-WinForms-blue" alt="devexpress" />
  <img src="https://img.shields.io/badge/SQLite-Database-orange?logo=sqlite" alt="sqlite" />
  <img src="https://img.shields.io/badge/Excel-Aktarım-brightgreen?logo=microsoftexcel" alt="excel" />
</p>
