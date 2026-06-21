using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Core.Interfaces
{
    public interface ITenantProvider
    {
        int GetTenantId();
    }
}