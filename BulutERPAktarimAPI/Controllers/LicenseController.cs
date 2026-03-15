using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BulutERPAktarimAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connStr;
        private readonly string _secretKey;

        public LicenseController(IConfiguration config)
        {
            _config = config;
            _connStr = config.GetConnectionString("DefaultConnection")!;
            _secretKey = config["License:SecretKey"]!;
        }

        // ── POST /api/license/validate ────────────────────────────────
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] LicenseRequest req)
        {
            if (req?.SecretKey != _secretKey)
                return Unauthorized(new LicenseResponse { IsValid = false, Message = "Geçersiz istek." });
            if (string.IsNullOrWhiteSpace(req.Hwid))
                return BadRequest(new LicenseResponse { IsValid = false, Message = "HWID boş olamaz." });
            try
            {
                using SqlConnection conn = new SqlConnection(_connStr);
                const string sql = @"
                    SELECT TOP 1 Hwid, ExpiryDate, IsActive
                    FROM Licenses
                    WHERE Hwid = @hwid";
                var license = await conn.QueryFirstOrDefaultAsync<LicenseRecord>(sql, new { hwid = req.Hwid });
                if (license == null)
                    return Ok(new LicenseResponse { IsValid = false, Message = "Bu cihaz için lisans kaydı bulunamadı." });
                if (!license.IsActive)
                    return Ok(new LicenseResponse { IsValid = false, Message = "Lisansınız devre dışı bırakılmıştır." });
                if (license.ExpiryDate < DateTime.UtcNow)
                    return Ok(new LicenseResponse
                    {
                        IsValid = false,
                        Message = $"Lisansınızın süresi dolmuştur. (Son gün: {license.ExpiryDate:dd.MM.yyyy})"
                    });
                return Ok(new LicenseResponse
                {
                    IsValid = true,
                    Message = "Lisans geçerli.",
                    ExpiryDate = license.ExpiryDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LicenseResponse { IsValid = false, Message = "Sunucu hatası oluştu." });
            }
        }

        // ── POST /api/license/activate ────────────────────────────────
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateRequest req)
        {
            if (req?.SecretKey != _secretKey)
                return Unauthorized(new { success = false, message = "Geçersiz istek." });
            if (string.IsNullOrWhiteSpace(req.Hwid))
                return BadRequest(new { success = false, message = "HWID boş olamaz." });
            try
            {
                using SqlConnection conn = new SqlConnection(_connStr);
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM Licenses WHERE Hwid = @hwid)
                        UPDATE Licenses SET ExpiryDate = @expiry, IsActive = 1, Note = @note, UpdatedAt = SYSUTCDATETIME() WHERE Hwid = @hwid
                    ELSE
                        INSERT INTO Licenses (Hwid, ExpiryDate, IsActive, Note) VALUES (@hwid, @expiry, 1, @note)";
                await conn.ExecuteAsync(sql, new { hwid = req.Hwid, expiry = req.ExpiryDate, note = req.Note });
                return Ok(new { success = true, message = "Lisans eklendi/güncellendi.", hwid = req.Hwid, expiryDate = req.ExpiryDate, note = req.Note });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ── POST /api/license/deactivate ─────────────────────────────
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateRequest req)
        {
            if (req?.SecretKey != _secretKey)
                return Unauthorized(new { success = false, message = "Geçersiz istek." });
            try
            {
                using SqlConnection conn = new SqlConnection(_connStr);
                const string sql = "UPDATE Licenses SET IsActive = 0, UpdatedAt = SYSUTCDATETIME() WHERE Hwid = @hwid";
                int rows = await conn.ExecuteAsync(sql, new { hwid = req.Hwid });
                if (rows == 0)
                    return NotFound(new { success = false, message = "Lisans bulunamadı." });
                return Ok(new { success = true, message = "Lisans devre dışı bırakıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ── GET /api/license/list ─────────────────────────────────────
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string secretKey)
        {
            if (secretKey != _secretKey)
                return Unauthorized();
            using SqlConnection conn = new SqlConnection(_connStr);
            const string sql = "SELECT Hwid, ExpiryDate, IsActive, Note, CreatedAt, UpdatedAt FROM Licenses ORDER BY CreatedAt DESC";
            var list = await conn.QueryAsync<LicenseRecord>(sql);
            return Ok(list);
        }
    }

    // ── Modeller ─────────────────────────────────────────────────────

    public class LicenseRequest
    {
        public string Hwid { get; set; } = "";
        public string SecretKey { get; set; } = "";
    }

    public class ActivateRequest
    {
        public string Hwid { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public string SecretKey { get; set; } = "";
        public string? Note { get; set; }
    }

    public class DeactivateRequest
    {
        public string Hwid { get; set; } = "";
        public string SecretKey { get; set; } = "";
    }

    public class LicenseResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = "";
        public DateTime? ExpiryDate { get; set; }
    }

    public class LicenseRecord
    {
        public string Hwid { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}