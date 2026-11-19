
using Application.Tarefa.Dto;
using Domain;
using Domain.Tarefa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tarefa
{
    public class TarefaCasoUso
    {
        readonly ITarefaRespositorio _tarefaRepositorio;
        readonly TarefaService _tarefaService;
        readonly TarefaService.TarefaStatus _tarefaStatusValidator = new TarefaService.TarefaStatus();
        readonly TarefaService.TarefaListaValidacao _tarefaListValidator = new TarefaService.TarefaListaValidacao();
        public TarefaCasoUso(ITarefaRespositorio tarefaRepositorio, TarefaService tarefaService)
        {
            _tarefaRepositorio = tarefaRepositorio;
            _tarefaService = tarefaService;
        }


        public TarefaResponse CriarTarefa(TarefaRequest request)
        {

            try
            {
                if (!string.IsNullOrEmpty(request.Id)){
                    if (!string.IsNullOrEmpty(_tarefaService.VerificarSeExiste(request.Id)?.Id))
                        return new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Tarefa ja Criada Anteriomente",
                        status: 400,
                        Erro: null
                    );
                }

                if (!_tarefaStatusValidator.IsValid(request.Status))
                {
                    return new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Status inválido. Os status permitidos são: Pendente, EmProgresso, Concluida.",
                        status: 400,
                        Erro: null
                    );
                }

                var tarefa = new TarefaInfo() { 
                    Id = request.Id,
                    Titulo = request.Titulo,
                    Descricao = request.Descricao,
                    DataVencimento = Rotina.ConverterDataConvencional(request.DataVencimento),
                    Status = EnumTarefa.GetEnumStatus(request.Status)
                };

                string guid_tarefa = _tarefaRepositorio.IncluirTarefa(tarefa);


                var response = new TarefaResponse(
                    Id: guid_tarefa,
                    Mensagem: string.IsNullOrEmpty(guid_tarefa) ? "Erro ao criar tarefa" : "Tarefa criada com sucesso",
                    status: string.IsNullOrEmpty(guid_tarefa) ? 500 : 200,
                    Erro: null
                );
                return response;
            }
            catch (Exception ex)
            {
                var response = new TarefaResponse(
                    Id: request.Id,
                    Mensagem: "Erro ao criar tarefa",
                    status: 500,
                    Erro: ex.Message
                );

                return response;
            }

        }


        public TarefaResponse AtualizarTarefa(TarefaRequest request)
        {
            try
            {
                if (!Rotina.ValidarCampoGuidString(request.Id))
                {
                    return new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Campo Guid inválido",
                        status: 400,
                        Erro: null
                    );
                }
                TarefaInfo tarefa = _tarefaService.VerificarSeExiste(request.Id);
                if (string.IsNullOrEmpty(tarefa?.Id))
                {
                    var response = new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Tarefa não encontrada",
                        status: 404,
                        Erro: null
                    );
                    return response;
                }


                tarefa.Titulo = request.Titulo;
                tarefa.Descricao = request.Descricao;
                tarefa.DataVencimento = Rotina.ConverterDataConvencional(request.DataVencimento);
                tarefa.Status = EnumTarefa.GetEnumStatus(request.Status);

                _tarefaRepositorio.AtualizarTarefa(tarefa);

                return new TarefaResponse(
                    Id: request.Id,
                    Mensagem: "Tarefa atualizada com sucesso",
                    status: 200,
                    Erro: null
                );
            }
            catch (Exception ex)
            {
                var response = new TarefaResponse(
                    Id: request.Id,
                    Mensagem: "Erro ao atualizar tarefa",
                    status: 500,
                    Erro: ex.Message
                );
                return response;
            }
        }

        public TarefaListResponse ListarTarefas(TarefaListRequest request, int pagina, int tamanho)
        {
            
            try
            {
                if (!_tarefaListValidator.ValidacaoPagina(pagina))
                {
                    return new TarefaListResponse()
                    {
                        erro = "Número da página inválido. A página deve ser maior que zero.",
                        status = 400
                    };
                }

                if (!_tarefaListValidator.ValidacaoTamanhoMaximo(tamanho))
                {
                    return new TarefaListResponse()
                    {
                        erro = "Tamanho máximo de tarefas por página inválido.",
                        status = 400

                    };
                }


                EnumTarefa.status? status = null;
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (!_tarefaStatusValidator.IsValid(request.Status))
                    {
                        return new TarefaListResponse()
                        {
                            erro = "Status inválido. Os status permitidos são: Pendente, EmProgresso, Concluida.",
                            status = 400
                        };
                    }
                    status = EnumTarefa.GetEnumStatus(request.Status);
                }

                DateTime? dataVencimento = null;
                if (!string.IsNullOrWhiteSpace(request.Datavencimento))
                {
                    try
                    {
                        dataVencimento = Rotina.ConverterDataConvencional(request.Datavencimento);
                    }
                    catch
                    {
                        return new TarefaListResponse()
                        {
                            erro = "Data de vencimento inválida. Use o formato dd/MM/yyyy.",
                            status = 400
                        };
                    }
                }

                return _tarefaRepositorio.ListarTarefas(status, dataVencimento, pagina, tamanho);

            }
            catch (Exception ex)
            {
                return new TarefaListResponse()
                {
                    erro = $"Erro ao listar tarefas: {ex.Message}",
                    status = 500
                };
            }
        }
    }
}