using CursoEFCore.Data;
using CursoEFCore.Domain;
using CursoEFCore.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CursoEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoverRegistroApenasComId();
        }

        #region Inserir
        /// <summary>
        /// Inserir registro no banco de dados
        /// </summary>
        private static void InserirDados()
        {
            var produto = new Produto
            {
                Descricao = "Produto Teste",
                CodigoBarras = "1234567891231",
                Valor = 10m,
                TipoProduto = TipoDoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            var db = new ApplicationContext();
            db.Produtos.Add(produto);
            /*db.Set<Produto>().Add(produto);
            db.Entry(produto).State = EntityState.Added;
            db.Add(produto);*/

            var registros = db.SaveChanges();
            Console.WriteLine($"Total de {registros} registros");
        }

        /// <summary>
        /// Inserir vários objetos do mesmo tipo no banco de dados
        /// </summary>
        private static void InserirObjetosDoMesmoTipoEmMassa()
        {
            var db = new ApplicationContext();

            var listaDeClientes = new[]
            {
                new Cliente()
                {
                    Nome = "Teste1",
                    CEP = "99999000",
                    Cidade = "Itabaiana",
                    Estado = "SE",
                    Telefone = "99000001111"
                },
                new Cliente()
                {
                    Nome = "Teste2",
                    CEP = "99999000",
                    Cidade = "Itabaiana",
                    Estado = "SE",
                    Telefone = "99000001111"
                }
            };

            db.Clientes.AddRange(listaDeClientes);

            var registros = db.SaveChanges();

            Console.WriteLine($"Foram afetados {registros} registros");
        }

        /// <summary>
        /// Inserir vários objetos de tipos diferentes no banco de dados
        /// </summary>
        private static void InserirObjetosDeTiposDiferentesEmMassa()
        {
            var db = new ApplicationContext();

            // Objetos diferentes com AddRange
            var produto = new Produto
            {
                Descricao = "Produto Teste 2",
                CodigoBarras = "1234567891231",
                Valor = 10m,
                TipoProduto = TipoDoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            var cliente = new Cliente
            {
                Nome = "Rafael Almeida",
                CEP = "99999000",
                Cidade = "Itabaiana",
                Estado = "SE",
                Telefone = "99000001111"
            };
            db.AddRange(produto, cliente);

            var registros = db.SaveChanges();

            Console.WriteLine($"Foram afetados {registros} registros");
        }
        #endregion

        #region Consultar
        /// <summary>
        /// Consultando dados do banco de dados
        /// </summary>
        private static void ConsultarDados()
        {
            var db = new ApplicationContext();

            // Consulta por Sintaxe
            var consultaPorSintaxe = (from c in db.Clientes where c.Id > 0 select c).ToList();

            // Consulta por metodo
            var consultaPorMetodo = db.Clientes
                .Where(cliente => cliente.Id > 0)
                .ToList();

            foreach (var cliente in consultaPorMetodo)
            {
                Console.WriteLine($"Consultando Cliente: {cliente.Id}");
            }
        }

        /// <summary>
        /// Consultar dados com carregamento adiantado
        /// </summary>
        private static void ConsultarPedidoCarregamentoAdiantado()
        {
            using var db = new ApplicationContext();
            var pedidos = db.Pedidos
                .Include(i => i.Itens)
                .ThenInclude(i => i.Produto)
                .ToList();

            Console.WriteLine(pedidos.Count);
        }
        #endregion

        #region Atualizar
        /// <summary>
        /// Faz Update com todos os campos do objeto (campos alterados ou nao)
        /// </summary>
        private static void AtualizarTodosOsDados()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel";
            db.Clientes.Update(cliente);

            db.SaveChanges();
        }
        /// <summary>
        /// Faz o Update com todos os campos do objeto (campos alterados ou nao)
        /// </summary>
        private static void AtualizarTodosOsDados2()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel2";
            db.Entry(cliente).State = EntityState.Modified;

            db.SaveChanges();
        }
        /// <summary>
        /// Faz o Update apenas com o campo que foi alterado fazendo a consulta antes do objeto no banco
        /// </summary>
        private static void AtualizarApenasOsCamposAlterados()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel3";

            db.SaveChanges();
        }
        /// <summary>
        /// Faz o Update apenas com o campo que foi alterado sem a necessidade de consultar o objeto no banco
        /// </summary>
        private static void AtualizarApenasOsCamposAlterados2()
        {
            var db = new ApplicationContext();

            var cliente = new Cliente()
            {
                Id = 1,
            };

            var clienteDesconectado = new
            {
                Nome = "Daniel4"
            };

            db.Attach(cliente);
            db.Entry(cliente).CurrentValues.SetValues(clienteDesconectado);
            db.SaveChanges();
        }
        #endregion

        #region Remover
        /// <summary>
        /// Remove registro do banco de dados, realizando antes a consulta do objeto no banco de dados
        /// </summary>
        private static void RemoverRegistro()
        {
            var db = new ApplicationContext();
            var cliente = db.Clientes.Find(2);
            db.Clientes.Remove(cliente);
            //db.Remove(cliente);
            //db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }

        /// <summary>
        /// Remove registro do banco de dados, sem a necessidade de consultar o objeto no banco de dados
        /// </summary>
        private static void RemoverRegistroApenasComId()
        {
            var db = new ApplicationContext();
            var cliente = new Cliente { Id = 3 };
            db.Clientes.Remove(cliente);
            //db.Remove(cliente);
            //db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }
        #endregion




        #region MetodosParaFazerOsTestesAnteriores
        /// <summary>
        /// Método para realizar testes de consulta
        /// </summary>
        private static void CadastrarPedido()
        {
            using var db = new ApplicationContext();

            var cliente = db.Clientes.FirstOrDefault();
            var produto = db.Produtos.FirstOrDefault();

            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                IniciadoEm = DateTime.Now,
                Finalizado = DateTime.Now,
                Observacao = "Pedido Teste",
                Status = StatusPedido.Analise,
                TipoFrete = TipoFrete.SemFrete,
                Itens = new List<PedidoItem>
                {
                    new PedidoItem
                    {
                        ProdutoId = produto.Id,
                        Desconto = 0,
                        Quantidade = 1,
                        Valor = 10
                    }
                }
            };

            db.Pedidos.Add(pedido);

            db.SaveChanges();
        }
        #endregion
    }
}
