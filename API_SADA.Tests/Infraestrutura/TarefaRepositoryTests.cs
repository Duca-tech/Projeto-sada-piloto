using Domain.Log;
using Domain.Tarefa;
using Infraestructure.TarefaInfra;
using Microsoft.EntityFrameworkCore;
using AppDbContext = Infraestructure.DbContext;

namespace API_SADA.Tests.Infraestrutura
{
    /// <summary>
    /// Testes do <see cref="TarefaRepository"/> contra o EF Core InMemory.
    /// Cada teste usa um banco com nome único, então um não interfere no outro.
    /// </summary>
    public class TarefaRepositoryTests : IDisposable
    {
        readonly AppDbContext _db;
        readonly TarefaRepository _repositorio;

        public TarefaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TarefasTeste-{Guid.NewGuid()}")
                .Options;

            _db = new AppDbContext(options);
            _repositorio = new TarefaRepository(_db);
        }

        public void Dispose() => _db.Dispose();

        static TarefaInfo NovaTarefa(string titulo = "Tarefa", EnumTarefa.status status = EnumTarefa.status.Pendente, DateTime? vencimento = null) => new()
        {
            Titulo = titulo,
            DataVencimento = vencimento ?? new DateTime(2026, 12, 31),
            Status = status
        };

        [Fact]
        public void IncluirTarefa_GeraIdUnicoEPersiste()
        {
            var id = _repositorio.IncluirTarefa(NovaTarefa("Nova"));

            Assert.True(Guid.TryParse(id, out _));
            Assert.Equal("Nova", _db.Tarefas.Single().Titulo);
        }

        [Fact]
        public void IncluirTarefa_GravaLogDeCadastro()
        {
            var id = _repositorio.IncluirTarefa(NovaTarefa());

            var log = Assert.Single(_db.Log);
            Assert.Equal(id, log.CodigoExterno);
            Assert.Equal((int)EnumLog.StatusLog.Cadastro, log.Status);
            Assert.Equal((int)EnumLog.TipoLog.Tarefa, log.Tipo);
        }

        [Fact]
        public void PesquisarPorId_RetornaATarefaOuNulo()
        {
            var id = _repositorio.IncluirTarefa(NovaTarefa("Procurada"));

            Assert.Equal("Procurada", _repositorio.PesquisarPorId(id).Titulo);
            Assert.Null(_repositorio.PesquisarPorId(Guid.NewGuid().ToString()));
        }

        [Fact]
        public void AtualizarTarefa_SalvaAlteracoesEGravaLog()
        {
            var id = _repositorio.IncluirTarefa(NovaTarefa("Antes"));
            var tarefa = _repositorio.PesquisarPorId(id);

            tarefa.Titulo = "Depois";
            tarefa.Status = EnumTarefa.status.Concluida;
            _repositorio.AtualizarTarefa(tarefa);

            var salva = _db.Tarefas.AsNoTracking().Single();
            Assert.Equal("Depois", salva.Titulo);
            Assert.Equal(EnumTarefa.status.Concluida, salva.Status);
            Assert.Contains(_db.Log, l => l.Status == (int)EnumLog.StatusLog.Atualizacao && l.CodigoExterno == id);
        }

        [Fact]
        public void ExcluirTarefa_RemoveEGravaLogDeExclusao()
        {
            var id = _repositorio.IncluirTarefa(NovaTarefa());

            _repositorio.ExcluirTarefa(_repositorio.PesquisarPorId(id));

            Assert.Empty(_db.Tarefas);
            Assert.Contains(_db.Log, l => l.Status == (int)EnumLog.StatusLog.Delete && l.CodigoExterno == id);
        }

        [Fact]
        public void ListarTarefas_SemFiltros_RetornaTodasComTotais()
        {
            for (var i = 1; i <= 3; i++)
                _repositorio.IncluirTarefa(NovaTarefa($"T{i}"));

            var resp = _repositorio.ListarTarefas(null, null, 1, 10);

            Assert.Equal(200, resp.status);
            Assert.Equal(3, resp.total);
            Assert.Equal(3, resp.dados.Count);
            Assert.Equal(1, resp.totalPaginas);
        }

