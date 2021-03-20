using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace DominandoEFCore.Conversores
{
    public class ConversorCustomizado : ValueConverter<Status, string>
    {
        public ConversorCustomizado(): base(status => ConverterParaOBancoDeDados(status), value => ConverterParaAplicacao(value), new ConverterMappingHints(1))
        {

        }

        static string ConverterParaOBancoDeDados(Status status)
        {
            return status.ToString()[0..1];
        }

        static Status ConverterParaAplicacao(string value)
        {
            var status = Enum
                .GetValues<Status>()
                .FirstOrDefault(p => p.ToString()[0..1] == value);

            return status;
        }
    }
}
