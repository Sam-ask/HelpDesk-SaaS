using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskAPI.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } 
        public ICollection<Usuario> Usuarios { get; set; }
    }

    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public int RolId { get; set; }
        [ForeignKey("RolId")]
        public Rol Rol { get; set; }
        [InverseProperty("Creador")]
        public ICollection<Ticket> TicketsCreados { get; set; }
        [InverseProperty("Tecnico")]
        public ICollection<Ticket> TicketsAsignados { get; set; }
    }

    public class Estado
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } 
    }

    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int EstadoId { get; set; }
        [ForeignKey("EstadoId")]
        public int CategoriaId { get; set; } = 1;
        public int PrioridadId { get; set; } = 1;
        public Estado? Estado { get; set; } 
        public int CreadorId { get; set; }
        [ForeignKey("CreadorId")]
        public Usuario? Creador { get; set; } 
        public int? TecnicoId { get; set; }
        [ForeignKey("TecnicoId")]
        public Usuario? Tecnico { get; set; } 

        public ICollection<Historial>? Historiales { get; set; }
    }
    public class Historial
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        public string Accion { get; set; } 

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}