        [Fact]
        public void ListarTarefas_FiltraPorStatus()
        {
            _repositorio.IncluirTarefa(NovaTarefa("Pendente", EnumTarefa.status.Pendente));
            _repositorio.IncluirTarefa(NovaTarefa("Concluída", EnumTarefa.status.Concluida));

            var resp = _repositorio.ListarTarefas(EnumTarefa.status.Concluida, null, 1, 10);

            Assert.Equal(1, resp.total);
            Assert.Equal("Concluída", Assert.Single(resp.dados).Titulo);
        }

        [Fact]
        public void ListarTarefas_FiltraPorDataIgnorandoAHora()
        {
            _repositorio.IncluirTarefa(NovaTarefa("Dia 20", vencimento: new DateTime(2026, 12, 20, 15, 30, 0)));
            _repositorio.IncluirTarefa(NovaTarefa("Dia 21", vencimento: new DateTime(2026, 12, 21)));

            var resp = _repositorio.ListarTarefas(null, new DateTime(2026, 12, 20), 1, 10);

            Assert.Equal("Dia 20", Assert.Single(resp.dados).Titulo);
        }

        [Fact]
        public void ListarTarefas_CombinaStatusEData()
        {
            var dia = new DateTime(2026, 12, 20);
            _repositorio.IncluirTarefa(NovaTarefa("Certa", EnumTarefa.status.EmProgresso, dia));
            _repositorio.IncluirTarefa(NovaTarefa("Status errado", EnumTarefa.status.Pendente, dia));
            _repositorio.IncluirTarefa(NovaTarefa("Data errada", EnumTarefa.status.EmProgresso, dia.AddDays(1)));

            var resp = _repositorio.ListarTarefas(EnumTarefa.status.EmProgresso, dia, 1, 10);

            Assert.Equal(1, resp.total);
            Assert.Equal("Certa", Assert.Single(resp.dados).Titulo);
        }

        [Fact]
        public void ListarTarefas_FiltraAntesDePaginar()
        {
            // 5 pendentes e 5 concluídas intercaladas: a 2ª página de concluídas precisa trazer concluídas.
            for (var i = 1; i <= 5; i++)
            {
                _repositorio.IncluirTarefa(NovaTarefa($"P{i}", EnumTarefa.status.Pendente, new DateTime(2026, 12, i)));
                _repositorio.IncluirTarefa(NovaTarefa($"C{i}", EnumTarefa.status.Concluida, new DateTime(2026, 12, i)));
            }

            var resp = _repositorio.ListarTarefas(EnumTarefa.status.Concluida, null, 2, 2);

            Assert.Equal(5, resp.total);
            Assert.Equal(3, resp.totalPaginas);
            Assert.Equal(new[] { "C3", "C4" }, resp.dados.Select(t => t.Titulo));
        }

        [Fact]
        public void ListarTarefas_OrdenaPelaDataDeVencimento()
        {
            _repositorio.IncluirTarefa(NovaTarefa("Mais tarde", vencimento: new DateTime(2027, 1, 10)));
            _repositorio.IncluirTarefa(NovaTarefa("Mais cedo", vencimento: new DateTime(2026, 12, 1)));

            var resp = _repositorio.ListarTarefas(null, null, 1, 10);

            Assert.Equal(new[] { "Mais cedo", "Mais tarde" }, resp.dados.Select(t => t.Titulo));
        }

        [Fact]
        public void ListarTarefas_PaginaAlemDoFim_RetornaListaVazia()
        {
            _repositorio.IncluirTarefa(NovaTarefa());

            var resp = _repositorio.ListarTarefas(null, null, 5, 10);

            Assert.Equal(1, resp.total);
            Assert.Empty(resp.dados);
        }
    }
}
