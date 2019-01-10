using SitefinityWebApp.Logisz.Core.Events;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Core.System.AutoInitializer;
using SitefinityWebApp.Logisz.Core.Modules.Debugger;
using SitefinityWebApp.Logisz.Core.System.Logger;
using SitefinityWebApp.Logisz.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Localization.UrlLocalizationStrategies;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Web;
using Telerik.Sitefinity.Web.Events;
using SitefinityWebApp.Logisz.Core.System.Plugins;

namespace SitefinityWebApp.Logisz
{
    [LogiszAutoInitializeOrder(3)]
    public class LogiszCustomHrefLang : LogiszPlugin
    {
        #region Attributes

        private LogiszObjectManager logiszObjectManager;
        private PageManager pageManager;

        /**
         * The allowed control object types.
         * */
        public List<string> AllowedControlObjectTypes { get; private set; }
        public List<Type> ContentTypes { get; set; }

        private readonly ILogiszDebugger _logiszDebugger;
        private readonly ILogiszEventManager _logiszEventManager;
        private readonly ILogiszLogger _logiszLogger;

        #endregion

        #region Initialize

        private LogiszCustomHrefLang(ILogiszDebugger logiszDebugger, ILogiszEventManager logiszEventManager, ILogiszLogger logiszLogger)
        {
            this._logiszDebugger = logiszDebugger;
            this._logiszLogger = logiszLogger;
            this._logiszEventManager = logiszEventManager;

            logiszObjectManager = LogiszObjectManager.GetManager();
            pageManager = PageManager.GetManager();

            AllowedControlObjectTypes = new List<string>()
            {
                "Telerik.Sitefinity.Frontend.Mvc.Infrastructure.Controllers.MvcWidgetProxy"
            };

            //ContentTypes = logiszObjectManager.GetTypes<DynamicContent>();
        }

        public override void Initialize()
        {
            DefineModuleScopedConfig(() => globalConfig.Modules.Hreflang);

            _logiszEventManager.RegisterLogiszEvent<IPagePreRenderCompleteEvent>(OnPagePreRenderCompleteEventHandler);
        }

        #endregion

        /// <summary>
        /// The pre render event which is triggered automatically.
        /// </summary>
        /// <param name="e"></param>
        public void OnPagePreRenderCompleteEventHandler(IPagePreRenderCompleteEvent e)
        {
            //Skip back-end.
            if (e.PageSiteNode.IsBackend)
                return;


            if (!globalConfig.Modules.Hreflang.Active)
                return;

            List<HtmlLink> hrefLangLinks = GetAlternateLinks(e, globalConfig.Modules.Hreflang.IncludeCurrentLanguage);
            
            try
            {
                foreach (HtmlLink link in hrefLangLinks)
                {
                    e.Page.Header.Controls.Add(link);
                    string q = null;
                    q = q.ToLower();
                }
            } catch (Exception ex) {
                _logiszLogger.LogException("Hreflang", ex);
            }

            Debug();
        }


