using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
                .UseNpgsql(
                    // String de conexao com o banco de dados
                    strConnection

                // Configura de forma global o splitquery para divisão de consultas no banco de dados
                // p => p.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery) 
                )

                // Ativar o tipo de carregamento lento
                //.UseLazyLoadingProxies() 

                // =================== CONFIGURAÇÕES DE LOG =====================
                .EnableSensitiveDataLogging() // Para exibir os valores dos parametros ao logar o SQL no console

                .LogTo(
                    // Onde deve ser logado os comandos executados pelo EF  
                    Console.WriteLine,
                    // Eventos especificos que devem ser logados
                    new[] { CoreEventId.ContextInitialized, RelationalEventId.CommandExecuted },
                    // Log a nivel de Informação
                    LogLevel.Information,
                    // Configurações de como deve ser logado (No exemplo, com horario local e em apenas uma unica linha cada log)
                    DbContextLoggerOptions.LocalTime | DbContextLoggerOptions.SingleLine 
                ); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Filtro Global
            //modelBuilder.Entity<Departamento>().HasQueryFilter(q => !q.Excluido);
        }
    }
}
