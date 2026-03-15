// LicenseHelper.cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BulutERPAktarim.Classes
{
    internal class LicenseResult
    {
        internal bool IsValid { get; set; }
        internal string Message { get; set; }
        internal DateTime? ExpiryDate { get; set; }
    }

    internal static class LicenseHelper
    {
        private const string ConfigKey_ApiUrl = "cfg_api_url";
        private const string ConfigKey_SecretKey = "cfg_secret_key";
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        // ── Ana Kontrol Metodu ───────────────────────────────────────
        internal static async Task<LicenseResult> CheckLicenseAsync()
        {
            string hwid = HwidHelper.GetHwid();
            return await ValidateWithApiAsync(hwid);
        }
        // ── Config Okuma (SQLite'tan şifreli) ───────────────────────
        private static async Task<string> GetApiUrlAsync()
        {
            string val = await GetConfigValue(ConfigKey_ApiUrl);
            if (val == null)
                return new LicenseResult { IsValid = false, Message = "Lisans yapılandırması bulunamadı." }.ToString();
            return EncryptionHelper.Decrypt(val);
        }
        private static async Task<string> GetSecretKeyAsync()
        {
            string val = await GetConfigValue(ConfigKey_SecretKey);
            if (val == null) return "";
            return EncryptionHelper.Decrypt(val);
        }
        private static async Task<string> GetConfigValue(string key)
        {
            string query = "SELECT Value FROM AppConfig WHERE Key = @key LIMIT 1";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@key", key } };
            DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, p);
            if (dt.Rows.Count == 0) return null;
            return dt.Rows[0]["Value"]?.ToString();
        }
        // ── API İsteği ───────────────────────────────────────────────
        private static async Task<LicenseResult> ValidateWithApiAsync(string hwid)
        {
            try
            {
                string apiUrl = await GetApiUrlAsync();
                string secretKey = await GetSecretKeyAsync();
                if (string.IsNullOrWhiteSpace(apiUrl))
                    return new LicenseResult { IsValid = false, Message = "Lisans yapılandırması bulunamadı." };
                var payload = new { hwid = hwid, secretKey = secretKey };
                string json = JsonConvert.SerializeObject(payload);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _http.PostAsync($"{apiUrl}/validate", content);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync($"License API HTTP hatası: {response.StatusCode} - {responseBody}");
                    return new LicenseResult { IsValid = false, Message = "Lisans sunucusuna bağlanılamadı." };
                }
                var result = JsonConvert.DeserializeObject<ApiLicenseResponse>(responseBody);
                if (result == null)
                    return new LicenseResult { IsValid = false, Message = "Sunucudan geçersiz yanıt alındı." };
                return new LicenseResult
                {
                    IsValid = result.IsValid,
                    Message = result.Message,
                    ExpiryDate = result.ExpiryDate
                };
            }
            catch (HttpRequestException)
            {
                return new LicenseResult { IsValid = false, Message = "İnternet bağlantısı yok veya lisans sunucusuna ulaşılamıyor." };
            }
            catch (TaskCanceledException)
            {
                return new LicenseResult { IsValid = false, Message = "Lisans sunucusu zaman aşımına uğradı." };
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"License API istek hatası: {ex.Message}");
                return new LicenseResult { IsValid = false, Message = $"Lisans kontrolü sırasında hata: {ex.Message}" };
            }
        }
        // ── API Response Model ───────────────────────────────────────
        private class ApiLicenseResponse
        {
            [JsonProperty("isValid")]
            public bool IsValid { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("expiryDate")]
            public DateTime? ExpiryDate { get; set; }
        }
    }
}