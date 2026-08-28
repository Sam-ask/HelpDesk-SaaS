using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDeskAPI.Data;
using HelpDeskAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public TicketsController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Estado)
                .Include(t => t.Creador)
                .Include(t => t.Tecnico)
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket([FromBody] TicketCreateDTO dto)
        {
            var nuevoTicket = new Ticket
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                CreadorId = dto.CreadorId,
                EstadoId = dto.EstadoId,
                CategoriaId = dto.CategoriaId, 
                PrioridadId = dto.PrioridadId, 
                FechaCreacion = DateTime.UtcNow
            };

            _context.Tickets.Add(nuevoTicket);
            await _context.SaveChangesAsync();

            return Ok(nuevoTicket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarTicket(int id, [FromBody] TicketEditDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID del ticket no coincide.");
            }

            if (dto.EstadoId < 1 || dto.EstadoId > 5)
            {
                return BadRequest($"Error de comunicación: Angular intentó enviar el EstadoId número '{dto.EstadoId}', el cual no existe en la base de datos.");
            }

            var ticketEnDb = await _context.Tickets.FindAsync(id);
            if (ticketEnDb == null)
            {
                return NotFound("El ticket no existe.");
            }

            ticketEnDb.Titulo = dto.Titulo;
            ticketEnDb.Descripcion = dto.Descripcion;
            ticketEnDb.EstadoId = dto.EstadoId;
            ticketEnDb.CategoriaId = dto.CategoriaId; // <-- AHORA SÍ SE ACTUALIZA
            ticketEnDb.PrioridadId = dto.PrioridadId; // <-- AHORA SÍ SE ACTUALIZA

            if (!string.IsNullOrEmpty(dto.Comentario))
            {
                var historial = new Historial
                {
                    TicketId = ticketEnDb.Id,
                    UsuarioId = dto.CreadorId,
                    Accion = $"Ticket Editado. Estado: {dto.EstadoId}. Comentario: {dto.Comentario}",
                    Fecha = DateTime.UtcNow
                };
                _context.Historiales.Add(historial);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/asignar")]
        public async Task<IActionResult> AsignarTicket(int id, [FromBody] TicketUpdateDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null) return NotFound("El ticket no existe.");

                ticket.TecnicoId = dto.TecnicoId;
                ticket.EstadoId = dto.NuevoEstadoId;

                var historial = new Historial
                {
                    TicketId = ticket.Id,
                    UsuarioId = dto.TecnicoId,
                    Accion = $"Cambio a estado {dto.NuevoEstadoId}. Comentario: {dto.Comentario}",
                    Fecha = DateTime.UtcNow
                };

                _context.Historiales.Add(historial);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Mensaje = "Ticket actualizado y trazabilidad guardada exitosamente.",
                    TicketId = ticket.Id
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Ocurrió un error en el servidor y la transacción fue revertida.");
            }
        }

        [HttpGet("{id}/historial")]
        public async Task<ActionResult> GetHistorialTicket(int id)
        {
            var ticketExiste = await _context.Tickets.AnyAsync(t => t.Id == id);
            if (!ticketExiste) return NotFound("El ticket no existe.");

            var historial = await _context.Historiales
                .Where(h => h.TicketId == id)
                .Include(h => h.Usuario)
                .OrderByDescending(h => h.Fecha)
                .Select(h => new
                {
                    IdHistorial = h.Id,
                    Accion = h.Accion,
                    Fecha = h.Fecha,
                    Usuario = h.Usuario.Nombre
                })
                .ToListAsync();

            return Ok(historial);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound("El ticket no existe.");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok("Ticket eliminado correctamente.");
        }
    }

    

    public class TicketCreateDTO
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int CreadorId { get; set; }
        public int EstadoId { get; set; }
        public int CategoriaId { get; set; } // <-- FALTABAN AQUÍ
        public int PrioridadId { get; set; } // <-- FALTABAN AQUÍ
    }

    public class TicketEditDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int CreadorId { get; set; }
        public int EstadoId { get; set; }
        public int CategoriaId { get; set; }
        public int PrioridadId { get; set; }
        public string? Comentario { get; set; }
    }

    public class TicketUpdateDTO
    {
        public int TecnicoId { get; set; }
        public int NuevoEstadoId { get; set; }
        public string Comentario { get; set; }
    }
}