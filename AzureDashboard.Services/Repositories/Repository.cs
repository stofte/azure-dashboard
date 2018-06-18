using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Repositories
{
    public abstract class Repository<T>
    {
        public abstract Task Start();
        public abstract IEnumerable<T> All();
        public abstract T Get();
        public abstract T Add();
        public abstract T Remove();
        protected abstract bool VerifySchema();
    }
}
