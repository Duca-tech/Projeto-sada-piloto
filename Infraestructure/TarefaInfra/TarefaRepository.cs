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

        public TarefaListResponse ListarTarefas(EnumTarefa.status? status, DateTime? dataVencimento, int pagina, int tamanho)
        {
            IQueryable<TarefaInfo> query = _dbContext.Tarefas.AsNoTracking();

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (dataVencimento.HasValue)
            {
                var dia = dataVencimento.Value.Date;
                query = query.Where(x => x.DataVencimento.Date == dia);
            }

            int total = query.Count();

            var dados = query
                .OrderBy(x => x.DataVencimento)
                .ThenBy(x => x.Id)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToList();

            return new TarefaListResponse(
                total: total,
                dados: dados,
                pagina: pagina,
                totalPaginas: (int)Math.Ceiling((double)total / tamanho),
                status: 200
            );
        }

        public TarefaInfo PesquisarPorId(string id)
        {
            return _dbContext.Tarefas.Find(id);
        }
    }
}
