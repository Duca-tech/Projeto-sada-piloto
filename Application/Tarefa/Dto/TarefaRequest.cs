using System.ComponentModel.DataAnnotations;

namespace Application.Tarefa.Dto
{
    public record TarefaRequest(
        string Id,

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, MinimumLength = 3)]
        string Titulo,
        
        string Descricao,
        string DataVencimento,

         
        string Status
        );
}
