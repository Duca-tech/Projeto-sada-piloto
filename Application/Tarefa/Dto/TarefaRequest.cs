using System.ComponentModel.DataAnnotations;

namespace Application.Tarefa.Dto
{
    public record TarefaRequest(
        string? Id,

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, MinimumLength = 3)]
        string Titulo,

        string? Descricao,

        [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
        string DataVencimento,

        [Required(ErrorMessage = "O status é obrigatório.")]
        string Status
        );
}
