using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Tarefa;

namespace Domain.Tarefa
{
    public static class EnumTarefa
    {
        public enum  status
        {
            Pendente = 1,
            EmProgresso = 2,
            Concluida = 3
        }

        public static string GetStatusDescription(status status)
        {
            switch (status)
            {
                case status.Pendente:
                    return "Pendente";
                case status.EmProgresso:
                    return "Em Progresso";
                case status.Concluida:
                    return "Concluída";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static EnumTarefa.status GetEnumStatus(string status)
        {
            switch (Rotina.TratarTexto(status))
            {
                case "pendente":
                    return EnumTarefa.status.Pendente;
                case "emprogresso":
                    return EnumTarefa.status.EmProgresso;
                case "concluida":
                    return EnumTarefa.status.Concluida;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

    }
}
