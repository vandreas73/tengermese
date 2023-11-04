using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.BillingoInvoicing.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.BillingoInvoicing.Fields.ApiKey.Hint")]
        public string ApiKey { get; set; }
        [NopResourceDisplayName("Plugins.Misc.BillingoInvoicing.Fields.BlockId.Hint")]
        public int BlockId { get; set; }
    }
}
