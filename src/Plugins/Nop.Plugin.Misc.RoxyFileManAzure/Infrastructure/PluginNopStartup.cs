using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.RoxyFileManAzure.Services;
using Nop.Services.Media.RoxyFileman;

namespace Nop.Plugin.Misc.RoxyFileManAzure.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //register services and interfaces
            var appSettings = Singleton<AppSettings>.Instance;
            if (appSettings.Get<AzureBlobConfig>().Enabled)
            {
                services.RemoveAll<IRoxyFilemanFileProvider>();

                services.AddScoped<RoxyAzurePictureService, RoxyAzurePictureService>();
                services.AddScoped<IRoxyFilemanFileProvider, RoxyFilemanAzureFileProvider>();
            }
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 2001;
    }
}