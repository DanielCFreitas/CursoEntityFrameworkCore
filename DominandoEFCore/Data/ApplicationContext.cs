using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DominandoEFCore.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }




        // =============================================== OnConfiguring =================================================================

        /// <summary>
        /// OnConfiguring Simples 
        /// Apenas com a string de conexao com o banco de dados
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection);
        }*/





        /// <summary>
        /// Configuração para todas as consultas fazer o SplitQuery
        /// O SplitQuery divide a consulta em outras menores para evitar de carregar dados a mais nas consultas, em alguns casos pode ser interessante o uso
        /// também é possível usar o SplitQuery apenas na hora da consulta, sem ser feito com configuração automatica igual o metodo abaixo
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection, p => p.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }*/





        /// <summary>
        /// Configuração para fazer os logs do Entity Framework no Console
        /// O SplitQuery divide a consulta em outras menores para evitar de carregar dados a mais nas consultas, em alguns casos pode ser interessante o uso
        /// também é possível usar o SplitQuery apenas na hora da consulta, sem ser feito com configuração automatica igual o metodo abaixo
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .EnableSensitiveDataLogging() // Para exibir os valores dos parametros ao logar o SQL no console
                .LogTo(
                    // Onde deve ser logado os comandos executados pelo EF  
                    Console.WriteLine,
                    // Eventos especificos que devem ser logados
                    new[] { CoreEventId.ContextInitialized, RelationalEventId.CommandExecuted },
                    // Log a nivel de Informação
                    LogLevel.Information,
                    // Configurações de como deve ser logado (No exemplo, com horario local e em apenas uma unica linha cada log)
                    DbContextLoggerOptions.LocalTime /*| DbContextLoggerOptions.SingleLine */
                );
        }





        /// <summary>
        /// Configuração para fazer os logs do Entity Framework em um arquivo .txt
        /// </summary>
        /*private readonly StreamWriter _writer = new StreamWriter("meu_log_do_ef_core.txt", append: true); // Para registrar os logs do Entity Framework em um arquivo
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .LogTo(_writer.WriteLine, LogLevel.Information);
        }
         public override void Dispose()
        {
            base.Dispose();
            _writer.Dispose();
        }*/





        /// <summary>
        /// Configuração para habilitar detalhes dos erros no console da aplicacao
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .UseLazyLoadingProxies();
        }*/





        /// <summary>
        /// Configuração para habilitar o carregamento lento em todas as consultas 
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .LogTo(Console.WriteLine)
                .EnableDetailedErrors();
        }*/







        // =============================================== OnModelCreating =================================================================


        /// <summary>
        /// Filtro para todas as consultas, filtrando registros que sao marcados como excluido com valores booleanos no banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Filtro Global
            //modelBuilder.Entity<Departamento>().HasQueryFilter(q => !q.Excluido);
        }
    }
}
