using DominandoEFCore.Data;
using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DominandoEFCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            ConsultaProjetada();
        }

        #region Metodos de apoio para o curso
        /// <summary>
        /// Setup apenas para popular o banco de dados para arealizar alguns exemplos
        /// </summary>
        /// <param name="db"></param>
        static void Setup(ApplicationContext db)
        {
            if (db.Database.EnsureCreated())
            {
                db.Departamentos.AddRange(
                    new Departamento()
                    {
                        Ativo = true,
                        Descricao = "Departamento 01",
                        Funcionarios = new List<Funcionario>
                        {
                            new Funcionario
                            {
                                Nome = "Rafael Almeida",
                                CPF = "999999991",
                                RG = "2100062"
                            }
                        },
                        Excluido = true
                    },
                    new Departamento()
                    {
                        Descricao = "Departamento 02",
                        Funcionarios = new List<Funcionario>
                        {
                            new Funcionario
                            {
                                Nome = "Bruno Brito",
                                CPF = "8888888882",
                                RG = "3100062"
                            },
                            new Funcionario
                            {
                                Nome = "Eduardo Pires",
                                CPF = "77777777772",
                                RG = "1111052"
                            }
                        }
                    }
                );
                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }
        #endregion




        #region Metodos do DATABASE para operações relacionadas ao banco de dados para um contexto
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
            // Aqui seria outro contexto, o que existia antes para o exemplo foi excluido para organizar o projeto
            using var db2 = new ApplicationContext(); 

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

        /// <summary>
        /// Executa comandos SQL no banco de dados
        /// </summary>
        static void ExecutarSQL()
        {
            using var db = new ApplicationContext();

            // Primeira Opção
            using(var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteNonQuery();
            }

            // Segunda Opção (Deixando passar DBParameter, evitando SQL Injection)
            var descricao = "Teste";
            db.Database.ExecuteSqlRaw("UPDATE departamento SET descricao={0} WHERE id=1", descricao);

            // Terceira Opção (Usa interpolação usando DBParameter, evita também SQL Injection)
            db.Database.ExecuteSqlInterpolated($"UPDATE departamento SET descricao={descricao} WHERE id=1");
        }

        /// <summary>
        /// Exemplo de problemas com SQL Injection
        /// </summary>
        static void ExemploDeProblemaComSQLInjection()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departamentos.AddRange(
                new Departamento()
                {
                    Descricao = "Departamento 01"
                },
                new Departamento()
                {
                    Descricao = "Departamento 02"
                });
            db.SaveChanges();

            var descricao = "Departamento 01";
            //db.Database.ExecuteSqlRaw("UPDATE \"Departamentos\" SET \"Descricao\"='DepartamentoAlterado' WHERE \"Descricao\"={0}", descricao); // Com DBParameter com protecao para SQL Injection
            
            // Ao concatenar o SQL a query fica vulneravel, podendo colocar condicoes a mais que nao deveriam existir
            // compremetendo a base de dados
            descricao = "'teste' or 1=1";
            db.Database.ExecuteSqlRaw($"UPDATE \"Departamentos\" SET \"Descricao\"='AtaqueSQLInjection' WHERE \"Descricao\"={descricao}"); // Sem DBParameter, vuneravel para SQLInjection

            foreach(var departamento in db.Departamentos.AsNoTracking())
            {
                Console.WriteLine($"Id: {departamento.Id}, Descricao: {departamento.Descricao}");
            }
        }

        /// <summary>
        /// Verificar migracoes que estão pendentes
        /// </summary>
        static void MigracoesPendentes()
        {
            using var db = new ApplicationContext();

            var migracoesPendentes = db.Database.GetPendingMigrations();

            Console.WriteLine($"Total: {migracoesPendentes.Count()}");

            foreach(var migracao in migracoesPendentes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }

        /// <summary>
        /// Aplicando migracao em tempo de execução
        /// </summary>
        static void AplicarMigracaoEmTempoDeExecucao()
        {
            using var db = new ApplicationContext();
            db.Database.Migrate();
        }

        /// <summary>
        /// Lista todas as migracoes existentes no projeto
        /// </summary>
        static void ListandoTodasAsMigracoes()
        {
            using var db = new ApplicationContext();
            var migracoes = db.Database.GetMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach(var migracao in migracoes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }

        /// <summary>
        /// Lista todas as migracoes que já foram aplicadas no banco de dados
        /// </summary>
        static void MigracoesJaAplicadas()
        {
            using var db = new ApplicationContext();
            var migracoes = db.Database.GetAppliedMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migracao in migracoes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }

        /// <summary>
        /// Gerar script da criação de todas as tabelas do banco de dados
        /// </summary>
        static void ScriptGeralDoBancoDeDados()
        {
            using var db = new ApplicationContext();
            var script = db.Database.GenerateCreateScript();

            Console.WriteLine(script);
        }
        #endregion

        #region Tipos de Carregamento
        /// <summary>
        /// Exemplo de uso do carregamento adiantado
        /// </summary>
        static void CarregamentoAdiantado()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db
                .Departamentos
                .Include(i => i.Funcionarios);

            foreach(var departamento in departamentos)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if(departamento.Funcionarios?.Any() ?? false)
                {
                    foreach(var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum Funcionário encontrado!");
                }
            }
        }

        /// <summary>
        /// Exemplo de uso do carregamento explicito
        /// </summary>
        static void CarregamentoExplicito()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db
                .Departamentos
                .ToList();

            foreach (var departamento in departamentos)
            {
                if(departamento.Id == 2)
                {
                   // db.Entry(departamento).Collection(c => c.Funcionarios)
                   //     .Load(); // Carregando explicitamente

                    db.Entry(departamento).Collection(c => c.Funcionarios)
                        .Query()
                        .Where(w => w.Id > 2)
                        .ToList(); // Carregamento explicito com condicao where
                }

                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum Funcionário encontrado!");
                }
            }
        }
        
        /// <summary>
        /// Exemplo de uso do carremento lento, faz a consulta no banco quando acessa uma propriedade
        /// </summary>
        static void CarregamentoLento()
        {
            using var db = new ApplicationContext();
            Setup(db);

            // Para desabilitar o carregamento lento e não realizar consultas a cada propriedade acessada
            //db.ChangeTracker.LazyLoadingEnabled = false; 
            
            var departamentos = db
                .Departamentos
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum Funcionário encontrado!");
                }
            }
        }
        #endregion


        /// <summary>
        /// Exemplo de uso de Filtro Global
        /// </summary>
        static void FiltroGlobal()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos.Where(w => w.Id > 0).ToList();

            foreach(var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluido: {departamento.Excluido}");
            }
        }

        /// <summary>
        /// Exemplo de uso de Filtro Global
        /// </summary>
        static void IgnorandoFiltroGlobal()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .IgnoreQueryFilters() // Ignora filtro global
                .Where(w => w.Id > 0).ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluido: {departamento.Excluido}");
            }
        }

        /// <summary>
        /// Exemplo de consulta projetada, selecionando apenas colunas que são utilizadas
        /// </summary>
        static void ConsultaProjetada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Where(w => w.Id > 0)
                .Select(s => new
                {
                    s.Descricao,
                    Funcionarios = s.Funcionarios.Select(f => f.Nome)
                })
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\tDescrição: {funcionario}");
                }
            }
        }
    }
}
