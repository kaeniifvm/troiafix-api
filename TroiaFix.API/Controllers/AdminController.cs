using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TroiaFix.API.Data;
using TroiaFix.API.Models;

namespace TroiaFix.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
   //  [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db) => _db = db;

        [HttpPost("criar-key")]
        public async Task<IActionResult> CriarKey([FromBody] CriarKeyRequest req)
        {
            string keyHash = HashSHA256(req.Key);
            
            if (await _db.Licenses.AnyAsync(l => l.KeyHash == keyHash))
                return BadRequest(new { erro = "Key já existe" });

            var license = new License
            {
                KeyHash = keyHash,
                KeyOriginal = req.Key,
                Status = "Pendente",
                DataExpiracao = req.DiasValidade > 0 
                    ? DateTime.UtcNow.AddDays(req.DiasValidade) 
                    : null
            };

            _db.Licenses.Add(license);
            await _db.SaveChangesAsync();

            return Ok(new { sucesso = true, licenseId = license.Id });
        }

        [HttpGet("licencas")]
        public async Task<IActionResult> ListarLicencas()
        {
            var licencas = await _db.Licenses
                .OrderByDescending(l => l.DataCriacao)
                .Select(l => new
                {
                    l.Id,
                    l.KeyOriginal,
                    l.Hwid,
                    l.Status,
                    l.DataAtivacao,
                    l.UltimoAcesso,
                    l.DataExpiracao,
                    l.DataCriacao
                })
                .ToListAsync();

            return Ok(licencas);
        }

        [HttpPut("revogar/{id}")]
        public async Task<IActionResult> RevogarKey(int id)
        {
            var license = await _db.Licenses.FindAsync(id);
            if (license == null) return NotFound();

            license.Status = "Revogada";
            await _db.SaveChangesAsync();
            return Ok(new { sucesso = true });
        }

        [HttpPut("resetar-hwid/{id}")]
        public async Task<IActionResult> ResetarHwid(int id)
        {
            var license = await _db.Licenses.FindAsync(id);
            if (license == null) return NotFound();

            license.Hwid = null;
            license.Status = "Pendente";
            await _db.SaveChangesAsync();

            return Ok(new { sucesso = true });
        }

        private string HashSHA256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public class CriarKeyRequest
    {
        public string Key { get; set; }
        public int DiasValidade { get; set; } = 0;
    }
}