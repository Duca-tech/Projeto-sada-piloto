using Application.Tarefa;
using Application.Tarefa.Dto;
using Domain.Tarefa;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SADA.Controllers
{
    [Route("api/tarefa")]
    [ApiController]
    public class TarefaController : ControllerBase
    {
        readonly TarefaCasoUso _tarefaCasoUso;
        public TarefaController(TarefaCasoUso tarefaCasoUso)
        {
            _tarefaCasoUso = tarefaCasoUso;
        }

        [Route("incluir")]
        [HttpPost]
        public ActionResult<TarefaResponse> Incluir([FromBody] TarefaRequest req)
        {
            var resp = _tarefaCasoUso.CriarTarefa(req);
            return StatusCode(resp.status, resp);
        }

        [Route("alterar")]
        [HttpPut]
        public ActionResult<TarefaResponse> Alterar([FromBody] TarefaRequest req)
        {
            var resp = _tarefaCasoUso.AtualizarTarefa(req);
            return StatusCode(resp.status, resp);
        }

       
        [Route("listar")]
        [HttpGet]
        public ActionResult<TarefaListResponse> Listar([FromQuery] string? status,[FromQuery] string? dataVencimento,[FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10)
        {
            var resp = _tarefaCasoUso.ListarTarefas(new TarefaListRequest(status, dataVencimento), pagina, tamanho);
            return StatusCode(resp.status, resp);
        }
    }
}
