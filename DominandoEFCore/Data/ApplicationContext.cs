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
        public DbSet<Estado> Estados { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder
                .UseNpgsql(strConnection)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging();
        }


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
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
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
        /* );
 }*/




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
        /// Configuração para habilitar o carregamento lento em todas as consultas 
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .UseLazyLoadingProxies();
        }*/





        /// <summary>
        /// Configuração para habilitar detalhes dos erros no console da aplicacao
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder.UseNpgsql(strConnection)
                .LogTo(Console.WriteLine)
                .EnableDetailedErrors();
        }*/




        /// <summary>
        /// Configuração para o uso de BatchSize
        /// o BatchSize otimiza a insersao de dados ao enviar para o banco de dados os lotes de inserts
        /// o tamanho padrao para lotes de inserts sao de 42, mas podemos configurar um tamanho maior
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder
                .UseNpgsql(strConnection, o => o.MaxBatchSize(100))
                .EnableSensitiveDataLogging() 
                .LogTo(Console.WriteLine, LogLevel.Information);
        }*/






        /// <summary>
        /// Configuração para o uso de TimeOut
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder
                .UseNpgsql(strConnection, o => o.CommandTimeout(5))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }*/





        /// <summary>
        /// Deixando a aplicacao mais resiliente
        /// Habilitando o EnableRetryOnFailure, ira trazer mais resiliencia ao banco de dados
        /// Vai tentar fazer as requisicoes mesmo em caso de alguma falha no banco de dados por algumas vezes
        /// e possivel passar um valor inteiro como parametro do metodo para indicar a quantidade de tentativas
        /// que deve ser feito antes de lancar a exceção vinda do banco de dados
        /// </summary>
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=DevIO-02;";

            optionsBuilder
                .UseNpgsql(
                    strConnection,
                    // 1º Parametro: Tenta realizar a requisicao por 4 vezes,
                    // 2º Parametro: Tempo maximo de 10 segundos para as tentativas de requisicao para o banco de dados,
                    // 3º Parametro: Não sobrescrever os codigos de erro ja existentes no EF Core 
                    o => o.EnableRetryOnFailure(4, TimeSpan.FromSeconds(10), null)) 
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }*/


        // =============================================== OnModelCreating =================================================================


        /// <summary>
        /// Filtro para todas as consultas, filtrando registros que sao marcados como excluido com valores booleanos no banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Departamento>().HasQueryFilter(q => !q.Excluido);
        }*/





        /// <summary>
        /// Uso de collaction no Entity Framework Core
        /// A collaction determina algumas configurações de agrupamento e diferenciação de textos no banco de dados
        /// Podemos configurar se o banco fará diferenciação entre letras maiusculas e minusculas,
        /// quais campos especificos esssas regras se aplicam, ignorar acentuacao, etc....
        /// CI = Case Insensitive
        /// CS = Case Sensitive
        /// AI = Acento Insensitive
        /// AS = Acento Sensitive
        /// Mais informações:
        /// https://docs.microsoft.com/pt-br/ef/core/miscellaneous/collations-and-case-sensitivity
        /// </summary>
        /// <param name="modelBuilder"></param>
       /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração Global
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AI");

            // Configurãção para uma determinada tabela
            modelBuilder.Entity<Departamento>().Property(p => p.Descricao)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS");
        }*/




        /// <summary>
        /// Configurando Sequences no banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
       /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("MinhaSequencia")
                .StartsAt(1)
                .IncrementsBy(2)
                .HasMin(1)
                .HasMax(10)
                .IsCyclic(); // <- Reinicia sequencia quando atinge o valor maximo

            modelBuilder.Entity<Departamento>()
                .Property(p => p.Id)
                .HasDefaultValueSql("nextval('\"MinhaSequencia\"')");
        }*/




        /// <summary>
        /// Configurando Indices no banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Departamento>()
                .HasIndex(p => new { p.Descricao, p .Ativo })
                .HasDatabaseName("idx_meu_indice_composto")
                .HasFilter("\"Descricao\" IS NOT NULL")
                .IsUnique();
        }
        */




        /// <summary>
        /// Propagação de dados iniciais para o banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Estado>()
                .HasData(new[]
                {
                    new Estado{ Id = 1, Nome = "São Paulo" },
                    new Estado{ Id = 2, Nome = "Sergipe" }
                });
        }*/




        /// <summary>
        /// Esquemas no banco de dados
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("cadastros");
            modelBuilder.Entity<Estado>().ToTable("Estados", "SegundoEsquema");
        }
    }
}
