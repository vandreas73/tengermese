using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.BillingoInvoicing.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public string ApiKey { get; set; }
        public int BlockId { get; set; }
    }
}
