using Application.Tarefa.Dto;
using Domain.Tarefa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace Application.Tarefa
{
    public interface ITarefaRespositorio
    {
        string IncluirTarefa(TarefaInfo request);

        TarefaInfo PesquisarPorId(string id);

        void AtualizarTarefa(TarefaInfo request);

        void ExcluirTarefa(TarefaInfo tarefa);

        TarefaListResponse ListarTarefas(EnumTarefa.status? status, DateTime? dataVencimento, int pagina, int tamanho);
    }
}
