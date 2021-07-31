using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITMO.ADO.NET.Assignment
{
    public partial class northwndEntities : DbContext
    {
        public northwndEntities(string connectionStringName)
                   : base(connectionStringName)
        {
        }
    }
}
