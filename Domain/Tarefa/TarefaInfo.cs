using Domain.Tarefa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Tarefa
{
    public class TarefaInfo
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataConclusao { get; set; }
        public DateTime DataVencimento { get; set; }
        public EnumTarefa.status Status { get; set; }
    }
}
