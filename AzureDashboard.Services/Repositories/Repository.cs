using AzureDashboard.Services.Helpers;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Repositories
{
    public abstract class Repository<T>
    {
        protected Database database;
        public Repository(Database database)
        {
            this.database = database;
        }
        
        public abstract IEnumerable<T> All();
        public abstract bool Add(T instance);
        public abstract T Remove();
    }
}
