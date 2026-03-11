using BulutERPAktarim.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BulutERPAktarim.Classes
{
    /// <summary>
    /// Bulut ERP SQL sorguları ve token yönetimi için servis sınıfı
    /// </summary>
    internal class BulutERPService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Random _random = new Random();

        #region Token Management

        /// <summary>
        /// Geçerli token getirir, dolmuşsa veya 5 dakikadan az kaldıysa yeniler
        /// </summary>
        public static async Task<(bool Success, string AccessToken, string ErrorMessage)> EnsureValidTokenAsync()
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(settings.AccessToken) ||
                    string.IsNullOrWhiteSpace(settings.TokenExpireDate))
                    return await BulutERPConnectionTest.GetTokenAsync();
                DateTime expireDate = DateTime.Parse(settings.TokenExpireDate);
                TimeSpan remaining = expireDate - DateTime.Now;
                if (remaining.TotalMinutes <= 5)
                    return await BulutERPConnectionTest.GetTokenAsync();
                return (true, settings.AccessToken, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ EnsureValidTokenAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region API Operations
        /// <summary>
        /// SQL sorgusu çalıştırır - Token otomatik kontrol eder ve yeniler
        /// </summary>
        public static async Task<(bool Success, List<Dictionary<string, object>> Data, string ErrorMessage)> ExecuteSelectQueryAsync(
            string sqlQuery,
            string accessToken = null,
            int maxCount = 10000)
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                var requestBody = new
                {
                    querySqlText = sqlQuery,
                    dataQueryParams = $"{{\"firm\":\"{settings.FirmNr.Trim()}\"}}",
                    jsonFormat = 1,
                    maxCount = maxCount
                };
                string jsonContent = JsonConvert.SerializeObject(requestBody);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/dataQuery/executeSelectQuery";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP API hatası - Status: {response.StatusCode}, Response: {jsonResponse}");
                    return (false, null, $"SQL sorgusu hatası: {response.StatusCode} - {response.ReasonPhrase}");
                }
                JObject json = JObject.Parse(jsonResponse);
                bool successful = json["successful"]?.Value<bool>() ?? false;
                if (!successful)
                {
                    string errorMsg = json["errorMessage"]?.ToString() ?? "Bilinmeyen hata";
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP executeSelectQuery başarısız: {errorMsg}");
                    return (false, null, errorMsg);
                }
                JArray rowsArray = json["rows"] as JArray;
                if (rowsArray == null)
                    return (true, new List<Dictionary<string, object>>(), null);
                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
                foreach (JToken item in rowsArray)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    foreach (JProperty prop in ((JObject)item).Properties())
                        dict[prop.Name] = prop.Value.ToObject<object>();
                    resultList.Add(dict);
                }
                return (true, resultList, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ExecuteSelectQueryAsync exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region Marka / Model

        /// <summary>
        /// Marka kodunu kontrol eder, yoksa oluşturur. LogicalRef döner.
        /// </summary>
        public static async Task<(bool Success, int LogicalRef, string ErrorMessage)> EnsureBrandAsync(
            string brandCode,
            string brandDescription,
            string accessToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(brandCode))
                    return (false, 0, "Marka kodu boş olamaz");
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, 0, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                string sql = $"SELECT LOGICALREF FROM U_$V(firm)_BRANDS WHERE CODE='{brandCode.Replace("'", "''")}'";
                var checkResult = await ExecuteSelectQueryAsync(sql, accessToken, 1);
                if (!checkResult.Success)
                    return (false, 0, checkResult.ErrorMessage);
                if (checkResult.Data != null && checkResult.Data.Count > 0)
                {
                    int existingRef = Convert.ToInt32(checkResult.Data[0]["LOGICALREF"]);
                    return (true, existingRef, null);
                }
                var body = new
                {
                    code = brandCode.Trim(),
                    description = string.IsNullOrWhiteSpace(brandDescription) ? brandCode.Trim() : brandDescription.Trim()
                };
                string jsonContent = JsonConvert.SerializeObject(body);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("lang", "TRTR");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/brands";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ Marka oluşturma hatası\n" +
                        $"   Marka: {brandCode}\n" +
                        $"   HTTP : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt: {responseContent}");
                    JsonLog($"MARKA_{brandCode}_HATA", body, responseContent);
                    return (false, 0, $"Marka oluşturulamadı: {responseContent}");
                }
                JObject responseJson = JObject.Parse(responseContent);
                int logicalRef = responseJson["logicalRef"]?.Value<int>() ?? 0;
                JsonLog($"MARKA_{brandCode}", body, responseJson);
                return (true, logicalRef, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ EnsureBrandAsync hatası: {ex}");
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Model kodunu kontrol eder, yoksa oluşturur. LogicalRef döner.
        /// </summary>
        public static async Task<(bool Success, int LogicalRef, string ErrorMessage)> EnsureModelAsync(
            string modelCode,
            string modelDescription,
            string brandCode,
            string accessToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelCode))
                    return (false, 0, "Model kodu boş olamaz");
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, 0, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                string brandFilter = string.IsNullOrWhiteSpace(brandCode)
                    ? ""
                    : $" AND BRANDREF=(SELECT LOGICALREF FROM U_$V(firm)_BRANDS WHERE CODE='{brandCode.Replace("'", "''")}')";
                string sql = $"SELECT LOGICALREF FROM U_$V(firm)_BRANDMODELS WHERE CODE='{modelCode.Replace("'", "''")}'{brandFilter}";
                var checkResult = await ExecuteSelectQueryAsync(sql, accessToken, 1);
                if (!checkResult.Success)
                    return (false, 0, checkResult.ErrorMessage);
                if (checkResult.Data != null && checkResult.Data.Count > 0)
                {
                    int existingRef = Convert.ToInt32(checkResult.Data[0]["LOGICALREF"]);
                    return (true, existingRef, null);
                }
                var body = new
                {
                    code = modelCode.Trim(),
                    description = string.IsNullOrWhiteSpace(modelDescription) ? modelCode.Trim() : modelDescription.Trim(),
                    brandCode = brandCode?.Trim() ?? ""
                };
                string jsonContent = JsonConvert.SerializeObject(body);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("lang", "TRTR");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/brandmodels";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ Model oluşturma hatası\n" +
                        $"   Model: {modelCode}\n" +
                        $"   Marka: {brandCode}\n" +
                        $"   HTTP : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt: {responseContent}");
                    JsonLog($"MODEL_{modelCode}_MARKA_{brandCode}_HATA", body, responseContent);
                    return (false, 0, $"Model oluşturulamadı: {responseContent}");
                }
                JObject responseJson = JObject.Parse(responseContent);
                int logicalRef = responseJson["logicalRef"]?.Value<int>() ?? 0;
                JsonLog($"MODEL_{modelCode}_MARKA_{brandCode}", body, responseJson);
                return (true, logicalRef, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ EnsureModelAsync hatası: {ex}");
                return (false, 0, ex.Message);
            }
        }
        #endregion

        #region Malzeme Oluşturma
        /// <summary>
        /// Excel satırından malzeme oluşturur.
        /// MT → cardType=16 (koli)
        /// TM → cardType=1 (ticari mal/adet)
        /// </summary>
        public static async Task<(bool Success, string MalzemeKodu, string ErrorMessage)> CreateMalzemeAsync(
            ExcelMalzemeRow row,
            string accessToken = null)
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                // 1. Marka/Model kontrolü ve oluşturma
                if (!string.IsNullOrWhiteSpace(row.Marka))
                {
                    var brandResult = await EnsureBrandAsync(row.Marka, row.Marka, accessToken);
                    if (!brandResult.Success)
                    {
                        await TextLog.LogToSQLiteAsync($"⚠ Marka oluşturulamadı: {row.Marka} - {brandResult.ErrorMessage}");
                        return (false, null, $"Marka oluşturulamadı ({row.Marka}): {brandResult.ErrorMessage}");
                    }
                    if (!string.IsNullOrWhiteSpace(row.Model))
                    {
                        var modelResult = await EnsureModelAsync(row.Model, row.Model, row.Marka, accessToken);
                        if (!modelResult.Success)
                        {
                            await TextLog.LogToSQLiteAsync($"⚠ Model oluşturulamadı: {row.Model} - {modelResult.ErrorMessage}");
                            return (false, null, $"Model oluşturulamadı ({row.Model}): {modelResult.ErrorMessage}");
                        }
                    }
                }
                bool isTakım = row.Tur?.Trim().ToUpper() == "MT";
                if (isTakım)
                {
                    var mtResult = await CreateSingleMalzemeAsync(
                        settings, accessToken,
                        cardType: 16,
                        unitSet: "06",
                        uomRef: 25,
                        row: row,
                        assortItemRef: 0
                    );
                    if (!mtResult.Success)
                        return (false, null, $"MT malzeme oluşturulamadı: {mtResult.ErrorMessage}");
                    return (true, row.MalzemeKodu, null);
                }
                else
                {
                    var tmResult = await CreateSingleMalzemeAsync(
                        settings, accessToken,
                        cardType: 1,
                        unitSet: "05",
                        uomRef: 23,
                        row: row,
                        assortItemRef: 0
                    );
                    if (!tmResult.Success)
                        return (false, null, $"Malzeme oluşturulamadı: {tmResult.ErrorMessage}");
                    return (true, row.MalzemeKodu, null);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateMalzemeAsync hatası: {ex}");
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Tek bir malzeme kartı oluşturur (iç kullanım)
        /// </summary>
        private static async Task<(bool Success, int LogicalRef, string ErrorMessage)> CreateSingleMalzemeAsync(
            BulutERPSettings settings,
            string accessToken,
            int cardType,
            string unitSet,
            int uomRef,
            ExcelMalzemeRow row,
            int assortItemRef = 0,
            string kodOverride = null,
            string aciklamaOverride = null)
        {
            try
            {
                string malzemeKodu = kodOverride ?? row.MalzemeKodu?.Trim();
                string aciklama = aciklamaOverride ?? row.MalzemeAciklamasi?.Trim();
                List<object> barcodeList = new List<object>();
                if (!string.IsNullOrWhiteSpace(row.Barkod))
                {
                    barcodeList.Add(new
                    {
                        number = 1,
                        barcode = row.Barkod.Trim(),
                        serializeNulls = false
                    });
                }
                var unitAssignment = new
                {
                    divisible = false,
                    barcodes = barcodeList,
                    modules = 7,
                    multiplier = 1.0,
                    divisor = 1.0,
                    serializeNulls = false
                };
                var body = new
                {
                    code = malzemeKodu,
                    description = aciklama,
                    cardType = cardType,
                    auxCode = row.OzelKod1?.Trim() ?? "",
                    auxiliaryCode2 = row.OzelKod2?.Trim() ?? "",
                    auxiliaryCode3 = row.OzelKod3?.Trim() ?? "",
                    auxiliaryCode4 = row.OzelKod4?.Trim() ?? "",
                    auxiliaryCode5 = row.OzelKod5?.Trim() ?? "",
                    brandCode = row.Marka?.Trim() ?? "",
                    modelCode = row.Model?.Trim() ?? "",
                    purchase = 10,
                    sales = 20,
                    retailSales = 20,
                    returns = 20,
                    retailSalesReturn = 20,
                    materialManagement = true,
                    purchasingManagement = true,
                    salesManagement = true,
                    corporateEmployeePortal = true,
                    ebusinessEnvironment = true,
                    demandManagement = true,
                    divisibleLotSize = true,
                    unitSet = unitSet,
                    depreciationType = 1,
                    depreciationType2 = 1,
                    revaluation = 1,
                    revaluationDepreciation = 1,
                    assetCategory = -1,
                    assortItemRef = assortItemRef,
                    unitAssignments = new[] { unitAssignment }
                };
                string jsonContent = JsonConvert.SerializeObject(body);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("lang", "TRTR");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/items?cardType={cardType}";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    string hataDetay = $"❌ Malzeme oluşturma hatası\n" +
                        $"   Kod     : {malzemeKodu}\n" +
                        $"   CardType: {cardType}\n" +
                        $"   UnitSet : {unitSet}\n" +
                        $"   HTTP    : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt   : {responseContent}";
                    await TextLog.LogToSQLiteAsync(hataDetay);
                    JsonLog($"MALZEME_{malzemeKodu}_CT{cardType}_HATA", body, responseContent);
                    return (false, 0, $"[{(int)response.StatusCode}] {responseContent}");
                }
                JObject responseJson = JObject.Parse(responseContent);
                int logicalRef = responseJson["logicalRef"]?.Value<int>() ?? 0;
                JsonLog($"MALZEME_{malzemeKodu}_CT{cardType}", body, responseJson);
                return (true, logicalRef, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateSingleMalzemeAsync hatası: {ex}");
                return (false, 0, ex.Message);
            }
        }
        #endregion

        #region Fiyat Kartları
        /// <summary>
        /// Malzeme için tüm fiyat kartlarını oluşturur (Alış + İnternet + Toptan + Mağaza)
        /// Tarihler: bugün → yıl sonu
        /// Para birimi: TRL (160)
        /// </summary>
        public static async Task<(bool Success, string ErrorMessage)> CreateFiyatKartlariAsync(
            string malzemeKodu,
            decimal alisFiyati,
            decimal internetFiyati,
            decimal toptanFiyati,
            decimal magazaFiyati,
            string accessToken = null,
            int uomRef = 23)
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                DateTime bugun = DateTime.Today;
                DateTime yilSonu = new DateTime(bugun.Year, 12, 31);
                long startTimestamp = new DateTimeOffset(bugun).ToUnixTimeSeconds();
                long endTimestamp = new DateTimeOffset(yilSonu).ToUnixTimeSeconds();
                bool herhangiHata = false;
                if (alisFiyati > 0)
                {
                    var alisResult = await CreateFiyatKartiAsync(
                        settings, accessToken,
                        isSales: false,
                        mmCode: malzemeKodu,
                        priceCode: $"{malzemeKodu}-A",
                        priceDescription: $"{malzemeKodu} Alış Fiyatı",
                        unitPrice: alisFiyati,
                        startTimestamp: startTimestamp,
                        endTimestamp: endTimestamp,
                        uomRef: uomRef
                    );
                    if (!alisResult.Success) herhangiHata = true;
                }
                if (internetFiyati > 0)
                {
                    var iResult = await CreateFiyatKartiAsync(
                        settings, accessToken,
                        isSales: true,
                        mmCode: malzemeKodu,
                        priceCode: $"{malzemeKodu}-I",
                        priceDescription: $"{malzemeKodu} İnternet Fiyatı",
                        unitPrice: internetFiyati,
                        startTimestamp: startTimestamp,
                        endTimestamp: endTimestamp,
                        uomRef: uomRef, priority: 3
                    );
                    if (!iResult.Success) herhangiHata = true;
                }
                if (toptanFiyati > 0)
                {
                    var tResult = await CreateFiyatKartiAsync(
                        settings, accessToken,
                        isSales: true,
                        mmCode: malzemeKodu,
                        priceCode: $"{malzemeKodu}-T",
                        priceDescription: $"{malzemeKodu} Toptan Fiyatı",
                        unitPrice: toptanFiyati,
                        startTimestamp: startTimestamp,
                        endTimestamp: endTimestamp,
                        uomRef: uomRef, priority: 2
                    );
                    if (!tResult.Success) herhangiHata = true;
                }
                if (magazaFiyati > 0)
                {
                    var pResult = await CreateFiyatKartiAsync(
                        settings, accessToken,
                        isSales: true,
                        mmCode: malzemeKodu,
                        priceCode: $"{malzemeKodu}-M",
                        priceDescription: $"{malzemeKodu} Mağaza Fiyatı",
                        unitPrice: magazaFiyati,
                        startTimestamp: startTimestamp,
                        endTimestamp: endTimestamp,
                        uomRef: uomRef, priority: 1
                    );
                    if (!pResult.Success) herhangiHata = true;
                }
                return herhangiHata
                    ? (false, "Bir veya daha fazla fiyat kartı oluşturulamadı")
                    : (true, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateFiyatKartlariAsync hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Tek fiyat kartı oluşturur (iç kullanım)
        /// </summary>
        private static async Task<(bool Success, string ErrorMessage)> CreateFiyatKartiAsync(
            BulutERPSettings settings,
            string accessToken,
            bool isSales,
            string mmCode,
            string priceCode,
            string priceDescription,
            decimal unitPrice,
            long startTimestamp,
            long endTimestamp,
            int uomRef = 23,
            int priority = 0)
        {
            try
            {
                var body = new
                {
                    mmCode = mmCode,
                    priceCode = priceCode,
                    priceDescription = priceDescription,
                    unitPrice = unitPrice,
                    currency = 160,
                    unit = uomRef,
                    priority = priority,
                    fixedDateStartDate = startTimestamp,
                    fixedDateEndDate = endTimestamp
                };
                string jsonContent = JsonConvert.SerializeObject(body);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("lang", "TRTR");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string endpoint = isSales ? "salesprice/createWmm" : "purchaseprice/createWmm";
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/{endpoint}?mmCode={Uri.EscapeDataString(mmCode)}";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ Fiyat kartı hatası\n" +
                        $"   Kod     : {priceCode}\n" +
                        $"   Tür     : {(isSales ? "Satış" : "Alış")}\n" +
                        $"   Fiyat   : {unitPrice}\n" +
                        $"   HTTP    : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt   : {responseContent}");
                    JsonLog($"FIYAT_{priceCode}_{(isSales ? "SATIS" : "ALIS")}_HATA", body, responseContent);
                    return (false, responseContent);
                }
                JsonLog($"FIYAT_{priceCode}_{(isSales ? "SATIS" : "ALIS")}", body, responseContent);
                return (true, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateFiyatKartiAsync hatası: {ex}");
                return (false, ex.Message);
            }
        }
        #endregion

        #region Malzeme Kontrol (Aktarım Öncesi)
        /// <summary>
        /// Malzeme kodunun Logo'da zaten var olup olmadığını kontrol eder
        /// </summary>
        public static async Task<(bool Success, bool Exists, string ErrorMessage)> CheckMalzemeExistsAsync(
            string malzemeKodu,
            string accessToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(malzemeKodu))
                    return (false, false, "Malzeme kodu boş olamaz");
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, false, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                string sql = $"SELECT LOGICALREF FROM U_$V(firm)_ITEMS WHERE BOSTATUS<>1 AND CODE='{malzemeKodu.Replace("'", "''")}'";
                var result = await ExecuteSelectQueryAsync(sql, accessToken, 1);
                if (!result.Success)
                    return (false, false, result.ErrorMessage);
                bool exists = result.Data != null && result.Data.Count > 0;
                return (true, exists, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CheckMalzemeExistsAsync hatası: {ex.Message}");
                return (false, false, ex.Message);
            }
        }
        #endregion

        #region Barkod Kontrol
        /// <summary>
        /// Barkodun Logo'da başka bir malzemede kayıtlı olup olmadığını kontrol eder
        /// </summary>
        public static async Task<(bool Success, bool Exists, string ExistingCode, string ErrorMessage)> CheckBarkodExistsAsync(
            string barkod,
            string accessToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barkod))
                    return (true, false, null, null);
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                string sql = $"SELECT UB.BARCODE, I.CODE FROM U_$V(firm)_UNITBARCODE UB INNER JOIN U_$V(firm)_ITEMS I ON I.LOGICALREF = UB.ITEMREF WHERE UB.BARCODE = '{barkod.Replace("'", "''")}' AND I.BOSTATUS <> 1";
                var result = await ExecuteSelectQueryAsync(sql, accessToken, 1);
                if (!result.Success)
                    return (false, false, null, result.ErrorMessage);
                bool exists = result.Data != null && result.Data.Count > 0;
                string existingCode = exists ? result.Data[0]["CODE"]?.ToString() : null;
                return (true, exists, existingCode, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CheckBarkodExistsAsync hatası: {ex.Message}");
                return (false, false, null, ex.Message);
            }
        }
        #endregion

        #region Üretimden Giriş Fişi

        /// <summary>
        /// Birden fazla Excel satırından TEK üretimden giriş fişi oluşturur ve onaylar.
        /// MT → unitCode=KOLİ, TM → unitCode=ADET
        /// </summary>
        public static async Task<(bool Success, string FisNo, string ErrorMessage)> CreateUretimdenGirisFisiAsync(
            List<ExcelMalzemeRow> rows,
            string accessToken = null)
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                var aktifRows = rows.Where(r => r.Miktar > 0).ToList();
                if (aktifRows.Count == 0)
                    return (false, null, "Miktar > 0 olan satır bulunamadı!");
                DateTime now = DateTime.Now;
                string dateStr = now.ToString("yyyy-MM-ddT00:00:00.000+03:00");
                string fisNo = now.ToString("yyMMddHHmmss") + _random.Next(1000, 9999).ToString();
                var transactions = aktifRows.Select(row => new
                {
                    code = row.MalzemeKodu,
                    quantity = (double)row.Miktar,
                    unitCode = row.IsTakim ? "KOLİ" : "ADET"
                }).ToList<object>();
                var slip = new
                {
                    type = 13,
                    no = fisNo,
                    date = dateStr,
                    time = new { hour = now.Hour, minute = now.Minute },
                    orgUnit = "01",
                    orgUnitDesc = "01",
                    warehouse = "01.01.02",
                    warehouseDesc = "01",
                    department = "01",
                    departmentDesc = "01",
                    checkBoxCalculateAddTaxAmount = false,
                    comboboxConnectionType = 1,
                    transactions = transactions
                };
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Culture = System.Globalization.CultureInfo.InvariantCulture
                };
                string jsonContent = JsonConvert.SerializeObject(slip, jsonSettings);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("lang", "TRTR");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/itemslips/inputfromProdSlip";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ Üretim fişi hatası\n" +
                        $"   Fiş No : {fisNo}\n" +
                        $"   Satır  : {aktifRows.Count}\n" +
                        $"   HTTP   : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt  : {responseContent}");
                    JsonLog($"URETIM_{fisNo}_HATA", slip, responseContent);
                    return (false, null, responseContent);
                }
                JObject responseJson = JObject.Parse(responseContent);
                int logicalRef = responseJson["logicalRef"]?.Value<int>() ?? 0;
                string returnedFisNo = responseJson["no"]?.ToString() ?? fisNo;
                JsonLog($"URETIM_{fisNo}", slip, responseJson);
                bool approved = await ApproveSlipAsync(settings, accessToken, logicalRef);
                return (true, returnedFisNo, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateUretimdenGirisFisiAsync hatası: {ex}");
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Malzeme fişini onaylar (controlflag)
        /// </summary>
        private static async Task<bool> ApproveSlipAsync(
            BulutERPSettings settings,
            string accessToken,
            int logicalRef)
        {
            try
            {
                var body = new
                {
                    paramIOControl = "",
                    paramMinLevelControl = "",
                    paramMaxLevelControl = "",
                    paramSafeLevelControl = "",
                    paramNegLevelControl = "",
                    paramBudgetControl = "",
                    paramCheckPriceRange = "",
                    paramCheckPriceValid = "",
                    paramCheckARPRisk = "",
                    paramReorderLevelControl = "",
                    paramUndoPaymentProc = "",
                    paramCheckDTS = "",
                    paramCheckEDocument = "",
                    paramCreditRemainControl = ""
                };
                string jsonContent = JsonConvert.SerializeObject(body);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/itemslips/status/controlflag/ref?logicalRef={logicalRef}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, apiUrl);
                request.Headers.Add("access-token", accessToken);
                request.Headers.Add("firm", settings.FirmNr.Trim());
                request.Headers.Add("lang", "TRTR");
                request.Headers.Add("Accept", "application/json");
                request.Content = content;
                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync(
                        $"❌ Fiş onaylama hatası\n" +
                        $"   LogicalRef: {logicalRef}\n" +
                        $"   HTTP      : {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                        $"   Yanıt     : {responseContent}");
                    JsonLog($"ONAYLA_{logicalRef}_HATA", body, responseContent);
                    return false;
                }
                JsonLog($"ONAYLA_{logicalRef}", body, responseContent);
                return true;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ApproveSlipAsync hatası: {ex}");
                return false;
            }
        }
        #endregion

        #region JSON Log Helper
        /// <summary>
        /// Her API isteğini ve yanıtını JSONLog klasörüne yazar.
        /// Başarılı: MALZEME_KOD_CT1.json
        /// Hatalı  : MALZEME_KOD_CT1_HATA.json
        /// </summary>
        private static void JsonLog(string prefix, object istek, object yanit = null)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSONLog");
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var logObj = new
                {
                    istek = istek,
                    yanit = yanit
                };
                string fileName = $"{timestamp}_{prefix}.json";
                foreach (char c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');
                File.WriteAllText(
                    Path.Combine(logPath, fileName),
                    JsonConvert.SerializeObject(logObj, Formatting.Indented));
            }
            catch { /* log hatası işlemi durdurmasın */ }
        }
        #endregion

    }
}