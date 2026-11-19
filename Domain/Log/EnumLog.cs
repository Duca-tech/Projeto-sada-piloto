using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Log
{
    public static class EnumLog
    {

        public enum StatusLog
        {
            Cadastro = 3,
            Atualizacao = 4,
            Delete = 5
        }
        public enum TipoLog
        {
            Tarefa = 1,
            Usuario = 2
        }
    }
}
