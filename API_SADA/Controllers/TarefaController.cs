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

        //[Route("/listar")]
        //[HttpGet]
        //public IActionResult ListarTarefas()
        //{
        //}

        [Route("incluir")]
        [HttpPost]
        public ActionResult<TarefaResponse> Incluir([FromBody] TarefaRequest req)
        {
            return _tarefaCasoUso.CriarTarefa(req);
        }

        [Route("alterar")]
        [HttpPut]
        public ActionResult<TarefaResponse> Alterar([FromBody] TarefaRequest req)
        {

            return _tarefaCasoUso.AtualizarTarefa(req);
        }

        [Route("listar")]
        public ActionResult<TarefaListResponse> Listar([FromBody] TarefaListRequest req, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        {
            

            return _tarefaCasoUso.ListarTarefas(req, pagina, tamanho);
        }
    }
}
