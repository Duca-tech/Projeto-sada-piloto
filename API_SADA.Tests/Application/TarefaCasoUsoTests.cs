using API_SADA.Tests.Fakes;
using Application.Tarefa;
using Application.Tarefa.Dto;
using Domain.Tarefa;
using Microsoft.Extensions.Logging.Abstractions;

namespace API_SADA.Tests.Application
{
    /// <summary>
    /// Testes de unidade do <see cref="TarefaCasoUso"/>, usando um repositório fake em memória.
    /// </summary>
    public class TarefaCasoUsoTests
    {
        const string IdInexistente = "6f9619ff-8b86-d011-b42d-00cf4fc964ff";

        readonly TarefaRepositorioFake _repositorio = new();
        readonly TarefaCasoUso _casoUso;

        public TarefaCasoUsoTests()
        {
            _casoUso = new TarefaCasoUso(
                _repositorio,
                new TarefaService(_repositorio),
                NullLogger<TarefaCasoUso>.Instance);
        }

        static TarefaRequest Requisicao(
            string titulo = "Estudar xUnit",
            string? descricao = "Escrever os testes",
            string dataVencimento = "31/12/2026",
            string status = "Pendente",
            string? id = null) => new(id, titulo, descricao, dataVencimento, status);

        // ---------------------------------------------------------------- Criar

        [Fact]
        public void CriarTarefa_ComDadosValidos_Retorna201EPersisteATarefa()
        {
            var resp = _casoUso.CriarTarefa(Requisicao());

            Assert.Equal(201, resp.status);
            Assert.True(Guid.TryParse(resp.Id, out _));
            var tarefa = Assert.Single(_repositorio.Tarefas);
            Assert.Equal(resp.Id, tarefa.Id);
            Assert.Equal("Estudar xUnit", tarefa.Titulo);
            Assert.Equal("Escrever os testes", tarefa.Descricao);
            Assert.Equal(new DateTime(2026, 12, 31), tarefa.DataVencimento);
            Assert.Equal(EnumTarefa.status.Pendente, tarefa.Status);
        }

        [Fact]
        public void CriarTarefa_DuasVezes_GeraCodigosDiferentes()
        {
            var primeira = _casoUso.CriarTarefa(Requisicao());
            var segunda = _casoUso.CriarTarefa(Requisicao());

            Assert.NotEqual(primeira.Id, segunda.Id);
        }

        [Theory]
        [InlineData("Pendente", EnumTarefa.status.Pendente)]
        [InlineData("EmProgresso", EnumTarefa.status.EmProgresso)]
        [InlineData("Em progresso", EnumTarefa.status.EmProgresso)]
        [InlineData("Concluida", EnumTarefa.status.Concluida)]
        [InlineData("Concluída", EnumTarefa.status.Concluida)]
        [InlineData("CONCLUÍDA", EnumTarefa.status.Concluida)]
        public void CriarTarefa_AceitaVariacoesDoStatus(string statusTexto, EnumTarefa.status esperado)
        {
            var resp = _casoUso.CriarTarefa(Requisicao(status: statusTexto));

            Assert.Equal(201, resp.status);
            Assert.Equal(esperado, Assert.Single(_repositorio.Tarefas).Status);
        }

        [Fact]
        public void CriarTarefa_SemDescricao_Retorna201()
        {
            var resp = _casoUso.CriarTarefa(Requisicao(descricao: null));

            Assert.Equal(201, resp.status);
            Assert.Null(Assert.Single(_repositorio.Tarefas).Descricao);
        }

        [Theory]
        [InlineData("Cancelada")]
        [InlineData("")]
        [InlineData("123")]
        public void CriarTarefa_ComStatusInvalido_Retorna400ENaoPersiste(string status)
        {
            var resp = _casoUso.CriarTarefa(Requisicao(status: status));

            Assert.Equal(400, resp.status);
            Assert.Contains("Status inválido", resp.Mensagem);
            Assert.Empty(_repositorio.Tarefas);
        }

        [Theory]
        [InlineData("2026-12-31")]
        [InlineData("31/13/2026")]
        [InlineData("32/01/2026")]
        [InlineData("amanhã")]
        [InlineData("")]
        public void CriarTarefa_ComDataInvalida_Retorna400ENaoPersiste(string data)
        {
            var resp = _casoUso.CriarTarefa(Requisicao(dataVencimento: data));

            Assert.Equal(400, resp.status);
            Assert.Contains("Data de vencimento inválida", resp.Mensagem);
            Assert.Empty(_repositorio.Tarefas);
        }

