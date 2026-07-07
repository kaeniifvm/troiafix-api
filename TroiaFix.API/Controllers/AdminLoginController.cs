using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TroiaFix.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminLoginController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Credenciais simples (em produção, validar contra banco)
            if (req.Usuario == "admin" && req.Senha == "Tro1a-Adm1n-2024")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("Tro1a-F1x-S3cret-K3y-2024-Muda-Isso!");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, req.Usuario) }),
                    Expires = DateTime.UtcNow.AddHours(8),
                    Issuer = "TroiaFix",
                    Audience = "TroiaFixClient",
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new { token = tokenHandler.WriteToken(token) });
            }

            return Unauthorized(new { erro = "Credenciais inválidas" });
        }
    }

    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
    }
}