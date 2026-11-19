using Domain.Tarefa;

namespace API_SADA.Tests.Domain
{
    public class EnumTarefaTests
    {
        [Theory]
        [InlineData("pendente", EnumTarefa.status.Pendente)]
        [InlineData("Em progresso", EnumTarefa.status.EmProgresso)]
        [InlineData("EmProgresso", EnumTarefa.status.EmProgresso)]
        [InlineData("Concluída", EnumTarefa.status.Concluida)]
        public void GetEnumStatus_ConverteTextoParaEnum(string texto, EnumTarefa.status esperado)
        {
            Assert.Equal(esperado, EnumTarefa.GetEnumStatus(texto));
        }

        [Fact]
        public void GetEnumStatus_ComTextoInvalido_LancaExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EnumTarefa.GetEnumStatus("Cancelada"));
        }

        [Theory]
        [InlineData(EnumTarefa.status.Pendente, "Pendente")]
        [InlineData(EnumTarefa.status.EmProgresso, "Em Progresso")]
        [InlineData(EnumTarefa.status.Concluida, "Concluída")]
        public void GetStatusDescription_RetornaDescricaoAmigavel(EnumTarefa.status status, string esperado)
        {
            Assert.Equal(esperado, EnumTarefa.GetStatusDescription(status));
        }

        [Fact]
        public void GetStatusDescription_ComValorInvalido_LancaExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EnumTarefa.GetStatusDescription((EnumTarefa.status)99));
        }
    }
}