        [Fact]
        public void CriarTarefa_ComIdJaExistente_Retorna409()
        {
            var existente = _repositorio.Semear();

            var resp = _casoUso.CriarTarefa(Requisicao(id: existente.Id));

            Assert.Equal(409, resp.status);
            Assert.Single(_repositorio.Tarefas);
        }

        [Fact]
        public void CriarTarefa_QuandoRepositorioFalha_Retorna500ComErro()
        {
            _repositorio.SimularFalha = true;

            var resp = _casoUso.CriarTarefa(Requisicao());

            Assert.Equal(500, resp.status);
            Assert.Equal("Falha simulada no banco", resp.Erro);
        }

        // ---------------------------------------------------------------- Atualizar

        [Fact]
        public void AtualizarTarefa_ComDadosValidos_Retorna200EAlteraOsCampos()
        {
            var tarefa = _repositorio.Semear();

            var resp = _casoUso.AtualizarTarefa(tarefa.Id, Requisicao(
                titulo: "Título novo",
                descricao: "Descrição nova",
                dataVencimento: "15/01/2027",
                status: "Concluída"));

            Assert.Equal(200, resp.status);
            Assert.Equal("Título novo", tarefa.Titulo);
            Assert.Equal("Descrição nova", tarefa.Descricao);
            Assert.Equal(new DateTime(2027, 1, 15), tarefa.DataVencimento);
            Assert.Equal(EnumTarefa.status.Concluida, tarefa.Status);
        }

        [Fact]
        public void AtualizarTarefa_UsaOIdDaRota_IgnorandoOIdDoCorpo()
        {
            var tarefa = _repositorio.Semear();

            var resp = _casoUso.AtualizarTarefa(tarefa.Id, Requisicao(titulo: "Pelo id da rota", id: IdInexistente));

            Assert.Equal(200, resp.status);
            Assert.Equal(tarefa.Id, resp.Id);
            Assert.Equal("Pelo id da rota", tarefa.Titulo);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("")]
        public void AtualizarTarefa_ComIdInvalido_Retorna400(string id)
        {
            var resp = _casoUso.AtualizarTarefa(id, Requisicao());

            Assert.Equal(400, resp.status);
            Assert.Equal("Campo Guid inválido", resp.Mensagem);
        }

        [Fact]
        public void AtualizarTarefa_Inexistente_Retorna404()
        {
            var resp = _casoUso.AtualizarTarefa(IdInexistente, Requisicao());

            Assert.Equal(404, resp.status);
        }

        [Fact]
        public void AtualizarTarefa_ComStatusInvalido_Retorna400ENaoAltera()
        {
            var tarefa = _repositorio.Semear(titulo: "Original");

            var resp = _casoUso.AtualizarTarefa(tarefa.Id, Requisicao(titulo: "Alterado", status: "Arquivada"));

            Assert.Equal(400, resp.status);
            Assert.Equal("Original", tarefa.Titulo);
        }

        [Fact]
        public void AtualizarTarefa_ComDataInvalida_Retorna400ENaoAltera()
        {
            var tarefa = _repositorio.Semear(titulo: "Original");

            var resp = _casoUso.AtualizarTarefa(tarefa.Id, Requisicao(titulo: "Alterado", dataVencimento: "2027-01-15"));

            Assert.Equal(400, resp.status);
            Assert.Equal("Original", tarefa.Titulo);
        }

        [Fact]
        public void AtualizarTarefa_QuandoRepositorioFalha_Retorna500()
        {
            var tarefa = _repositorio.Semear();
            _repositorio.SimularFalha = true;

            var resp = _casoUso.AtualizarTarefa(tarefa.Id, Requisicao());

            Assert.Equal(500, resp.status);
            Assert.NotNull(resp.Erro);
        }

        // ---------------------------------------------------------------- Buscar

        [Fact]
        public void BuscarTarefa_Existente_Retorna200ComOsDados()
        {
            var tarefa = _repositorio.Semear(titulo: "Para buscar");

            var resp = _casoUso.BuscarTarefa(tarefa.Id);

            Assert.Equal(200, resp.status);
            Assert.NotNull(resp.Dados);
            Assert.Equal("Para buscar", resp.Dados!.Titulo);
        }

