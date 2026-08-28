using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDeskAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HelpDeskContext _context;
        private readonly IConfiguration _config;

        public AuthController(HelpDeskContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.PasswordHash == dto.Password);

            if (usuario == null)
            {
                return Unauthorized("Credenciales inválidas."); 
            }

          
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre) 
            };

          
            var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), 
                signingCredentials: creds
            );

            return Ok(new
            {
                mensaje = $"Bienvenido {usuario.Nombre}",
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}