// DbContext para Entity Framework com comentários
using AcessoSeguro.Models;
using Microsoft.EntityFrameworkCore;

namespace AcessoSeguro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; } // Representa a tabela de usuários no banco
    }
}