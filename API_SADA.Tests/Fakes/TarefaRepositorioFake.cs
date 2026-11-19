using Application.Tarefa;
using Application.Tarefa.Dto;
using Domain.Tarefa;

namespace API_SADA.Tests.Fakes
{
    /// <summary>
    /// Repositório em memória usado para testar o <see cref="TarefaCasoUso"/> isolado do banco.
    /// Guarda as tarefas numa lista e registra os parâmetros recebidos na listagem.
    /// </summary>
    public class TarefaRepositorioFake : ITarefaRespositorio
    {
        public List<TarefaInfo> Tarefas { get; } = new();

        /// <summary>Quando true, as operações de escrita lançam exceção (simula falha de banco).</summary>
        public bool SimularFalha { get; set; }

        public EnumTarefa.status? UltimoStatusFiltrado { get; private set; }
        public DateTime? UltimaDataFiltrada { get; private set; }
        public bool ListarFoiChamado { get; private set; }

        public string IncluirTarefa(TarefaInfo tarefa)
        {
            if (SimularFalha)
                throw new InvalidOperationException("Falha simulada no banco");

            tarefa.Id = Guid.NewGuid().ToString();
            Tarefas.Add(tarefa);
            return tarefa.Id;
        }

        public TarefaInfo PesquisarPorId(string id) => Tarefas.FirstOrDefault(t => t.Id == id)!;

        public void AtualizarTarefa(TarefaInfo tarefa)
        {
            if (SimularFalha)
                throw new InvalidOperationException("Falha simulada no banco");
        }

        public void ExcluirTarefa(TarefaInfo tarefa)
        {
            if (SimularFalha)
                throw new InvalidOperationException("Falha simulada no banco");

            Tarefas.Remove(tarefa);
        }

        public TarefaListResponse ListarTarefas(EnumTarefa.status? status, DateTime? dataVencimento, int pagina, int tamanho)
        {
            ListarFoiChamado = true;
            UltimoStatusFiltrado = status;
            UltimaDataFiltrada = dataVencimento;

            var filtradas = Tarefas
                .Where(t => status == null || t.Status == status)
                .Where(t => dataVencimento == null || t.DataVencimento.Date == dataVencimento.Value.Date)
                .ToList();

            return new TarefaListResponse(
                total: filtradas.Count,
                dados: filtradas.Skip((pagina - 1) * tamanho).Take(tamanho).ToList(),
                pagina: pagina,
                totalPaginas: (int)Math.Ceiling((double)filtradas.Count / tamanho),
                status: 200);
        }

        /// <summary>Adiciona uma tarefa diretamente na lista (preparação dos testes).</summary>
        public TarefaInfo Semear(string titulo = "Tarefa existente", EnumTarefa.status status = EnumTarefa.status.Pendente, DateTime? vencimento = null)
        {
            var tarefa = new TarefaInfo
            {
                Id = Guid.NewGuid().ToString(),
                Titulo = titulo,
                Descricao = "Descrição original",
                DataVencimento = vencimento ?? new DateTime(2026, 12, 31),
                Status = status
            };
            Tarefas.Add(tarefa);
            return tarefa;
        }
    }
}
