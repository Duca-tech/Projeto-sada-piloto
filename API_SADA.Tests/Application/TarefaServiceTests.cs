using API_SADA.Tests.Fakes;
using Application.Tarefa;

namespace API_SADA.Tests.Application
{
    public class TarefaServiceTests
    {
        readonly TarefaRepositorioFake _repositorio = new();

        // ---------------------------------------------------------------- VerificarSeExiste

        [Fact]
        public void VerificarSeExiste_TarefaExistente_RetornaATarefa()
        {
            var tarefa = _repositorio.Semear();
            var service = new TarefaService(_repositorio);

            Assert.Same(tarefa, service.VerificarSeExiste(tarefa.Id));
        }

        [Fact]
        public void VerificarSeExiste_TarefaInexistente_RetornaNulo()
        {
            var service = new TarefaService(_repositorio);

            Assert.Null(service.VerificarSeExiste(Guid.NewGuid().ToString()));
        }

        // ---------------------------------------------------------------- TarefaStatus

        [Theory]
        [InlineData("Pendente")]
        [InlineData("EmProgresso")]
        [InlineData("em progresso")]
        [InlineData("Concluida")]
        [InlineData("Concluída")]
        public void TarefaStatus_ComStatusPermitido_EhValido(string status)
        {
            Assert.True(new TarefaService.TarefaStatus().IsValid(status));
        }

        [Theory]
        [InlineData("Cancelada")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(1)]
        public void TarefaStatus_ComValorNaoPermitido_EhInvalido(object? valor)
        {
            Assert.False(new TarefaService.TarefaStatus().IsValid(valor));
        }

        // ---------------------------------------------------------------- TarefaListaValidacao

        [Theory]
        [InlineData(1, true)]
        [InlineData(50, true)]
        [InlineData(0, false)]
        [InlineData(-5, false)]
        public void ValidacaoPagina_AceitaApenasPaginasPositivas(int pagina, bool esperado)
        {
            Assert.Equal(esperado, new TarefaService.TarefaListaValidacao().ValidacaoPagina(pagina));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(101, false)]
        public void ValidacaoTamanhoMaximo_AceitaDe1A100(int tamanho, bool esperado)
        {
            Assert.Equal(esperado, new TarefaService.TarefaListaValidacao().ValidacaoTamanhoMaximo(tamanho));
        }
    }
}
