# BulutERPAktarimAPI

<p align="center">
  <img src="https://i.hizliresim.com/e1qhdq8.png" alt="BulutERPAktarimAPI" width="600"/>
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/dogukankosan/BulutERPAktarimAPI" alt="License"/>
  <img src="https://img.shields.io/github/stars/dogukankosan/BulutERPAktarimAPI" alt="Stars"/>
  <img src="https://img.shields.io/github/issues/dogukankosan/BulutERPAktarimAPI" alt="Issues"/>
  <img src="https://img.shields.io/github/last-commit/dogukankosan/BulutERPAktarimAPI" alt="Last Commit"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-Framework%204.7.2+-purple?logo=dotnet" alt=".NET"/>
  <img src="https://img.shields.io/badge/Logo-Bulut%20ERP-green" alt="Logo ERP"/>
  <img src="https://img.shields.io/badge/DevExpress-WinForms-blue" alt="DevExpress"/>
  <img src="https://img.shields.io/badge/SQLite-Database-orange?logo=sqlite" alt="SQLite"/>
  <img src="https://img.shields.io/badge/Excel-Aktarım-brightgreen?logo=microsoftexcel" alt="Excel"/>
</p>

> **BulutERPAktarimAPI**, Excel dosyasındaki malzeme verilerini Logo Bulut ERP sistemine otomatik olarak aktaran; marka/model yönetimi, fiyat kartı oluşturma/güncelleme, malzeme güncelleme ve üretimden giriş fişi işlemlerini REST API üzerinden gerçekleştiren bir masaüstü C#/.NET uygulamasıdır.

---

## İçindekiler

