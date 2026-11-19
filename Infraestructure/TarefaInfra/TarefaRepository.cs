using Application.Tarefa;
using Application.Tarefa.Dto;
using Domain.Tarefa;
using Infraestructure.LogBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.TarefaInfra
{
    public class TarefaRepository: LogBase.LogBase, ITarefaRespositorio
    {
        private readonly DbContext _dbContext;
        public TarefaRepository(DbContext db): base(db)
        {
            _dbContext = db;
        }

        public void AtualizarTarefa(TarefaInfo tarefa)
        {
            _dbContext.Tarefas.Update(tarefa);
            _dbContext.SaveChanges();

            SalvarLog(new Domain.Log.LogInfo()
            {
                Id = Guid.NewGuid().ToString(),
                Mensagem = "Tarefa Atualizada com sucesso",
                Status = (int)Domain.Log.EnumLog.StatusLog.Atualizacao,
                Tipo = (int)Domain.Log.EnumLog.TipoLog.Tarefa,
                CodigoExterno = tarefa.Id
            });

        }

        public string IncluirTarefa(TarefaInfo tarefa)
        {
            tarefa.Id = Guid.NewGuid().ToString();
           string id = _dbContext.Tarefas.Add(tarefa).Entity.Id;
            _dbContext.SaveChanges();

            SalvarLog(new Domain.Log.LogInfo()
            {
                Id = Guid.NewGuid().ToString(),
                Mensagem = "Tarefa cadastrada com sucesso",
                Status = (int)Domain.Log.EnumLog.StatusLog.Cadastro,
                Tipo = (int)Domain.Log.EnumLog.TipoLog.Tarefa,
                CodigoExterno = id
            });

            return id;


        }

        public TarefaListResponse ListarTarefas(TarefaListRequest request, int pagina, int tamanho)
        {
            var response = new TarefaListResponse(
                total: _dbContext.Tarefas.Count(),
                pagina: pagina,
                totalPaginas: (int)Math.Ceiling((double)_dbContext.Tarefas.Count() / tamanho),
                dados: _dbContext.Tarefas
                    .Skip((pagina - 1) * tamanho)
                    .Take(tamanho)
                    .Where(x=>x.DataVencimento.Equals(request.Datavencimento) && x.Status.Equals(request.Status)).ToList()
                );

            return response;


        }

        public TarefaInfo PesquisarPorId(string id)
        {
            return _dbContext.Tarefas.Find(id);
        }
    }
}
