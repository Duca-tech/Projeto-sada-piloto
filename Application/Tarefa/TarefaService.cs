using Application.Tarefa.Dto;
using Domain;
using Domain.Tarefa;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tarefa
{
    public class TarefaService
    {
        readonly ITarefaRespositorio _tarefaRepositorio;

        readonly int Tamanho_maximo = 100;
        public TarefaService(ITarefaRespositorio tarefaRepositorio)
        {
            _tarefaRepositorio = tarefaRepositorio;
        }
        public TarefaInfo VerificarSeExiste(string Id)
        {
            try
            {
                var tarefa = _tarefaRepositorio.PesquisarPorId(Id);
                return tarefa;
            }
            catch(Exception ex)
            {
                throw new Exception($"Erro ao Pesquisar Tarefa: {ex.Message}");
            }
           
        }

        public class TarefaStatus: ValidationAttribute
        {
            private readonly string[] _statusPermitidos =
               {
                    "pendente",
                    "emprogresso",
                    "concluida"
                };
            public override bool IsValid(object? value)
            {
                if (value is not string status)
                    return false;

                return _statusPermitidos.Contains(
                    Rotina.TratarTexto(value.ToString()),
                    StringComparer.OrdinalIgnoreCase
                );
            }

        }


        public class TarefaListaValidacao()
        {

            public  bool ValidacaoPagina(int pagina)
            {
                if (pagina < 1)
                    return false;

                return true;
            }

            public bool ValidacaoTamanhoMaximo(int TamanhoPagina)
            {
                if (TamanhoPagina < 1 || TamanhoPagina > 100)
                    return false;
                return true;
            }
        }
    }
}
