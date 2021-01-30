using DominandoEFCore.Data;

namespace DominandoEFCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            EnsureDeleted();
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
    }
}
