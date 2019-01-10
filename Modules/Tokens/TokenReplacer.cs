using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Core.Events;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Core.System.AutoInitializer;
using System.Collections.Generic;
using System.IO;
using Telerik.Sitefinity.Web.Events;
using SitefinityWebApp.Logisz.Core.System.Plugins;

namespace SitefinityWebApp.Logisz.Modules
{
    [LogiszAutoInitializeOrder(1)]
    public class TokenReplacer : LogiszPlugin
    {
        #region Attributes

        //private static TokensConfig tokenConfig = Config.Get<TokensConfig>();
        private List<string> _tokenize = new List<string>() { ".txt" };
        private readonly ILogiszEventManager _logiszEventManager;

        #endregion

        #region Constructor

        public TokenReplacer(ILogiszEventManager logiszEventManager)
        {
            this._logiszEventManager = logiszEventManager;
        }

        public override void Initialize()
        {
            DefineModuleScopedConfig(() => globalConfig.Modules.Shortcoder);
            _logiszEventManager.RegisterLogiszEvent<IPagePreRenderCompleteEvent>(OnPagePreRenderCompleteEventHandler);
        }

        #endregion

        /// <summary>
        /// page
        /// </summary>
        /// <param name="e"></param>
        public void OnPagePreRenderCompleteEventHandler(IPagePreRenderCompleteEvent e)
        {
            ILogiszConfigManager _logiszConfigManager = LogiszDependencyContainer.Resolve<ILogiszConfigManager>();

            if (!_logiszConfigManager.GetConfig().Modules.Shortcoder.Active)
                return;

            if (!e.PageSiteNode.IsBackend)
            {
                if (!Path.HasExtension(e.Page.Request.Url.AbsolutePath) ||
                    (Path.HasExtension(e.Page.Request.Url.AbsolutePath) && _tokenize.Contains(Path.GetExtension(e.Page.Request.Url.AbsolutePath).ToLower())))
                {
                    e.Page.Response.Filter = new TokenizedStream(e.Page.Response.Filter);
                }
            }
        }

    }
}