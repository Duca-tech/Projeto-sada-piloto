using Domain.Tarefa;

namespace Application.Tarefa.Dto
{
    public record TarefaBuscaResponse(
            TarefaInfo? Dados,
            string? Mensagem,
            int status,
            string? Erro
        );
}
