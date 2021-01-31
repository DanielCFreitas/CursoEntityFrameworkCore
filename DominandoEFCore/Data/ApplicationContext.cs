using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace DominandoEFCore.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder
                .UseNpgsql(strConnection) // String de conexao com o banco de dados
                //.UseLazyLoadingProxies() // Ativar o tipo de carregamento lento
                .EnableSensitiveDataLogging() // Para exibir os valores dos parametros ao logar o SQL no console
                .LogTo(Console.WriteLine, LogLevel.Information); // Onde deve logar a consulta e o nivel do Log
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Filtro Global
            modelBuilder.Entity<Departamento>().HasQueryFilter(q => !q.Excluido);
        }
    }
}
