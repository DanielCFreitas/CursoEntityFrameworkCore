﻿using DominandoEFCore.Data;
using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace DominandoEFCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            FuncaoCollate();
            Console.ReadLine();
        }

        #region Metodos de apoio para o curso
        /// <summary>
        /// Setup apenas para popular o banco de dados para arealizar alguns exemplos
        /// </summary>
        /// <param name="db"></param>
        static void Setup(ApplicationContext db)
        {
            db.Database.EnsureDeleted();

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

        #region Consultas
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

        /// <summary>
        /// Exemplo de consulta parametrizada
        /// </summary>
        static void ConsultaParametrizada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var id = 0;
            var departamentos = db.Departamentos
                .FromSqlRaw("SELECT * FROM \"Departamentos\" WHERE \"Id\" > {0}", id) // Consulta parametrizada
                .Where(w => !w.Excluido)
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        /// <summary>
        /// Exemplo de consulta interpolada com FromSql
        /// </summary>
        static void ConsultaInterpolada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var id = 0;
            var departamentos = db.Departamentos
                .FromSqlInterpolated($"SELECT * FROM \"Departamentos\" WHERE \"Id\" > {id}") // Consulta Interpolada
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        /// <summary>
        /// Exemplo de consulta com Tag
        /// </summary>
        static void ConsultaComTag()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .TagWith("Estou enviando um comentário para o servidor") // Adicionando Tag para consulta, enviar tag para servidor
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        /// <summary>
        /// Consulta com relação de 1:N
        /// </summary>
        static void EntendendoConsulta1N()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Include(i => i.Funcionarios)
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\tNome: {funcionario.Nome}");
                }
            }
        }
        
        /// <summary>
        /// Consulta com relação de N:1
        /// </summary>
        static void EntendendoConsultaN1()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var funcionarios = db.Funcionarios
                .Include(i => i.Departamento)
                .ToList();

            foreach (var funcionario in funcionarios)
            {
                Console.WriteLine($"Nome: {funcionario.Nome} / Departamento: {funcionario.Departamento.Descricao}");
            }
        }

        /// <summary>
        /// Dividir a consulta para resolver problema de explosão cartesiana 
        /// carregando dados duplicados durante a consulta
        /// </summary>
        static void DivisaoDeConsulta()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Include(i => i.Funcionarios)
                .Where((w => w.Id < 3))
                .AsSplitQuery() // Método que utiliza a divisão de consulta
                //.AsSingleQuery() // Ignora configuração global (caso exista) do split query
                .ToList();

            foreach(var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
                foreach(var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\t{funcionario.Nome}");
                }
            }
        }
        #endregion

        #region StoredProcedures e Functions
        /// <summary>
        /// Criando StoredProcedure no Banco de Dados
        /// </summary>
        static void CriartStoredProcedure()
        {
            var criarDepartamentoProcedure = "CREATE OR REPLACE PROCEDURE CriarDepartamento ( Descricao TEXT, Ativo BOOL ) LANGUAGE SQL AS " +
            "$$ INSERT INTO \"Departamentos\" ( \"Descricao\", \"Ativo\", \"Excluido\" ) VALUES (Descricao, Ativo, FALSE) $$; ";

            using var db = new ApplicationContext();
            db.Database.ExecuteSqlRaw(criarDepartamentoProcedure);
        }
        
        /// <summary>
        /// Inserção de dados utilizando StoredProcedure
        /// </summary>
        static void InserirDadosViaProcedure()
        {
            using var db = new ApplicationContext();
            db.Database.ExecuteSqlRaw("CALL CriarDepartamento(@p0, @p1)", "Departamento Via Procedure", true);
        }

        /// <summary>
        /// Criando Funcion (Procedure) de consulta no banco de dados
        /// </summary>
        static void CriartStoredProcedureDeConsulta()
        {
            var criarDepartamentoProcedure = "CREATE OR REPLACE FUNCTION GetDepartamentos(Descricao TEXT) " +
                                             "RETURNS TABLE( \"Id\" int4, \"Descricao\" TEXT, \"Ativo\" BOOL, \"Excluido\" BOOL ) " +
                                             "AS $$ " +
                                             "SELECT * FROM \"Departamentos\" WHERE \"Descricao\" LIKE CONCAT(Descricao, '%' ); " +
                                             "$$ LANGUAGE SQL;";

            using var db = new ApplicationContext();
            db.Database.ExecuteSqlRaw(criarDepartamentoProcedure);
        }

        /// <summary>
        /// Consultando dados via function (procedure) no banco de dados
        /// </summary>
        static void ConsultarDadosViaProcedure()
        {
            using var db = new ApplicationContext();
            var departamentos = db.Departamentos
                .FromSqlRaw("SELECT * FROM GetDepartamentos(@p0)", "Departamento 0")
                .ToList();

            foreach(var departamento in departamentos)
            {
                Console.WriteLine(departamento.Descricao);
            }
        }
        #endregion

        #region Infraestrutura
        /// <summary>
        /// Seta um timeout para um comando especifo, sem usar a configuracao global do timeout,
        /// Ao utilizar resiliencia para fazer requisicoes em caso de falhas, pode ocorrer o caso de o banco conseguir
        /// salvar os dados mas na hora de retornar dar algum problema, e tentar fazer a requisicao para o banco novamente
        /// duplicando dados ja existentes, para evitar este caso usamos o "strategy", que gerencia a transacao e so executa
        /// apenas uma vez o comando SQL no banco de dados
        /// </summary>
        static void UsandoTimeOutParaUmComandoEspecifico()
        {
            using var db = new ApplicationContext();
            db.Database.SetCommandTimeout(15);

            db.Database.ExecuteSqlRaw("SELECT pg_sleep(5); SELECT 1;");
        }

        static void TesteParaExecutarEstrategiaDeResiliencia()
        {
            using var db = new ApplicationContext();

            var strategy = db.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = db.Database.BeginTransaction();

                db.Departamentos.Add(new Departamento() { Descricao = "Departamento Transacao" });
                db.SaveChanges();

                transaction.Commit();
            });
        }

        static void TesteLogSimplificado()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var departamentos = db.Departamentos.Where(w => w.Id > 0).ToArray();
            var teste = "";
        }

        static void TesteBatchSize()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            for(var i = 0; i < 50; i++)
            {
                db.Departamentos.Add(new Departamento() {  Descricao = $"Departamento {i}" });
            }

            db.SaveChanges();
        }

        static void TesteTimeOut()
        {
            using var db = new ApplicationContext();
            db.Database.ExecuteSqlRaw("SELECT pg_sleep(10); SELECT 1;");
        }

        #endregion

        #region Modelo de Dados
        /// <summary>
        /// Testando o uso de Collactions configuradko no OnModelCreating do ApplicationContext
        /// </summary>
        static void TesteCollactions()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

        }

        static void TestePropagarDados()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var script = db.Database.GenerateCreateScript();
            Console.WriteLine(script);
        }

        static void TesteEsquemas()
        {
            using var db = new ApplicationContext();

            var script = db.Database.GenerateCreateScript();

            Console.WriteLine(script);
        }

        static void TesteConversorDeValor() => TesteEsquemas();

        static void TesteConversorCustomizado()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Conversores.Add(
                new Conversor
                {
                    Status = Status.Devolvido
                });

            db.SaveChanges();

            var conversorEmAnalise = db.Conversores.AsNoTracking().FirstOrDefault(p => p.Status == Status.Analise);

            var conversorDevolvido = db.Conversores.AsNoTracking().FirstOrDefault(p => p.Status == Status.Devolvido);
        }

        static void TestePropriedadesDeSombra()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        static void TesteProprieadesDeSombraCriadaPeloDesenvolvedor()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var departamento = new Departamento()
            {
                Descricao = "Departamento Propriedade de Sombra"
            };

            db.Departamentos.Add(departamento);

            // INSERINDO DADO DE PROPRIEDADE DE SOMBRA
            db.Entry(departamento).Property("UltimaAtualizacao").CurrentValue = DateTime.Now;

            db.SaveChanges();

            // CONSULTANDO PROPRIEDADE E SOMBRA
            var departamentos = db.Departamentos
                .Where(w => EF.Property<DateTime>(w, "UltimaAtualizacao") < DateTime.Now)
                .ToArray();
        }

        static void TesteOwnedTypes()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var cliente = new Cliente()
            {
                Nome = "Fulano de Tal",
                Telefone = "(12) 3888-5847",
                Endereco = new Endereco()
                {
                    Bairro = "Bairro Teste",
                    Cidade = "Cidade Teste",
                    Estado = "Estado Teste",
                    Logradouro = "Logradouro Teste"
                }
            };

            db.Clientes.Add(cliente);

            db.SaveChanges();

            var clientes = db.Clientes.AsNoTracking().ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };

            clientes.ForEach(cliente =>
            {
                var json = JsonSerializer.Serialize(cliente, options);
                Console.WriteLine(json);
            });
        }

        static void TesteRelacionamentoUmParaUm()
        {
            using var db = new ApplicationContext();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var estado = new Estado()
            {
                Nome = "Sergipe",
                Governador = new Governador() { Nome = "Rafael Almeida" }
            };

            db.Estados.Add(estado);

            db.SaveChanges();

            var estados = db.Estados.AsNoTracking().ToList();

            estados.ForEach(estado =>
            {
                Console.WriteLine($"Estado: { estado.Nome } é governado por { estado.Governador.Nome }");
            });
        }

        static void TesteRelacionamentoUmParaMuitos()
        {
            using (var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var estado = new Estado()
                {
                    Nome = "Sergipe",
                    Governador = new Governador() { Nome = "Rafael Almeida" }
                };

                estado.Cidades.Add(new Cidade() { Nome = "Itabaiana" });

                db.Estados.Add(estado);

                db.SaveChanges();
            }

            using (var db = new ApplicationContext())
            {
                var estados = db.Estados.ToList();

                estados[0].Cidades.Add(new Cidade() { Nome = "Aracaju" });

                db.SaveChanges();

                foreach(var estado in db.Estados.Include(i => i.Cidades))
                {
                    Console.WriteLine($"Estado: { estado.Nome } / Governador: { estado.Governador.Nome }");

                    foreach(var cidade in estado.Cidades)
                    {
                        Console.WriteLine($"\t Cidade: { cidade.Nome }");
                    }
                }
            }
        }

        static void TesteRelacionamentoMuitosParaMuitos()
        {
            using(var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var ator1 = new Ator() { Nome = "Rafael" };
                var ator2 = new Ator() { Nome = "Pires" };
                var ator3 = new Ator() { Nome = "Bruno" };

                var filme1 = new Filme() { Nome = "A volta dos que não foram" };
                var filme2 = new Filme() { Nome = "De volta para o futuro" };
                var filme3 = new Filme() { Nome = "Poeira em alto mar" };

                ator1.Filmes.Add(filme1);
                ator1.Filmes.Add(filme2);

                ator2.Filmes.Add(filme1);

                filme3.Atores.Add(ator1);
                filme3.Atores.Add(ator2);
                filme3.Atores.Add(ator3);

                db.AddRange(ator1, ator2, filme3);

                db.SaveChanges();

                foreach(var ator in db.Atores.Include(i => i.Filmes).ToList())
                {
                    Console.WriteLine($"Ator: { ator.Nome }");
                    
                    foreach(var filme in ator.Filmes)
                    {
                        Console.WriteLine($"\t Filme: { filme.Nome }");
                    }
                }
            }
        }

        static void TesteCampoDeApoio()
        {
            using (var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var documento = new Documento();
                documento.SetCPF("12345678933");

                db.Documentos.Add(documento);
                db.SaveChanges();

                foreach(var doc in db.Documentos.AsNoTracking())
                {
                    Console.WriteLine($"CPF -> { doc.GetCPF() }");
                }
            }
        }

        static void ExemploDeTabelaPorHeranca()
        {
            using (var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var pessoa = new Pessoa { Nome = "Fulano de Tal" };
                var instrutor = new Instrutor { Nome = "Rafael Almeida", Tecnologia = ".NET", Desde = DateTime.Now };
                var aluno = new Aluno { Nome = "Maria Thysbe", Idade = 31, DataContrato = DateTime.Now.AddDays(-1) };

                db.AddRange(pessoa, instrutor, aluno);
                db.SaveChanges();

                var pessoas = db.Pessoas.AsNoTracking().ToArray();
                var instrutores = db.Instrutores.AsNoTracking().ToArray();
                // var alunos = db.Alunos.AsNoTracking().ToArray();
                var alunos = db.Pessoas.OfType<Aluno>().AsNoTracking().ToArray(); // Sem o uso de DbSet atraves do OffType

                Console.WriteLine("----- Pessoas -----");
                foreach(var pessoaItem in pessoas)
                {
                    Console.WriteLine($"{pessoaItem.Id} -> {pessoaItem.Nome}");
                }

                Console.WriteLine("----- Instrutores -----");
                foreach(var instrutorItem in instrutores)
                {
                    Console.WriteLine($"Id: {instrutorItem.Id} -> {instrutorItem.Nome}, Tecnologia: {instrutorItem.Tecnologia}, Desde: {instrutorItem.Desde}");
                }

                Console.WriteLine("----- Alunos -----");
                foreach(var alunoItem in alunos)
                {
                    Console.WriteLine($"Id: {alunoItem.Id} -> {alunoItem.Nome}, Idade: {alunoItem.Idade}, Data do Contrato: {alunoItem.DataContrato}");
                }
            }
        }

        static void ExemploPacoteDePropriedades()
        {
            using(var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var configuracao = new Dictionary<string, object>
                {
                    ["Chave"] = "SenhaBancoDeDados",
                    ["Valor"] = Guid.NewGuid().ToString()
                };

                db.Configuracoes.Add(configuracao);
                db.SaveChanges();

                var configuracoes = db
                    .Configuracoes
                    .AsNoTracking()
                    .Where(w => w["Chave"] == "SenhaBancoDeDados")
                    .ToArray();

                foreach(var dic in configuracoes)
                {
                    Console.WriteLine($"Chave: {dic["Chave"]} - Valor: {dic["Valor"]}");
                }
            }
        }
        #endregion

        #region Annotations
        static void Atributos()
        {
            using(var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var script = db.Database.GenerateCreateScript();

                Console.WriteLine(script);

                db.Atributos.Add(new Atributo()
                {
                    Descricao = "Exemplo",
                    Observacao = "Observacao"
                });

                db.SaveChanges();
            }
        }
        #endregion


        #region EF Functions

        /// <summary>
        /// Criando o banco de dados e preenchendo os dados da tabela Funcoes
        /// </summary>
        static void ApagarCriarBancoDeDados()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Funcoes.AddRange(
                new Funcao()
                {
                    Data1 = DateTime.Now.AddDays(2),
                    Data2 = "2021-01-01",
                    Descricao1 = "Bala 1",
                    Descricao2 = "Bala 1"
                },
                new Funcao()
                {
                    Data1 = DateTime.Now.AddDays(1),
                    Data2 = "XX21-01-01",
                    Descricao1 = "Bola 1",
                    Descricao2 = "Bola 1"
                },
                new Funcao()
                {
                    Data1 = DateTime.Now.AddDays(1),
                    Data2 = "XX21-01-01",
                    Descricao1 = "Tela",
                    Descricao2 = "Tela"
                }
            );

            db.SaveChanges();
        }

        /// <summary>
        /// Exemplo de funcoes que envolvem datas
        /// OS EXEMPLOS MOSTRADOS NESTA AULA, O POSTGRES NAO POSSUI METODOS PARA ELES
        /// </summary>
        static void FuncoesDeDatas()
        {
            ApagarCriarBancoDeDados();

            using(var db = new ApplicationContext())
            {
                var script = db.Database.GenerateCreateScript();

                Console.WriteLine(script);

                var dados = db.Funcoes.AsNoTracking().Select(s => new
                {
                    //Dias = EF.Functions.DateDiffDay(DateTime.Now, s.Data1),
                    //Data = EF.Functions.DateFromParts(2021, 1, 2),                    AS 3 FUNCOES SO VAO FUNCIONAR COM O SQLSERVER
                    //DataValida = EF.Functions.IsDate(s.Data2)
                });

                foreach(var dado in dados)
                {
                    Console.WriteLine(dado);
                }
            }
        }

        /// <summary>
        /// Exemplo de uso da funcao LIKE
        /// </summary>
        static void FuncaoLike()
        {
            ApagarCriarBancoDeDados();
            using (var db = new ApplicationContext())
            {
                var script = db.Database.GenerateCreateScript();

                Console.WriteLine(script);

                var dados = db.Funcoes
                    .AsNoTracking()
                    .Where(w => EF.Functions.Like(w.Descricao1, "%Bo%"))
                    .Select(s => s.Descricao1)
                    .ToArray();

                Console.WriteLine("Resultado: ");
                foreach(var descricao in dados)
                {
                    Console.WriteLine(descricao);
                }
            }
        }

        /// <summary>
        /// Verificar o tamanho de dados em bytes que foi alocado no banco para um campo especifico
        /// </summary>
        static void FuncaoDataLength()
        {
            using(var db = new ApplicationContext())
            {
                var resultado = db
                    .Funcoes
                    .AsNoTracking()
                    .Select(s => new
                    {
                        //TotalBytesCampoData = EF.Functions.DataLength(s.Data1),
                        //TotalBytes = EF.Functions.DataLength(s.Data1),                    FUNCAO DISPONIVEL APENAS PARA O SQL SERVER
                        //TotalBytes2 = EF.Functions.DataLength(s.Data1),
                        Total1 = s.Descricao1.Length,
                        Total2 = s.Descricao2.Length,
                    })
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Uso de Collactions usando o EF Functions (Deve-se criar as collactions antes no banco de dados para serem usadas no C#)
        /// </summary>
        static void FuncaoCollate()
        {
            using(var db = new ApplicationContext())
            {
                var consulta1 = db
                    .Funcoes
                    .FirstOrDefault(f => EF.Functions.Collate(f.Descricao1, "SQL_Latin1_General_CP1_CS_AS") == "tela");
                
                var consulta2 = db
                    .Funcoes
                    .FirstOrDefault(f => EF.Functions.Collate(f.Descricao1, "SQL_Latin1_General_CP1_CI_AS") == "tela");

                Console.WriteLine($"Consulta1: {consulta1?.Descricao1}");
                Console.WriteLine($"Consulta2: {consulta2?.Descricao1}");
            }
        }


        #endregion
    }
}
