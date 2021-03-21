using System;

namespace DominandoEFCore.Domain
{
    public class Documento
    {
        private string _cpf;
        public int Id { get; set; }

        public void SetCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                throw new Exception("CPF Invalido");
            }

            _cpf = cpf;
        }

        public string GetCPF() => _cpf;
    }
}
