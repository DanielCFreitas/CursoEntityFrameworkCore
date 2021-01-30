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
            /*using var db = new Data.ApplicationContext();
            
            var existe = db.Database.GetPendingMigrations().Any();
            if (existe)
            {

            }*/


            RemoverRegistroApenasComId();
        }



        // Inserir
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
        private static void InserirDadosEmMassa()
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

            // Lista de objetos iguais
            /*var listaDeClientes = new[]
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
            };*/
            //db.Clientes.AddRange(listaDeClientes);

            var registros = db.SaveChanges();

            Console.WriteLine($"Foram afetados {registros} registros");
        }
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



        // Consultar
        private static void ConsultarDados()
        {
            var db = new ApplicationContext();

            
            var consultaPorSintaxe = (from c in db.Clientes where c.Id > 0 select c).ToList();

            var consultaPorMetodo = db.Clientes
                .Where(cliente => cliente.Id > 0)
                .ToList();

            foreach(var cliente in consultaPorMetodo)
            {
                Console.WriteLine($"Consultando Cliente: {cliente.Id}");
                //db.Clientes.Find(cliente.Id);
                db.Clientes.FirstOrDefault(f => f.Id == cliente.Id);
            }
        }
        private static void ConsultarPedidoCarregamentoAdiantado()
        {
            using var db = new ApplicationContext();
            var pedidos = db.Pedidos
                .Include(i => i.Itens)
                .ThenInclude(i => i.Produto)
                .ToList();

            Console.WriteLine(pedidos.Count);
        }



        // Atualizar 
        private static void AtualizarTodosOsDados()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel";
            db.Clientes.Update(cliente);

            db.SaveChanges();
        }
        private static void AtualizarTodosOsDados2()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel2";
            db.Entry(cliente).State = EntityState.Modified;

            db.SaveChanges();
        }


        private static void AtualizarApenasOsCamposAlterados()
        {
            var db = new ApplicationContext();

            var cliente = db.Clientes.Find(1);
            cliente.Nome = "Daniel3";

            db.SaveChanges();
        }
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
    


        // Remover
        private static void RemoverRegistro()
        {
            var db = new ApplicationContext();
            var cliente = db.Clientes.Find(2);
            db.Clientes.Remove(cliente);
            //db.Remove(cliente);
            //db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }


        private static void RemoverRegistroApenasComId()
        {
            var db = new ApplicationContext();
            var cliente = new Cliente { Id = 3 };
            db.Clientes.Remove(cliente);
            //db.Remove(cliente);
            //db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }
    }
}
