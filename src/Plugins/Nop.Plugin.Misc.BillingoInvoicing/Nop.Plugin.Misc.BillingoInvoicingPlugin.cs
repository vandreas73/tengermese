using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.BillingoInvoicing
{
    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class CustomPlugin : BasePlugin
    {
        private readonly ISettingService _settingService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public CustomPlugin(ISettingService settingService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
            _settingService = settingService;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public override async Task InstallAsync()
        {
            // settings
            await _settingService.SaveSettingAsync(new BillingoInvoicingSettings
            {
                ApiKey = "YOUR_API_KEY"
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<BillingoInvoicingSettings>();

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(BillingoInvoicingDefaults.ConfigurationRouteName);
        }
    }
}
