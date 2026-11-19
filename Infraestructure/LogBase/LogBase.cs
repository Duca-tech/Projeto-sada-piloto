using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.LogBase
{
    public class LogBase
    {
        readonly DbContext _db;
        public LogBase(DbContext dbContext) 
        {
            _db  = dbContext;
        }


        public void SalvarLog(Domain.Log.LogInfo log)
        {
            _db.Log.Add(log);
            _db.SaveChanges();
        }
    }
}
