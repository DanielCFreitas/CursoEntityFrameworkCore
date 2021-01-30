using DominandoEFCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Diagnostics;
using System.Linq;

namespace DominandoEFCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            new ApplicationContext().Departamentos.AsNoTracking().Any();
            _count = 0;
            GerenciarEstadoDaConexao(false);
            _count = 0;
            GerenciarEstadoDaConexao(true);
        }

        /// <summary>
        /// Executando método para criar o banco de dados para caso ele ainda não exista, de acordo com a string de conexao
        /// que foi passada dentro do ApplicationContext
        /// </summary>
        static void EnsureCreated()
        {
            var db = new ApplicationContext();
            db.Database.EnsureCreated();
        }

        /// <summary>
        /// Executando método para excluir banco de dados
        /// </summary>
        static void EnsureDeleted()
        {
            var db = new ApplicationContext();
            db.Database.EnsureDeleted();
        }

        /// <summary>
        /// Resolvendo problema para instâncias de contextos diferentes
        /// (ApplicationContext / ApplicationContextCidade) para um mesmo banco de dados
        /// </summary>
        static void GapDoEnsureCreated()
        {
            using var db1 = new ApplicationContext();
            using var db2 = new ApplicationContextCidade();

            db1.Database.EnsureCreated();
            db2.Database.EnsureCreated();

            var databaseCreator = db2.GetService<IRelationalDatabaseCreator>();
            databaseCreator.CreateTables();
        }

        /// <summary>
        /// Fazendo um HealthCheck do banco de dados, verificar se a conexão com o banco está funcionando
        /// </summary>
        static void HealthCheckBancoDeDados()
        {
            using var db = new ApplicationContext();
            var canConnect = db.Database.CanConnect();

            if (canConnect)
            {
                Console.WriteLine("Posso me conectar");
            }
            else
            {
                Console.WriteLine("Não posso me conectar");
            }
        }

        /// <summary>
        /// Deixando o gerenciamento de conexão com o desenvolvedor ao invés do EntityFramework gerenciar
        /// Ganho de Performance e diminuição do número de conexão abertas
        /// 
        /// Programa que vai no Main para testar: 
        /// 
        /// new ApplicationContext().Departamentos.AsNoTracking().Any();
        //_count = 0;
        //  GerenciarEstadoDaConexao(false);
        //_count = 0;
        //  GerenciarEstadoDaConexao(true);
        /// </summary>
        static int _count;
        static void GerenciarEstadoDaConexao(bool gerenciarEstadoConexao)
        {
            using var db = new ApplicationContext();
            var time = Stopwatch.StartNew();

            var conexao = db.Database.GetDbConnection();
            conexao.StateChange += (_, __) => ++_count;

            if (gerenciarEstadoConexao)
            {
                conexao.Open();
            }

            for(var i = 0; i<200; i++)
            {
                db.Departamentos.AsNoTracking().Any();
            }

            time.Stop();
            var mensagem = $"Tempo: {time.Elapsed.ToString()}, {gerenciarEstadoConexao}, Contador: {_count}";

            Console.WriteLine(mensagem);
        }
    }
}
