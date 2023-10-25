using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.BillingoInvoicing
{
    public class BillingoInvoicingSettings : ISettings
    {
        public string ApiKey { get; set; }
    }
}
