using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Log
{
    public class LogInfo
    {
        public string Id { get; set; }
        public string Mensagem { get; set; }
        public int Status { get; set; }
        public int Tipo { get; set; }
        public string CodigoExterno { get; set; }
        
        public string Erro { get; set; }
    }
}
