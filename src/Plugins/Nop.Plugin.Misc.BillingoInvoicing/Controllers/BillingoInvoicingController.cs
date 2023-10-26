using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.BillingoInvoicing.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.BillingoInvoicing.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    public class BillingoInvoicingController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public BillingoInvoicingController(
            ISettingService settingService, 
            IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<BillingoInvoicingSettings>(storeId);

            var model = new ConfigurationModel()
            {
                ApiKey = settings.ApiKey,
                BlockId = settings.BlockId
            };

            return View("~/Plugins/Misc.BillingoInvoicing/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<BillingoInvoicingSettings>(storeId);

            //set new settings values
            settings.ApiKey = model.ApiKey;
            settings.BlockId = model.BlockId;

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.ApiKey, true, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.BlockId, true, storeId, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
