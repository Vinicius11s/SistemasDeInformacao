using Microsoft.EntityFrameworkCore;
using UniEventosAPI.Models;

namespace UniEventosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        public DbSet<Evento> Eventos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Evento>().HasData(
                new Evento
                {
                    Id = 1,
                    Titulo = "Semana Acadêmica de SI",
                    Descricao = "Palestras e minicursos da graduação",
                    Local = "Auditório Central",
                    Data = "25/09/2026",
                    Status = StatusEvento.Aberto
                },
                new Evento
                {
                    Id = 2,
                    Titulo = "Feira de Estágios",
                    Descricao = "Empresas da região apresentam vagas",
                    Local = "Pátio da Faculdade",
                    Data = "02/10/2026",
                    Status = StatusEvento.Aberto
                }
            );
        }
    }
}