        /// <summary>
        /// Gets the hreflangs
        /// </summary>
        /// <param name="e"></param>
        /// <param name="includeCurrentLanguage"></param>
        /// <returns></returns>
        private List<HtmlLink> GetAlternateLinks(IPagePreRenderCompleteEvent e, bool includeCurrentLanguage)
        {
            _logiszDebugger.AddDebugData("Include current language", includeCurrentLanguage.ToString());

            var alternateLinks = new List<HtmlLink>();

            #region Get page node

            PageNode pageNode = pageManager.GetPageNodes().Where(pN => pN.PageId == e.PageSiteNode.PageId).FirstOrDefault();

            #endregion

            #region Try get content item

            bool foundDetailContentItem = false;
            string urlName = RemoveHostFromUrl(e.Page.Request);
            _logiszDebugger.AddDebugData("Urlname", urlName);

            object contentObject = null;
            IHasUrlName o = null;
            Lstring finalUrlName = new Lstring();

            //Check if excluded
            if (globalConfig.Modules.Hreflang.PageIsExcluded(urlName))
            {
                _logiszDebugger.AddDebugData("Page is excluded", "True");
                return null;
            }

            bool pageWithUrlNameExists = (pageManager.GetPageNodes().Where(q => q.UrlName == urlName).Count() != 0);
            _logiszDebugger.AddDebugData("Found page with urlname", pageWithUrlNameExists.ToString());
            if (!pageWithUrlNameExists)
            {
                //No page found, search object.
                foundDetailContentItem = GetObjectByUrlName(pageNode, urlName, out contentObject);
                _logiszDebugger.AddDebugData("Found content item with urlname", foundDetailContentItem.ToString());

                if (contentObject != null)
                {
                    o = (IHasUrlName)contentObject;

                    if (o != null)
                        finalUrlName = o.UrlName;

                    //validate type of static modules
                    _logiszDebugger.AddDebugData("Type of content", contentObject.GetType().Name);
                    if (contentObject.GetType() == typeof(NewsItem))
                    {
                        NewsItem n = (NewsItem)contentObject;
                        finalUrlName = n.ItemDefaultUrl;
                    }
                }
               
            }



            #endregion

            #region Check for detailed page

            //Detailed page
            var cultures = includeCurrentLanguage ? e.PageSiteNode.AvailableLanguages : e.PageSiteNode.AvailableLanguages.Where(c => c != CultureInfo.CurrentUICulture);
            var ulService = ObjectFactory.Resolve<UrlLocalizationService>();
            if (foundDetailContentItem)
            {
                foreach (var culture in cultures.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                {
                    var defaultLocation = UrlPath.ResolveUrl(ulService.ResolveUrl(e.PageSiteNode.GetUrl(culture, false), culture), true, true);
                    if (!string.IsNullOrWhiteSpace(defaultLocation))
                    {
                        string languagedUrlName = String.Empty;
                        finalUrlName.TryGetValue(out languagedUrlName, culture);

                        defaultLocation += "/" + languagedUrlName;
                        var alternateControl = new HtmlLink();
                        alternateControl.Attributes.Add("rel", "alternate");
                        alternateControl.Attributes.Add("href", defaultLocation);
                        alternateControl.Attributes.Add("hreflang", culture.TextInfo.CultureName);

                        _logiszDebugger.AddDebugData("Added hreflang tag ("+ culture.TextInfo.CultureName + ")", defaultLocation);

                        alternateLinks.Add(alternateControl);
                    }
                }
            }

            #endregion

            #region Static page
            //Static page
            else
            {

                foreach (var culture in cultures.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                {
                    var defaultLocation = UrlPath.ResolveUrl(ulService.ResolveUrl(e.PageSiteNode.GetUrl(culture, false), culture), true, true);
                    if (!string.IsNullOrWhiteSpace(defaultLocation))
                    {
                        var alternateControl = new HtmlLink();
                        alternateControl.Attributes.Add("rel", "alternate");
                        alternateControl.Attributes.Add("href", defaultLocation);
                        alternateControl.Attributes.Add("hreflang", culture.TextInfo.CultureName);
                        _logiszDebugger.AddDebugData("Added hreflang tag (" + culture.TextInfo.CultureName + ")", defaultLocation);
                        alternateLinks.Add(alternateControl);
                    }
                }
            }

            #endregion

            return alternateLinks;
        }


        /// <summary>
        /// Gets a object by controls on the page, searches for a url!
        /// </summary>
        /// <param name="node">The page node</param>
        /// <param name="pd">The page data</param>
        /// <param name="urlName">The url name</param>
        /// <returns></returns>
        private bool GetObjectByUrlName(PageNode node, string urlName, out object contentObject)
        {
            List<Type> AllowedContentTypes = new List<Type>();
            AllowedContentTypes.AddRange(logiszObjectManager.GetTypes<DynamicContent>());

            contentObject = logiszObjectManager.GetDataItemByUrlName(urlName, AllowedContentTypes);

            //Extend function to filter for HasUrlName
            if (contentObject != null)
                if (!contentObject.GetType().ImplementsInterface(typeof(IHasUrlName)))
                    contentObject = null;

            return contentObject != null;
        }



        /// <summary>
        /// Removes host from url
        /// </summary>
        /// <param name="httpReq">The httpRequest</param>
        /// <returns>string</returns>
        private string RemoveHostFromUrl(HttpRequest httpReq)
        {
            string url = httpReq.Url.Segments.Last();
            return url;
        }
    }
}