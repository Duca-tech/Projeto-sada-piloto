using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Domain;
using Domain.Tarefa;
using Domain.Log;

namespace Infraestructure
{
    public class DbContext: Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options):base(options) { }

        public DbSet<TarefaInfo> Tarefas => Set<TarefaInfo>();
        public DbSet<LogInfo> Log => Set<LogInfo>();
    }
}
