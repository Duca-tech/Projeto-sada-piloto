using Domain.Tarefa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tarefa.Dto
{
    public record TarefaListResponse(
            int total = 0,
            List<TarefaInfo> dados = null,
            int pagina = 0,
            string erro = null,
            int totalPaginas = 0,
            int status = 0
        );
}
