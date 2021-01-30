using CursoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CursoEFCore.Data
{
    public class ApplicationContext : DbContext
    {
        private static readonly ILoggerFactory _logger = LoggerFactory.Create(p => p.AddConsole()); // Utilizacao do LOG dos comandos SQL gerados pelo EF 


        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        /// <summary>
        /// Configurações do Banco de Dados
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(_logger)  // Utilizacao do LOG dos comandos SQL gerados pelo EF 
                .EnableSensitiveDataLogging()  // Utilizacao do LOG dos comandos SQL gerados pelo EF 
                .UseNpgsql(
                    "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=CursoEF;", // String de conexão com o banco de dados
                    p => p.EnableRetryOnFailure( // Habilita a tentativa de conexao com o banco de dados em caso de falha
                        maxRetryCount: 2, // Numero maximo de tentativas de conexao em caso de falha
                        maxRetryDelay: TimeSpan.FromSeconds(5),  // Tempo entre cada tentativa de conexao
                        errorCodesToAdd: null // Codigo de erro adicional para o EF interpretar (null, utiliza o padrao)
                    )
                    .MigrationsHistoryTable("curso_ef_core") // Nome da tabela que ficara o historico das migrations
                ); 
        }

        /// <summary>
        /// Configurações dos modelos, aplicando CodeFirst neste método
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly); // Acessa os arquivos de Configuracao do Code First com FluentAPI que estão na pasta Configuration
            MapearPropriedadesEsquecidas(modelBuilder); // Chamada do método para maepar propriedades que foram esquecidas de ser mapeadas
        }

        /// <summary>
        /// Método para mapear propriedades esquecidas quando usado CodeFirst
        /// Neste caso, as propriedades do tipo string que nao foram mapeadas
        /// serão setadas com o vipo VARCHAR(100)
        /// </summary>
        /// <param name="modelBuilder"></param>
        private void MapearPropriedadesEsquecidas(ModelBuilder modelBuilder)
        {
            foreach(var entity in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entity.GetProperties().Where(w => w.ClrType == typeof(string));
               
                foreach(var property in properties)
                {
                    if (string.IsNullOrEmpty(property.GetColumnType()) && !property.GetMaxLength().HasValue)
                    {
                        // property.SetMaxLength(100);
                        property.SetColumnType("VARCHAR(100)");
                    }
                }
            }
        }
    }
}
