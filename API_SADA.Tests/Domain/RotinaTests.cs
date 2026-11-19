using Domain;

namespace API_SADA.Tests.Domain
{
    public class RotinaTests
    {
        [Theory]
        [InlineData("Pendente", "pendente")]
        [InlineData("Em Progresso", "emprogresso")]
        [InlineData("  em   progresso  ", "emprogresso")]
        [InlineData("Concluída", "concluida")]
        [InlineData("CONCLUÍDA!", "concluida")]
        public void TratarTexto_RemoveAcentosEspacosESimbolos(string entrada, string esperado)
        {
            Assert.Equal(esperado, Rotina.TratarTexto(entrada));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TratarTexto_ComTextoVazio_RetornaOMesmoValor(string? entrada)
        {
            Assert.Equal(entrada, Rotina.TratarTexto(entrada!));
        }

        [Fact]
        public void ConverterDataConvencional_ComFormatoBrasileiro_ConverteCorretamente()
        {
            Assert.Equal(new DateTime(2026, 12, 31), Rotina.ConverterDataConvencional("31/12/2026"));
        }

        [Theory]
        [InlineData("2026-12-31")]
        [InlineData("12/31/2026")]
        [InlineData("31/12/26")]
        [InlineData("texto")]
        [InlineData("")]
        public void ConverterDataConvencional_ComFormatoInvalido_LancaExcecao(string data)
        {
            var ex = Assert.Throws<Exception>(() => Rotina.ConverterDataConvencional(data));
            Assert.Equal("Data inválida.", ex.Message);
        }

        [Fact]
        public void ValidarCampoGuidString_ComGuidValido_RetornaTrue()
        {
            Assert.True(Rotina.ValidarCampoGuidString(Guid.NewGuid().ToString()));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("")]
        public void ValidarCampoGuidString_ComValorInvalido_RetornaFalse(string valor)
        {
            Assert.False(Rotina.ValidarCampoGuidString(valor));
        }
    }
}
