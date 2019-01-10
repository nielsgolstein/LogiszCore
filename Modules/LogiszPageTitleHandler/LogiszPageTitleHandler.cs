/* Logisz Page Title Handler
 * 
 * Allows us to modify the page title of every page.
 * Version 1.0
 **/

using SitefinityWebApp.Logisz.Core.System.AutoInitializer;
using SitefinityWebApp.Logisz.Core.Configurations.Config;
using SitefinityWebApp.Logisz.Core.Modules.Debugger;
using SitefinityWebApp.Logisz.Core.Events;
using System;
using System.Web.UI;
using Telerik.Sitefinity.Web.Events;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.System.Logger;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Core.System.Plugins;

namespace SitefinityWebApp.Logisz.Modules
{
    [LogiszAutoInitializeOrder(2)]
    public class LogiszPageTitleHandler : LogiszPlugin
    {
        #region Attributes

        private readonly ILogiszEventManager _logiszEventManager;
        private readonly ILogiszLogger _logiszLogger;
        private readonly ILogiszDebugger _logiszDebugger;

        #endregion

        #region Constructor

        public LogiszPageTitleHandler(ILogiszEventManager logiszEventManager,
            ILogiszLogger logiszLogger,
            ILogiszDebugger logiszDebugger)
        {
            this._logiszEventManager = logiszEventManager;
            this._logiszLogger = logiszLogger;
            this._logiszDebugger = logiszDebugger;
        }

        /// <summary>
        /// Automatically initializes
        /// </summary>
        public override void Initialize()
        {
            DefineModuleScopedConfig(() => globalConfig.Modules.PageTitleHandler);

            _logiszEventManager.RegisterLogiszEvent<IPagePreRenderCompleteEvent>(OnPagePreRenderCompleteEventHandler);
        }

        #endregion

        /// <summary>
        /// The on pre render event handler
        /// </summary>
        /// <param name="evt"></param>
        public void OnPagePreRenderCompleteEventHandler(IPagePreRenderCompleteEvent evt)
        {
            //Do nothing if it's not active
            if (!globalConfig.Modules.PageTitleHandler.Active)
                return;

            ExtendPageTitleWithCompanyName(evt.Page);
        }


        /// <summary>
        /// Adds the company name to our page title
        /// </summary>
        /// <param name="page">The page</param>
        private void ExtendPageTitleWithCompanyName(Page page)
        {
            try
            {
                if (page.IsBackend())
                    return;

                string oldPageTitle = page.Title;
                page.Title = AppendTitleWithCompanyName(page.Title);

                ILogiszDebugger debugger = _logiszDebugger;
                debugger.AddDebugData("Old title", oldPageTitle);
                debugger.AddDebugData("Company name", globalConfig.CompanyName);
                debugger.AddDebugData("Format", globalConfig.Modules.PageTitleHandler.Format);
                debugger.AddDebugData("Final title", page.Title);
                debugger.TryEnableDebugger("Page title handler", globalConfig.Modules.PageTitleHandler);
               
            } catch(Exception e) {
                _logiszLogger.Log("LogiszPageTitleHandler: Error, " + e.Message + e.InnerException);
            }
            
        }


        /// <summary>
        /// Appends a value with the company name
        /// </summary>
        /// <param name="title">The title</param>
        /// <returns>string</returns>
        public static string AppendTitleWithCompanyName(string title)
        {
            LogiszConfig config = LogiszDependencyContainer.Resolve<ILogiszConfigManager>().GetConfig();
            if (!config.Modules.PageTitleHandler.Active)
                return title;


            string companyName = config.CompanyName;
            string format = config.Modules.PageTitleHandler.Format;

            if (!title.Contains(companyName))
            {
                title = String.Format(format, title, companyName);
            }

            return title;
        }


    }
}