- [Özellikler](#özellikler)
- [Ekranlar](#ekranlar)
- [Proje Yapısı](#proje-yapısı)
- [Kurulum](#kurulum)
- [Yapılandırma](#yapılandırma)
- [Ana Servisler](#ana-servisler)
- [Malzeme Türleri](#malzeme-türleri)
- [Fiyat Kartları](#fiyat-kartları)
- [Token Yönetimi](#token-yönetimi)
- [Aktarım Akışı](#aktarım-akışı)
- [Excel Şablonları](#excel-şablonları)
- [Veritabanı](#veritabanı)
- [Loglama](#loglama)
- [API Endpoint'leri](#api-endpointleri)
- [SQL Sorguları](#sql-sorguları)
- [Hata Yönetimi](#hata-yönetimi)
- [NuGet Paketleri](#nuget-paketleri)
- [Kullanım İpuçları](#kullanım-ipuçları)
- [Katkı](#katkı)
- [Lisans](#lisans)

---

## Özellikler

| # | Özellik |
|---|---------|
| 🔗 | Logo Bulut ERP ile tam REST API entegrasyonu |
| 📊 | Excel'den toplu malzeme aktarımı (MT/TM türleri) |
| 🔄 | Otomatik token yönetimi ve yenileme (5 dk erken yenileme) |
| 🏷️ | Marka ve Model otomatik oluşturma (upsert mantığı) |
| 💰 | 4 tip fiyat kartı oluşturma (Alış / İnternet / Toptan / Mağaza) |
| 💱 | Toplu fiyat kartı güncelleme (mevcut fiyat karşılaştırmalı) |
| ✏️ | Toplu malzeme güncelleme (açıklama, barkod, özel kodlar, marka/model) |
| 📋 | Üretimden giriş fişi oluşturma ve otomatik onaylama |
| ✅ | Logo'da malzeme ve barkod tekrar kontrolü (duplicate check) |
| 🔍 | Aktarım öncesi kontrol (Mevcut / Yeni / Hata ayrımı) |
| 🔐 | HWID tabanlı lisans doğrulama sistemi |
| 💾 | SQLite ile yerel ayar ve log yönetimi |
| 📝 | Detaylı JSON log sistemi (başarılı ve hatalı işlemler) |
| 🎨 | DevExpress FluentDesign UI & tema sistemi |
| 🖥️ | Gruplu malzeme listesi görünümü |

---

## Ekranlar

Uygulamada bulunan formlar ve işlevleri:

| Form | Açıklama |
|------|----------|
| `HomeForm` | Ana pencere, navigasyon |
| `ProductSendForm` | Excel'den yeni malzeme aktarımı |
| `ProductUpdateForm` | Mevcut malzemeleri toplu güncelleme |
| `PriceAddForm` | Yeni fiyat kartı ekleme |
| `PriceUpdateForm` | Mevcut fiyat kartlarını güncelleme |
| `BulutERPSettingsForm` | ERP bağlantı ayarları |
| `ErrorListForm` | Hata kayıtları ve log ekranı |
| `SQLiteForm` | SQLite ayarları |
| `AboutForm` | Hakkımızda |

---

## Proje Yapısı

```
BulutERPAktarimAPI/
├── Classes/
│   ├── BulutERPService.cs          # Ana ERP servisi, token ve API yönetimi
│   ├── BulutERPConnectionTest.cs   # Bağlantı testi ve ayar okuma
│   ├── LicenseHelper.cs            # HWID tabanlı lisans doğrulama
│   ├── HwidHelper.cs               # Donanım kimliği üretimi
│   ├── EncryptionHelper.cs         # Şifreleme/çözme yardımcısı
│   ├── ThemeManager.cs             # Tema kayıt/yükleme yöneticisi
│   ├── TextLog.cs                  # SQLite loglama sınıfı
│   └── ExcelReader.cs              # Excel dosyası okuma
├── Models/
│   ├── ExcelMalzemeRow.cs          # Excel satırı veri modeli (aktarım)
│   ├── ProductUpdateRow.cs         # Malzeme güncelleme satır modeli
│   ├── FiyatEkleRow.cs             # Fiyat ekleme satır modeli
│   ├── FiyatGuncelleRow.cs         # Fiyat güncelleme satır modeli
│   └── BulutERPSettings.cs         # ERP bağlantı ayarları modeli
├── Forms/
│   ├── HomeForm.cs                 # Ana pencere (FluentDesignForm)
│   ├── ProductSendForm.cs          # Malzeme aktarım ekranı
│   ├── ProductUpdateForm.cs        # Malzeme güncelleme ekranı
│   ├── PriceAddForm.cs             # Fiyat kartı ekleme ekranı
│   ├── PriceUpdateForm.cs          # Fiyat kartı güncelleme ekranı
│   ├── BulutERPSettingsForm.cs     # ERP bağlantı ayarları ekranı
│   ├── ErrorListForm.cs            # Hata kayıtları ekranı
│   ├── SQLiteForm.cs               # SQLite ayarları ekranı
│   └── AboutForm.cs                # Hakkımızda ekranı
├── Database/
│   └── Settings.db                 # SQLite veritabanı
└── JSONLog/                        # API istek/yanıt JSON logları
```

---

## Kurulum

### Gereksinimler

- .NET Framework 4.7.2 veya üzeri
- Visual Studio 2019+
- Logo Bulut ERP hesabı ve API erişimi
- DevExpress WinForms bileşenleri

### Adımlar

**1. Projeyi klonla:**

```bash
git clone https://github.com/dogukankosan/BulutERPAktarimAPI.git
cd BulutERPAktarimAPI
```

**2. Visual Studio ile aç ve derle.**

**3. İlk çalıştırma:**
- Bulut ERP Ayarları ekranından bağlantı bilgilerini girin
- Bağlantıyı test edin
- İlgili ekrandan Excel şablonunu indirin ve doldurun
- Aktarımı başlatın

---

## Yapılandırma

### Bulut ERP Bağlantı Ayarları

```json
{
  "ServerUrl":       "https://your-erp-server.com",
  "MachineID":       "your-machine-id",
  "FirmNr":          "001",
  "Username":        "api-user",
  "Password":        "your-password",
  "AccessToken":     "...",
  "TokenExpireDate": "2025-01-01T12:00:00"
}
```

---

## Ana Servisler

### BulutERPService

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

**Örnek — Malzeme oluşturma:**

```csharp
var existsResult = await BulutERPService.CheckMalzemeExistsAsync("MAL001");

if (!existsResult.Exists)
{
    var createResult = await BulutERPService.CreateMalzemeAsync(excelRow);

    if (createResult.Success)
    {
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

**Örnek — Üretimden giriş fişi:**

```csharp
var fisResult = await BulutERPService.CreateUretimdenGirisFisiAsync(aktifRows);

if (fisResult.Success)
    Console.WriteLine($"Fiş No: {fisResult.FisNo}");
```

---

## Malzeme Türleri

| Tür | cardType | unitSet | UomRef | Açıklama |
|-----|----------|---------|--------|----------|
| **MT** (Malzeme Takımı) | 16 | 06 | 25 | Koli bazlı ürün |
| **TM** (Ticari Mal) | 1 | 05 | 23 | Adet bazlı ürün |

> ⚠️ **Not:** Malzeme Takımı (cardType=16) türündeki kayıtlar Logo API tarafından güncelleme için desteklenmemektedir. Güncelleme ekranında bu kayıtlar otomatik olarak "Desteklenmiyor" durumuna alınır.

---

## Fiyat Kartları

| Tür | Kod Suffix | Endpoint | Öncelik |
|-----|-----------|----------|---------|
| Alış Fiyatı | `-A` | `purchaseprice/createWmm` | 0 |
| İnternet Fiyatı | `-I` | `salesprice/createWmm` | 3 |
| Toptan Fiyatı | `-T` | `salesprice/createWmm` | 2 |
| Mağaza Fiyatı | `-M` | `salesprice/createWmm` | 1 |

- Para birimi: **TRL (160)**
- Tarih aralığı: **Bugün → Yıl sonu**
- Sıfır fiyatlar atlanır; yalnızca `> 0` olan fiyatlar için kart oluşturulur

---

## Token Yönetimi

```
1. API İsteği Tetiklendi
   ↓
2. Token Kontrol
   ├─ Boş / Yok          → Token Al
   ├─ Süresi dolmuş      → Token Al
   ├─ 5 dk'dan az kaldı  → Token Al
   └─ Geçerli            → Kullan
   ↓
3. API İsteği Gönder
   ↓
4. Başarılı mı?
   ├─ Evet → Veriyi Döndür
   └─ Hayır → Hata Logla + JSON Kaydet
```

---

## Aktarım Akışı

### Yeni Malzeme Aktarımı

```
Excel Satırları Yüklendi
   ↓
Her Satır İçin:
   ├─ Malzeme kodu Logo'da var mı?   → Mevcut / Yeni ayrımı
   ├─ Barkod başka malzemede var mı? → Uyarı
   └─ Tür (MT/TM) doğru mu?
   ↓
Kullanıcıya Özet Göster
(Mevcut: X | Yeni: Y | Hata: Z)
   ↓
"Seçilenleri Logo'ya Aktar" butonu
```

### Malzeme Güncelleme

```
Excel Yüklendi
   ↓
Her Satır İçin:
   ├─ Logo'dan LOGICALREF + CARDTYPE çek
   ├─ cardType == 16 → Desteklenmiyor (atla)
   ├─ Barkod varsa → başka malzemede kayıtlı mı?
   ├─ Marka varsa  → Logo'da mevcut mu?
   └─ Model varsa  → Logo'da mevcut mu?
   ↓
GET ile mevcut kart çekilir
Sadece değişen alanlar merge edilir
PUT ile gönderilir
```

### Fiyat Güncelleme

```
Excel Yüklendi
   ↓
Her Satır İçin:
   ├─ GET ile mevcut fiyat okunur
   ├─ Fiyat ve öncelik aynıysa → "Değişiklik Yok"
   └─ Farklıysa → "Güncellenecek"
   ↓
Yalnızca değişen satırlar PUT ile gönderilir
```

---

## Excel Şablonları

Her ekranın kendi şablonu vardır. İlgili ekrandan **"Şablonu İndir"** butonu ile indirilebilir.

### Malzeme Aktarımı Şablonu

| Kolon | Zorunlu | Açıklama |
|-------|---------|----------|
| Malzeme Kodu | ✅ | Benzersiz ürün kodu |
| Malzeme Açıklaması | ✅ | Ürün adı/açıklaması |
| Tür | ✅ | `MT` (Koli) veya `TM` (Adet) |
| Özel Kod 1–5 | ⬜ | Serbest sınıflandırma alanları |
| Marka | ⬜ | Marka kodu (yoksa otomatik oluşturulur) |
| Model | ⬜ | Model kodu (yoksa otomatik oluşturulur) |
| Barkod | ⬜ | Ürün barkodu |
| Alış Fiyatı | ⬜ | TRL alış fiyatı |
| İnternet Fiyatı | ⬜ | TRL internet satış fiyatı |
| Toptan Fiyatı | ⬜ | TRL toptan satış fiyatı |
| Mağaza Fiyatı | ⬜ | TRL mağaza satış fiyatı |
| Miktar | ⬜ | Üretim giriş miktarı |

### Malzeme Güncelleme Şablonu

| Kolon | Zorunlu | Açıklama |
|-------|---------|----------|
| Malzeme Kodu | ✅ | Logo'da kayıtlı kod |
| Açıklama | ✅ | Yeni açıklama |
| Barkod | ⬜ | Yazılırsa güncellenir; başka malzemede varsa hata |
| Özel Kod 1–5 | ⬜ | Boş bırakılırsa mevcut değer korunur |
| Marka | ⬜ | Yazılırsa Logo'da mevcut olmalı |
| Model | ⬜ | Yazılırsa Logo'da mevcut olmalı |

### Fiyat Ekleme / Güncelleme Şablonu

| Kolon | Zorunlu | Açıklama |
|-------|---------|----------|
| Malzeme Kodu | ✅ | Logo'da kayıtlı kod |
| Malzeme Açıklaması | ⬜ | Bilgi amaçlı |
| Fiyat Kart Kodu | ✅ | Örn: `KOL-001-I` |
| Önem Derecesi | ✅ | Sayısal (0–99) |
| Fiyat | ✅ | TRL, 0'dan büyük olmalı |

---

## Veritabanı

### SQLite Tabloları

| Tablo | Açıklama |
|-------|----------|
| `BulutERPSettings` | ERP bağlantı bilgileri ve token |
| `AppConfig` | Şifreli uygulama yapılandırması (lisans API URL vb.) |
| `ThemeSettings` | Kullanıcı tema tercihi |
| `Logs` | İşlem logları ve hatalar |

---

## Loglama

### SQLite Logları

```csharp
await TextLog.LogToSQLiteAsync("✅ Malzeme başarıyla oluşturuldu: MAL001");
await TextLog.LogToSQLiteAsync("❌ Barkod başka malzemede kayıtlı: 8690000000001");
```

### JSON Log Sistemi

Her API isteği ve yanıtı `JSONLog/` klasörüne kaydedilir.

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
    "unitSet": "05"
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

## API Endpoint'leri

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/restservices/rest/dataQuery/executeSelectQuery` | POST | SQL sorgusu çalıştırma |
| `/restservices/rest/v2.0/items?cardType={type}` | POST | Malzeme kartı oluşturma |
| `/restservices/rest/v2.0/items/ref?logicalRef={ref}` | GET | Malzeme kartı okuma |
| `/restservices/rest/v2.0/items/ref?logicalRef={ref}` | PUT | Malzeme kartı güncelleme |
| `/restservices/rest/v2.0/brands` | POST | Marka oluşturma |
| `/restservices/rest/v2.0/brandmodels` | POST | Model oluşturma |
| `/restservices/rest/v2.0/salesprice` | GET | Satış fiyatı okuma |
| `/restservices/rest/salesprice/createWmm` | POST | Satış fiyatı oluşturma |
| `/restservices/rest/v2.0/salesprice` | PUT | Satış fiyatı güncelleme |
| `/restservices/rest/purchaseprice/createWmm` | POST | Alış fiyatı oluşturma |
| `/restservices/rest/v2.0/salesprice/ref?logicalRef={ref}` | POST | Fiyat kartı ekleme (ref ile) |
| `/restservices/rest/v2.0/itemslips/inputfromProdSlip` | POST | Üretimden giriş fişi |
| `/restservices/rest/v2.0/itemslips/status/controlflag/ref` | PUT | Fiş onaylama |

---

## SQL Sorguları

> **Not:** `$V(firm)` parametresi çalışma zamanında firma numarasıyla otomatik değiştirilir.

### Malzeme Kontrol

```sql
SELECT LOGICALREF, CARDTYPE
FROM U_$V(firm)_ITEMS
WHERE BOSTATUS <> 1
  AND CODE = 'MAL001'
```

### Barkod Kontrol (Çakışma)

```sql
SELECT UB.BARCODE, I.CODE
FROM U_$V(firm)_UNITBARCODE UB
INNER JOIN U_$V(firm)_ITEMS I ON I.LOGICALREF = UB.ITEMREF
WHERE UB.BARCODE = '8690000000001'
  AND I.BOSTATUS <> 1
  AND I.LOGICALREF <> {mevcutLogicalRef}
```

### Fiyat Kartı Kontrol

```sql
SELECT LOGICALREF
FROM U_$V(firm)_PRICES
WHERE CODE = 'KOL-001-I'
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

---

## Hata Yönetimi

```csharp
// Barkod kontrolü ile güvenli aktarım
var barkodResult = await BulutERPService.CheckBarkodExistsAsync(row.Barkod);

if (barkodResult.Exists)
{
    await TextLog.LogToSQLiteAsync(
        $"⚠ Barkod zaten mevcut: {row.Barkod} → {barkodResult.ExistingCode}"
    );
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

## NuGet Paketleri

```xml
<PackageReference Include="Newtonsoft.Json"    Version="13.0.3"   />
<PackageReference Include="System.Data.SQLite" Version="1.0.118"  />
<PackageReference Include="DevExpress.WinForms" Version="24.x"   />
<PackageReference Include="ClosedXML"          Version="0.102.x"  />
<PackageReference Include="EPPlus"             Version="7.x"      />
```

---

## Kullanım İpuçları

1. **Token Süresi:** Token'lar otomatik yönetilir, 5 dakika önceden yenilenir.
2. **Excel Şablonu:** Her ekrandaki "Şablonu İndir" butonu ile doğru formatı indirin.
3. **MT/TM Türü:** Koli ürünler `MT`, adet ürünler `TM` olarak işaretleyin. MT malzemeler güncellenemez.
4. **Toplu Aktarım:** "Tümünü Seç" ile tüm geçerli satırları tek seferde işleyebilirsiniz.
5. **Grup Görünümü:** Malzemeler kod prefix'ine göre otomatik gruplanır.
6. **JSON Logları:** Hata durumunda `JSONLog/` klasörünü inceleyin.
7. **Barkod:** Aynı barkod birden fazla malzemede kullanılamaz; kontrol otomatik yapılır.
8. **Fiyat:** Sıfır fiyatlar atlanır; yalnızca `> 0` değerler için kart oluşturulur.
9. **Malzeme Güncelleme:** Boş bırakılan opsiyonel alanlar (marka, model, özel kodlar) mevcut değeri ezmez.
10. **Fiyat Güncelleme:** Değişmeyen satırlar "Değişiklik Yok" olarak işaretlenir ve API'ye gönderilmez.

---

## Katkı

Katkı sağlamak için projeyi fork'layabilir ve pull request gönderebilirsiniz.

```bash
# 1. Fork edin
# 2. Feature branch oluşturun
git checkout -b feature/YeniOzellik

# 3. Commit yapın
git commit -m 'Yeni özellik eklendi'

# 4. Branch'e push edin
git push origin feature/YeniOzellik

# 5. Pull Request açın
```

---

## Güncellemeler

### v1.1 (2025)

- **Yeni:** Toplu malzeme güncelleme ekranı (`ProductUpdateForm`)
- **Yeni:** Fiyat kartı ekleme ekranı (`PriceAddForm`)
- **Yeni:** Fiyat kartı güncelleme ekranı (`PriceUpdateForm`) — değişiklik karşılaştırmalı
- **Yeni:** HWID tabanlı lisans doğrulama sistemi
- **İyileştirme:** Malzeme Takımı (MT) güncelleme desteği dışlandı, kullanıcıya bildirim eklendi
- **İyileştirme:** Barkod çakışma kontrolü kendi logicalRef'i hariç tutacak şekilde güncellendi

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

## Lisans

Bu proje [MIT License](LICENSE) ile lisanslanmıştır.

---

## İletişim

- 👨‍💻 Geliştirici: [@dogukankosan](https://github.com/dogukankosan)
- 🐞 Öneri veya sorunlar için: [Issues](https://github.com/dogukankosan/BulutERPAktarimAPI/issues)
