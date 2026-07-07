using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TroiaFix.API.Data;
using TroiaFix.API.Models;

namespace TroiaFix.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("ativar")]
        public async Task<IActionResult> AtivarKey([FromBody] AtivarRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Hwid))
                {
                    return BadRequest(new { sucesso = false, mensagem = "Key e HWID são obrigatórios" });
                }

                string keyHash = HashSHA256(request.Key);
                var license = await _db.Licenses.FirstOrDefaultAsync(l => l.KeyHash == keyHash);

                if (license == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Key inválida" });
                }

                if (license.Status == "Revogada")
                {
                    return StatusCode(403, new { sucesso = false, mensagem = "Key revogada pelo administrador" });
                }

                if (license.Status == "Suspensa")
                {
                    return StatusCode(403, new { sucesso = false, mensagem = "Key suspensa" });
                }

                if (license.DataExpiracao.HasValue && license.DataExpiracao < DateTime.UtcNow)
                {
                    license.Status = "Expirada";
                    await _db.SaveChangesAsync();
                    return StatusCode(403, new { sucesso = false, mensagem = "Key expirada" });
                }

                if (string.IsNullOrWhiteSpace(license.Hwid))
                {
                    license.Hwid = request.Hwid;
                    license.DataAtivacao = DateTime.UtcNow;
                    license.Status = "Ativa";
                    license.UltimoAcesso = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    return Ok(new { sucesso = true, mensagem = "Login autorizado", dataAtivacao = license.DataAtivacao });
                }

                if (license.Hwid != request.Hwid)
                {
                    return StatusCode(403, new { sucesso = false, mensagem = "Key já utilizada em outro computador" });
                }

                license.UltimoAcesso = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Ok(new { sucesso = true, mensagem = "Login autorizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro interno: {ex.Message}" });
            }
        }

        private string HashSHA256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public class AtivarRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Hwid { get; set; } = string.Empty;
    }
}