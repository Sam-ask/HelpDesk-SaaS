using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDeskAPI.Data; 

namespace HelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UsuariosController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public UsuariosController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    RolNombre = u.Rol.Nombre
                })
                .ToListAsync();

            return Ok(usuarios);
        }
    }
}