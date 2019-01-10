using SitefinityWebApp.Logisz.Core.Events;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Core.System.AutoInitializer;
using SitefinityWebApp.Logisz.Modules.Yoast;
using System.Web.UI;
using Telerik.Sitefinity.Web.Events;
using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Core.System.Plugins;

namespace SitefinityWebApp.Logisz.Modules
{
    [LogiszPluginData("Yoast")]
    [LogiszAutoInitializeOrder(1001)]
    public class YoastModule : LogiszPlugin
    {
        private YoastCalculationService yoastCalculationService;
        private PageValidationMethods pageValidationMethods;
        private readonly ILogiszConfigManager _logiszConfigManager;
        private readonly ILogiszEventManager _logiszEventManager;

        #region Methods

        public YoastModule(ILogiszConfigManager logiszConfigManager,
            ILogiszEventManager logiszEventManager,
            YoastCalculationService yoastCalculationService)
        {
            this._logiszConfigManager = logiszConfigManager;
            this._logiszEventManager = logiszEventManager;
            this.yoastCalculationService = yoastCalculationService;
            this.pageValidationMethods = new PageValidationMethods(_logiszConfigManager);
        }

        /// <summary>
        /// Auto initializer
        /// </summary>
        public override void Initialize()
        {
            DefineModuleScopedConfig(() => globalConfig.Modules.Yoast);

            //Register event
            _logiszEventManager.RegisterLogiszEvent<IPagePreRenderCompleteEvent>(OnPagePreRenderCompleteEventHandler);
        }

        #endregion

        #region Event(s)

        public void OnPagePreRenderCompleteEventHandler(IPagePreRenderCompleteEvent e)
        {
            Page page = e.Page;

            if (page.IsBackend())
                return;

            if (!Config.Active)
                return;

            Yoast.Models.ValidationResult result = new Yoast.Models.ValidationResult();
            result.pageValidationResult = pageValidationMethods.ValidatePage(e.PageSiteNode, e.Page);

            result.Score = yoastCalculationService.GetPageSEOScore(result);


            //Calculate score

            #region Debug

            Debug(result);

            #endregion
        }

        #endregion
    }
}