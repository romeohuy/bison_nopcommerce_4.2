using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Integration.KiotViet
{
    public class IntegrateKiotVietProvider : BasePlugin
    {
        
        private readonly ILocalizationService _localizationService;

        public IntegrateKiotVietProvider(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Integration.KiotViet.Error", "You can use Nop.Plugin.Integration.KiotViet");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Integration.KiotViet.Error");

            base.Uninstall();
        }

    }
}
