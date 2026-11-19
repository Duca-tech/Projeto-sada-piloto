using Application.Tarefa;
using Application.Tarefa.Dto;
using Domain.Tarefa;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SADA.Controllers
{
    [Route("api/tarefa")]
    [ApiController]
    [Produces("application/json")]
    public class TarefaController : ControllerBase
    {
        readonly TarefaCasoUso _tarefaCasoUso;
        public TarefaController(TarefaCasoUso tarefaCasoUso)
        {
            _tarefaCasoUso = tarefaCasoUso;
        }

        /// <summary>
        /// Cadastra uma nova tarefa e retorna o código único gerado.
        /// </summary>
        /// <response code="201">Tarefa criada. O header Location aponta para a nova tarefa.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="409">Já existe uma tarefa com o Id informado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult<TarefaResponse> Incluir([FromBody] TarefaRequest req)
        {
            var resp = _tarefaCasoUso.CriarTarefa(req);

            if (resp.status == StatusCodes.Status201Created)
                return CreatedAtAction(nameof(Buscar), new { id = resp.Id }, resp);

            return StatusCode(resp.status, resp);
        }

        /// <summary>
        /// Busca uma tarefa pelo código.
        /// </summary>
        /// <param name="id">Código (GUID) da tarefa.</param>
        /// <response code="200">Tarefa encontrada.</response>
        /// <response code="400">Código inválido.</response>
        /// <response code="404">Tarefa não encontrada.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TarefaBuscaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TarefaBuscaResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TarefaBuscaResponse), StatusCodes.Status404NotFound)]
        public ActionResult<TarefaBuscaResponse> Buscar(string id)
        {
            var resp = _tarefaCasoUso.BuscarTarefa(id);
            return StatusCode(resp.status, resp);
        }

        /// <summary>
        /// Edita uma tarefa existente (título, descrição, data de vencimento e status).
        /// </summary>
        /// <param name="id">Código (GUID) da tarefa.</param>
        /// <param name="req">Novos dados da tarefa. O campo Id do corpo é ignorado; vale o da rota.</param>
        /// <response code="200">Tarefa atualizada.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Tarefa não encontrada.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult<TarefaResponse> Alterar(string id, [FromBody] TarefaRequest req)
        {
            var resp = _tarefaCasoUso.AtualizarTarefa(id, req);
            return StatusCode(resp.status, resp);
        }

        /// <summary>
        /// Exclui uma tarefa.
        /// </summary>
        /// <param name="id">Código (GUID) da tarefa.</param>
        /// <response code="204">Tarefa excluída.</response>
        /// <response code="400">Código inválido.</response>
        /// <response code="404">Tarefa não encontrada.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status500InternalServerError)]
        public IActionResult Excluir(string id)
        {
            var resp = _tarefaCasoUso.ExcluirTarefa(id);

            if (resp.status == StatusCodes.Status204NoContent)
                return NoContent();

            return StatusCode(resp.status, resp);
        }

        /// <summary>
        /// Lista as tarefas, com filtros opcionais por status e/ou data de vencimento.
        /// </summary>
        /// <param name="status">Pendente, EmProgresso ou Concluida (opcional).</param>
        /// <param name="dataVencimento">Data no formato dd/MM/yyyy (opcional).</param>
        /// <param name="pagina">Número da página, a partir de 1.</param>
        /// <param name="tamanho">Itens por página, de 1 a 100.</param>
        /// <response code="200">Lista de tarefas.</response>
        /// <response code="400">Filtro ou paginação inválidos.</response>
        [HttpGet]
        [ProducesResponseType(typeof(TarefaListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TarefaListResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TarefaListResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult<TarefaListResponse> Listar([FromQuery] string? status,[FromQuery] string? dataVencimento,[FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10)
        {
            var resp = _tarefaCasoUso.ListarTarefas(new TarefaListRequest(status, dataVencimento), pagina, tamanho);
            return StatusCode(resp.status, resp);
        }
    }
}
