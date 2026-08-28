using HelpDeskAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace HelpDeskAPI.Data
{
    public class HelpDeskContext : DbContext
    {
        public HelpDeskContext(DbContextOptions<HelpDeskContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Historial> Historiales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Creador)
                .WithMany(u => u.TicketsCreados)
                .HasForeignKey(t => t.CreadorId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Tecnico)
                .WithMany(u => u.TicketsAsignados)
                .HasForeignKey(t => t.TecnicoId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Admin" },
                new Rol { Id = 2, Nombre = "Tecnico" },
                new Rol { Id = 3, Nombre = "Empleado" }
            );

            modelBuilder.Entity<Estado>().HasData(
                new Estado { Id = 1, Nombre = "Abierto" },
                new Estado { Id = 2, Nombre = "Asignado" },
                new Estado { Id = 3, Nombre = "En Proceso" },
                new Estado { Id = 4, Nombre = "Resuelto" },
                new Estado { Id = 5, Nombre = "Cerrado" }
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nombre = "Juan Empleado",
                    Email = "juan@empresa.com",
                    PasswordHash = "12345", 
                    RolId = 3
                }
            );
        }
    }
}