        [Fact]
        public void BuscarTarefa_Inexistente_Retorna404()
        {
            var resp = _casoUso.BuscarTarefa(IdInexistente);

            Assert.Equal(404, resp.status);
            Assert.Null(resp.Dados);
        }

        [Fact]
        public void BuscarTarefa_ComIdInvalido_Retorna400()
        {
            var resp = _casoUso.BuscarTarefa("nao-e-guid");

            Assert.Equal(400, resp.status);
        }

        // ---------------------------------------------------------------- Excluir

        [Fact]
        public void ExcluirTarefa_Existente_Retorna204ERemove()
        {
            var tarefa = _repositorio.Semear();

            var resp = _casoUso.ExcluirTarefa(tarefa.Id);

            Assert.Equal(204, resp.status);
            Assert.Empty(_repositorio.Tarefas);
        }

        [Fact]
        public void ExcluirTarefa_Inexistente_Retorna404()
        {
            var resp = _casoUso.ExcluirTarefa(IdInexistente);

            Assert.Equal(404, resp.status);
        }

        [Fact]
        public void ExcluirTarefa_ComIdInvalido_Retorna400()
        {
            var resp = _casoUso.ExcluirTarefa("abc");

            Assert.Equal(400, resp.status);
        }

        [Fact]
        public void ExcluirTarefa_QuandoRepositorioFalha_Retorna500ENaoRemove()
        {
            var tarefa = _repositorio.Semear();
            _repositorio.SimularFalha = true;

            var resp = _casoUso.ExcluirTarefa(tarefa.Id);

            Assert.Equal(500, resp.status);
            Assert.Single(_repositorio.Tarefas);
        }

        // ---------------------------------------------------------------- Listar

        [Fact]
        public void ListarTarefas_SemFiltros_RetornaTodas()
        {
            _repositorio.Semear("A");
            _repositorio.Semear("B");

            var resp = _casoUso.ListarTarefas(new TarefaListRequest(null, null), 1, 10);

            Assert.Equal(200, resp.status);
            Assert.Equal(2, resp.total);
            Assert.Null(_repositorio.UltimoStatusFiltrado);
            Assert.Null(_repositorio.UltimaDataFiltrada);
        }

        [Fact]
        public void ListarTarefas_ComFiltros_ConverteEEnviaAoRepositorio()
        {
            _repositorio.Semear("Certa", EnumTarefa.status.EmProgresso, new DateTime(2026, 12, 20));
            _repositorio.Semear("Outro status", EnumTarefa.status.Pendente, new DateTime(2026, 12, 20));
            _repositorio.Semear("Outra data", EnumTarefa.status.EmProgresso, new DateTime(2026, 12, 21));

            var resp = _casoUso.ListarTarefas(new TarefaListRequest("Em progresso", "20/12/2026"), 1, 10);

            Assert.Equal(200, resp.status);
            Assert.Equal(EnumTarefa.status.EmProgresso, _repositorio.UltimoStatusFiltrado);
            Assert.Equal(new DateTime(2026, 12, 20), _repositorio.UltimaDataFiltrada);
            Assert.Equal("Certa", Assert.Single(resp.dados).Titulo);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ListarTarefas_ComPaginaInvalida_Retorna400(int pagina)
        {
            var resp = _casoUso.ListarTarefas(new TarefaListRequest(null, null), pagina, 10);

            Assert.Equal(400, resp.status);
            Assert.False(_repositorio.ListarFoiChamado);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void ListarTarefas_ComTamanhoInvalido_Retorna400(int tamanho)
        {
            var resp = _casoUso.ListarTarefas(new TarefaListRequest(null, null), 1, tamanho);

            Assert.Equal(400, resp.status);
            Assert.False(_repositorio.ListarFoiChamado);
        }

        [Fact]
        public void ListarTarefas_ComStatusInvalido_Retorna400()
        {
            var resp = _casoUso.ListarTarefas(new TarefaListRequest("Cancelada", null), 1, 10);

            Assert.Equal(400, resp.status);
            Assert.False(_repositorio.ListarFoiChamado);
        }

        [Fact]
        public void ListarTarefas_ComDataInvalida_Retorna400()
        {
            var resp = _casoUso.ListarTarefas(new TarefaListRequest(null, "2026-12-20"), 1, 10);

            Assert.Equal(400, resp.status);
            Assert.False(_repositorio.ListarFoiChamado);
        }
    }
}
