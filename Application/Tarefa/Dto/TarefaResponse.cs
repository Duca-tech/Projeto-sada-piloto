using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tarefa.Dto
{
    public record TarefaResponse(
            string Id,
            string Mensagem,
            int status,
            string? Erro
        );
}
