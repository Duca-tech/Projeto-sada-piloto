
using Application.Tarefa.Dto;
using Domain;
using Domain.Tarefa;
using Microsoft.Extensions.Logging;
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
        readonly ILogger<TarefaCasoUso> _logger;
        readonly TarefaService.TarefaStatus _tarefaStatusValidator = new TarefaService.TarefaStatus();
        readonly TarefaService.TarefaListaValidacao _tarefaListValidator = new TarefaService.TarefaListaValidacao();
        public TarefaCasoUso(ITarefaRespositorio tarefaRepositorio, TarefaService tarefaService, ILogger<TarefaCasoUso> logger)
        {
            _tarefaRepositorio = tarefaRepositorio;
            _tarefaService = tarefaService;
            _logger = logger;
        }


        public TarefaResponse CriarTarefa(TarefaRequest request)
        {

            try
            {
                if (!string.IsNullOrEmpty(request.Id)){
                    if (!string.IsNullOrEmpty(_tarefaService.VerificarSeExiste(request.Id)?.Id))
                    {
                        _logger.LogWarning("Tentativa de criar tarefa já existente {TarefaId}", request.Id);
                        return new TarefaResponse(
                            Id: request.Id,
                            Mensagem: "Tarefa ja Criada Anteriomente",
                            status: 409,
                            Erro: null
                        );
                    }
                }

                if (!_tarefaStatusValidator.IsValid(request.Status))
                {
                    _logger.LogWarning("Status inválido ao criar tarefa: {Status}", request.Status);
                    return new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Status inválido. Os status permitidos são: Pendente, EmProgresso, Concluida.",
                        status: 400,
                        Erro: null
                    );
                }

                if (!DataValida(request.DataVencimento))
                {
                    _logger.LogWarning("Data de vencimento inválida ao criar tarefa: {DataVencimento}", request.DataVencimento);
                    return new TarefaResponse(
                        Id: request.Id,
                        Mensagem: "Data de vencimento inválida. Use o formato dd/MM/yyyy.",
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

                if (string.IsNullOrEmpty(guid_tarefa))
                {
                    _logger.LogError("Repositório não retornou o id da tarefa criada");
                    return new TarefaResponse(
                        Id: null,
                        Mensagem: "Erro ao criar tarefa",
                        status: 500,
                        Erro: null
                    );
                }

                _logger.LogInformation("Tarefa {TarefaId} criada com sucesso", guid_tarefa);
                return new TarefaResponse(
                    Id: guid_tarefa,
                    Mensagem: "Tarefa criada com sucesso",
                    status: 201,
                    Erro: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tarefa");
                var response = new TarefaResponse(
                    Id: request.Id,
                    Mensagem: "Erro ao criar tarefa",
                    status: 500,
                    Erro: ex.Message
                );

                return response;
            }

        }


        public TarefaResponse AtualizarTarefa(string id, TarefaRequest request)
        {
            try
            {
                if (!Rotina.ValidarCampoGuidString(id))
                {
                    _logger.LogWarning("Id inválido ao atualizar tarefa: {TarefaId}", id);
                    return new TarefaResponse(
                        Id: id,
                        Mensagem: "Campo Guid inválido",
                        status: 400,
                        Erro: null
                    );
                }

                if (!_tarefaStatusValidator.IsValid(request.Status))
                {
                    _logger.LogWarning("Status inválido ao atualizar tarefa {TarefaId}: {Status}", id, request.Status);
                    return new TarefaResponse(
                        Id: id,
                        Mensagem: "Status inválido. Os status permitidos são: Pendente, EmProgresso, Concluida.",
                        status: 400,
                        Erro: null
                    );
                }

                if (!DataValida(request.DataVencimento))
                {
                    _logger.LogWarning("Data de vencimento inválida ao atualizar tarefa {TarefaId}: {DataVencimento}", id, request.DataVencimento);
                    return new TarefaResponse(
                        Id: id,
                        Mensagem: "Data de vencimento inválida. Use o formato dd/MM/yyyy.",
                        status: 400,
                        Erro: null
                    );
                }

                TarefaInfo tarefa = _tarefaService.VerificarSeExiste(id);
                if (string.IsNullOrEmpty(tarefa?.Id))
                {
                    _logger.LogWarning("Tarefa {TarefaId} não encontrada para atualização", id);
                    var response = new TarefaResponse(
                        Id: id,
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

                _logger.LogInformation("Tarefa {TarefaId} atualizada com sucesso", id);
                return new TarefaResponse(
                    Id: id,
                    Mensagem: "Tarefa atualizada com sucesso",
                    status: 200,
                    Erro: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tarefa {TarefaId}", id);
                var response = new TarefaResponse(
                    Id: id,
                    Mensagem: "Erro ao atualizar tarefa",
                    status: 500,
                    Erro: ex.Message
                );
                return response;
            }
        }

        public TarefaBuscaResponse BuscarTarefa(string id)
        {
            try
            {
                if (!Rotina.ValidarCampoGuidString(id))
                {
                    _logger.LogWarning("Id inválido ao buscar tarefa: {TarefaId}", id);
                    return new TarefaBuscaResponse(
                        Dados: null,
                        Mensagem: "Campo Guid inválido",
                        status: 400,
                        Erro: null
                    );
                }

                TarefaInfo tarefa = _tarefaService.VerificarSeExiste(id);
                if (string.IsNullOrEmpty(tarefa?.Id))
                {
                    _logger.LogWarning("Tarefa {TarefaId} não encontrada", id);
                    return new TarefaBuscaResponse(
                        Dados: null,
                        Mensagem: "Tarefa não encontrada",
                        status: 404,
                        Erro: null
                    );
                }

                return new TarefaBuscaResponse(
                    Dados: tarefa,
                    Mensagem: null,
                    status: 200,
                    Erro: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tarefa {TarefaId}", id);
                return new TarefaBuscaResponse(
                    Dados: null,
                    Mensagem: "Erro ao buscar tarefa",
                    status: 500,
                    Erro: ex.Message
                );
            }
        }

        public TarefaResponse ExcluirTarefa(string id)
        {
            try
            {
                if (!Rotina.ValidarCampoGuidString(id))
                {
                    _logger.LogWarning("Id inválido ao excluir tarefa: {TarefaId}", id);
                    return new TarefaResponse(
                        Id: id,
                        Mensagem: "Campo Guid inválido",
                        status: 400,
                        Erro: null
                    );
                }

                TarefaInfo tarefa = _tarefaService.VerificarSeExiste(id);
                if (string.IsNullOrEmpty(tarefa?.Id))
                {
                    _logger.LogWarning("Tarefa {TarefaId} não encontrada para exclusão", id);
                    return new TarefaResponse(
                        Id: id,
                        Mensagem: "Tarefa não encontrada",
                        status: 404,
                        Erro: null
                    );
                }

                _tarefaRepositorio.ExcluirTarefa(tarefa);

                _logger.LogInformation("Tarefa {TarefaId} excluída com sucesso", id);
                return new TarefaResponse(
                    Id: id,
                    Mensagem: "Tarefa excluída com sucesso",
                    status: 204,
                    Erro: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tarefa {TarefaId}", id);
                return new TarefaResponse(
                    Id: id,
                    Mensagem: "Erro ao excluir tarefa",
                    status: 500,
                    Erro: ex.Message
                );
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
                    if (!DataValida(request.Datavencimento))
                    {
                        return new TarefaListResponse()
                        {
                            erro = "Data de vencimento inválida. Use o formato dd/MM/yyyy.",
                            status = 400
                        };
                    }
                    dataVencimento = Rotina.ConverterDataConvencional(request.Datavencimento);
                }

                return _tarefaRepositorio.ListarTarefas(status, dataVencimento, pagina, tamanho);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tarefas");
                return new TarefaListResponse()
                {
                    erro = $"Erro ao listar tarefas: {ex.Message}",
                    status = 500
                };
            }
        }

        private static bool DataValida(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            try
            {
                Rotina.ConverterDataConvencional(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
