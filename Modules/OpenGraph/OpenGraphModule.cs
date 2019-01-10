
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using Telerik.Sitefinity;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Modules.News;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web;
using System.Web.UI.HtmlControls;
using System.Web;
using Telerik.Sitefinity.Web.Events;
using System.Web.UI.WebControls;
using SitefinityWebApp.Logisz.Modules.Config;
using SitefinityWebApp.Logisz.Core.System.AutoInitializer;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Core.Events;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Core.System.Logger;
using SitefinityWebApp.Logisz.Core.System.Plugins;

namespace SitefinityWebApp.Logisz.Modules
{
    [LogiszAutoInitializeOrder(5)]
    public class OpenGraphModule : LogiszPlugin
    {
        #region Attributes

        
        /*private Guid currentPage;
        private PageNode page;
        private PageData currentPageData;
        private Page pageHandler;*/

        private OpenGraphModel openGraphModel;

        private string companyName, openGraphDefaultDescription, openGraphDefaultImage, cultureName, openGraphDefaultTitle, finalCurrentModuleType = string.Empty;

        private List<HtmlMeta> HtmlMetaControls = new List<HtmlMeta>();
        private const char seperator = ',';

        private readonly PageManager _pageManager;
        private readonly LibrariesManager _liberariesManager;
        private readonly ILogiszEventManager _logiszEventManager;
        private readonly ILogiszLogger _logiszLogger;

        #endregion

        #region Constructor and initialization

        public OpenGraphModule(ILogiszEventManager logiszEventManager, ILogiszLogger logiszLogger)
        {
            this._pageManager = PageManager.GetManager();
            this._liberariesManager = LibrariesManager.GetManager();
            this._logiszEventManager = logiszEventManager;
            this._logiszLogger = logiszLogger;
        }

        /// <summary>
        /// Auto initializer
        /// </summary>
        public override void Initialize()
        {
            DefineModuleScopedConfig(() => globalConfig.Modules.Opengraph);

            //Register OG event
            _logiszEventManager.RegisterLogiszEvent<IPagePreRenderCompleteEvent>(OnPagePreRenderCompleteEventHandler);
        }

        #endregion


        /// <summary>
        /// Hits on page visit
        /// </summary>
        /// <param name="e"></param>
        public void OnPagePreRenderCompleteEventHandler(IPagePreRenderCompleteEvent e)
        {
            if (!IsActive)
                return;

            //Ensure the model is as new.
            openGraphModel = new OpenGraphModel();

            if (SiteMapBase.GetActualCurrentNode() != null)
            {
                Guid currentPageId = SiteMapBase.GetActualCurrentNode().Id;

                PageNode page = _pageManager.GetPageNode(currentPageId);

                //Validate current page
                if (page == null)
                    return;
                if (page.IsBackend)
                    return;

                openGraphModel.SetActivePage(page);
            } else
            {
                //Invalid
                return;
            }

            if(!IsValid)
            {
                _logiszLogger.LogException("Failed to enable opengraph due an error:" + GetErrorMessage());
                return;
            }

            Index();
        }


        public void Index()
        {		
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            cultureName = cultureInfo.Name;
            //Defaults
            companyName = globalConfig.CompanyName;
            openGraphDefaultTitle = globalConfig.Modules.Opengraph.DefaultOGTitle;
            openGraphDefaultDescription = globalConfig.Modules.Opengraph.DefaultOGDescription;
            openGraphDefaultImage = String.Empty;


            OpengraphModuleConfig type = null;
            bool foundDetailContentItem = TryGetDetailsObjectByPageUrl(out type);

            //Check foundDetailContentItem to avoid list pages to take over another type
            if (type != null && foundDetailContentItem)//
            {
                openGraphModel.ogType = type.OgType;
            }

			try
            {
                openGraphModel.ogDescription = removeHtml(openGraphModel.ogDescription);
                openGraphModel.ogTitle = removeHtml(openGraphModel.ogTitle);

            }
            catch (Exception e) {
                _logiszLogger.LogException(ModuleName, e);
            }

            //Update page
            BuildPageHandler();

            //Enable debug
            Debug(openGraphModel);
        }

        /// <summary>
        /// Updates the model based on details page.
        /// </summary>
        /// <returns>True if this is a detail page with a content item</returns>
        private bool TryGetDetailsObjectByPageUrl(out OpengraphModuleConfig type)
        {
            //Required assignment
            type = null;

            //cultureName = cultureInfo.Name;
            var pageUrl = UrlPath.ResolveUrl(openGraphModel.pageHandler.Request.Path, true, true);

            //Removing host and pathnames
            //pageUrl = pageUrl.Replace(openGraphModel.page.GetDefaultUrl(), "");//
            pageUrl = pageUrl.Replace(openGraphModel.pageHandler.Request.Url.Host, "");
            pageUrl = pageUrl.Replace(openGraphModel.pageHandler.Request.Url.Scheme + "://", "");

            //Get current data item
            bool foundDetailContentItem = false;

            if (pageUrl != String.Empty)
                using (var fluent = App.WorkWith())
                {
                    //Get the page controls
                    var controls = openGraphModel.currentPageData.Controls;

                    if (controls != null || controls.Count > 0)
                    {
                        //Loop the page controls to find out which type it is.
                        foreach (PageControl pageControl in controls)
                        {
                            IEnumerable<ControlProperty> props = pageControl.GetProperties(true);
                            string key = pageControl.Caption.ToLower();

                            //List found controls.
                            openGraphModel.pageControlTypes.Add(key);

                            type = globalConfig.Modules.Opengraph.Modules[key];
                            if (type != null)
                            {
                                foundDetailContentItem = GetDynamicItemFromPageUrl(pageUrl, type);
                                if (foundDetailContentItem)
                                    break;
                            }
                        }
                    }
                }

            return foundDetailContentItem;
        }


        /// <summary>
        /// This method builds the page handler it's OG meta controls.
        /// </summary>
        private void BuildPageHandler()
        {
            #region Title

            /* OpenGraph Title */
            string openGraphTitle = String.Empty;
            try
            {
                openGraphTitle = openGraphModel.page.GetValue("LgszOpenGraphTitle").ToString();
            }
            catch (Exception) { }

            var metaOpenGraphTitle = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphTitle.Attributes.Add("name", "LgszOpenGraphTitle");
            metaOpenGraphTitle.Attributes.Add("property", "og:title");
            metaOpenGraphTitle.Content = !String.IsNullOrEmpty(openGraphTitle) ? openGraphTitle : openGraphModel.page.Title.ToString(); // If openGraphTitle is not null, do openGraphTitle, else take Page.Title

            //If the custom title is not null
            if(openGraphModel.ogTitle != null && openGraphModel.ogTitle != String.Empty)
            {
                metaOpenGraphTitle.Content = openGraphModel.ogTitle;
            }
            //Set in meta
            if(metaOpenGraphTitle.Content != String.Empty)
            {
                //metaOpenGraphTitle.Content = LogiszPageTitleHandler.AppendTitleWithCompanyName(metaOpenGraphTitle.Content);

                openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphTitle);
                openGraphModel.ogTitle = metaOpenGraphTitle.Content;
            }

            #endregion

            #region Description 

            string openGraphDescription = String.Empty;
            var metaOpenGraphDescription = new System.Web.UI.HtmlControls.HtmlMeta();
            try
            {
                openGraphDescription = openGraphModel.page.GetValue("LgszOpenGraphDescription").ToString();
            }
            catch (Exception) { }
            metaOpenGraphDescription.Attributes.Add("name", "LgszOpenGraphDescription");
            metaOpenGraphDescription.Attributes.Add("property", "og:description");
            metaOpenGraphDescription.Content = openGraphDescription;

            if (String.IsNullOrEmpty(metaOpenGraphDescription.Content))
                metaOpenGraphDescription.Content = openGraphModel.currentPageData.Description.ToString();

            //If the custom title is not null
            if (openGraphModel.ogDescription != null && openGraphModel.ogDescription != String.Empty)
            {
                metaOpenGraphDescription.Content = openGraphModel.ogDescription;
            }

            if (String.IsNullOrEmpty(metaOpenGraphDescription.Content))
                metaOpenGraphDescription.Content = openGraphDefaultDescription;

            if (metaOpenGraphDescription.Content != String.Empty)
            {
                openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphDescription);
                openGraphModel.ogDescription = metaOpenGraphDescription.Content;
            }


            #endregion

            #region Image

            /* OpenGraph Image */
            var metaOpenGraphImage = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphImage.Attributes.Add("name", "LgszOpenGraphImage");
            metaOpenGraphImage.Attributes.Add("property", "og:image");
            IDataItem dataItem = null;
            try
            {
                dataItem = openGraphModel.page.GetRelatedItems("LgszOpenGraphImage").FirstOrDefault();
            }
            catch (Exception) { }
            if (dataItem != null)
            {

                var manager = LibrariesManager.GetManager();
                var image = manager.GetImages().FirstOrDefault(i => i.Id == dataItem.Id);

                if (image != null)
                {
                    metaOpenGraphImage.Content = image.MediaUrl;
                }

            }
            else if(openGraphDefaultImage != String.Empty && openGraphDefaultImage != null)
            {
                var currentURL = openGraphModel.pageHandler.Request.Url.Scheme + System.Uri.SchemeDelimiter + openGraphModel.pageHandler.Request.Url.Host;
                metaOpenGraphImage.Content = currentURL + openGraphDefaultImage;
            }

            if(openGraphModel.ogImage != null && openGraphModel.ogImage != String.Empty)
            {
                metaOpenGraphImage.Content = openGraphModel.ogImage;
            }

            if(metaOpenGraphImage.Content != null && metaOpenGraphImage.Content != String.Empty)
            {
                openGraphModel.ogImage = metaOpenGraphImage.Content;
                openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphImage);
            }

            #endregion

            /* OpenGraph Locale */
            var metaOpenGraphLocale = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphLocale.Attributes.Add("name", "LgszOpenGraphLocale");
            metaOpenGraphLocale.Attributes.Add("property", "og:locale");
            metaOpenGraphLocale.Content = cultureName;
            openGraphModel.ogLocale = metaOpenGraphLocale.Content;

            /* OpenGraph Site Name */
            var metaOpenGraphSiteName = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphSiteName.Attributes.Add("name", "LgszOpenGraphSiteName");
            metaOpenGraphSiteName.Attributes.Add("property", "og:site_name");
            metaOpenGraphSiteName.Content = companyName;
            openGraphModel.ogSiteName = metaOpenGraphSiteName.Content;

            /* OpenGraph Site Name */
            var metaOpenGraphFbAppId = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphSiteName.Attributes.Add("name", "LgszOpenGraphFbAppId");
            metaOpenGraphSiteName.Attributes.Add("property", "fb:app_id");
            metaOpenGraphSiteName.Content = globalConfig.Modules.Opengraph.FbAppId;
            openGraphModel.ogFbAppId = metaOpenGraphSiteName.Content;

            /* OpenGraph URL */
            var metaOpenGraphUrl = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphUrl.Attributes.Add("name", "LgszOpenGraphUrl");
            metaOpenGraphUrl.Attributes.Add("property", "og:url");
            metaOpenGraphUrl.Content = UrlPath.ResolveUrl(openGraphModel.pageHandler.Request.Path, true, true); ;
            openGraphModel.ogUrl = metaOpenGraphUrl.Content;

            /* OpenGraph Type */
            var metaOpenGraphType = new System.Web.UI.HtmlControls.HtmlMeta();
            metaOpenGraphType.Attributes.Add("name", "LgszOpenGraphType");
            metaOpenGraphType.Attributes.Add("property", "og:type");
            metaOpenGraphType.Content = "page";
            if(!String.IsNullOrEmpty(openGraphModel.ogType))
            {
                metaOpenGraphType.Content = openGraphModel.ogType;
            }

            openGraphModel.ogType = metaOpenGraphType.Content;
            openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphLocale);
            openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphSiteName);
            openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphUrl);
            openGraphModel.pageHandler.Header.Controls.Add(metaOpenGraphType);
        }


        /// <summary>
        /// Gets an dynamic item from page url. This method automatically updates the opengraph model.
        /// </summary>
        /// <param name="pageUrl">The page url (Without host and single segment</param>
        /// <param name="finalCurrentModuleType">The used type</param>
        /// <returns></returns>
        private bool GetDynamicItemFromPageUrl(string pageUrl, OpengraphModuleConfig ogConfigElement)
        {
            Type itemsType = null;

            //Try catched this to avoid errors for build in modules
            try {

                bool returnStatement = false;
                //Remove white spaces
                ogConfigElement.ModuleType = ogConfigElement.ModuleType.Replace(" ", "");

                //Check for multiple types
                if (ogConfigElement.ModuleType.Contains(seperator))
                {
                    //Loop all given types
                    foreach(string stringType in ogConfigElement.ModuleType.Split(seperator))
                    {
                        itemsType = TypeResolutionService.ResolveType(stringType);
                        returnStatement =  GetDataItemByType(itemsType, ogConfigElement, pageUrl);

                        if (returnStatement)
                            return returnStatement;
                    }
                } else
                {
                    //Only one type, get it.
                    itemsType = TypeResolutionService.ResolveType(ogConfigElement.ModuleType);
                    returnStatement =  GetDataItemByType(itemsType, ogConfigElement, pageUrl);

                    if (returnStatement)
                        return returnStatement;
                }
                return returnStatement;
            }
            catch(Exception e)
            {
                _logiszLogger.LogException(ModuleName, e);
                return false;
            }
        }


        /// <summary>
        /// Updates the OG item model.
        /// </summary>
        /// <param name="itemsType">Type of the item</param>
        /// <param name="ogModel">The open graph model</param>
        /// <param name="pageUrl">The url</param>
        /// <returns> true or false</returns>
        private bool GetDataItemByType(Type itemsType, OpengraphModuleConfig ogModel, string pageUrl)
        {
            switch (itemsType.FullName)
            {
                //Default type
                case "Telerik.Sitefinity.News.Model.NewsItem":
                    NewsManager newsManager = NewsManager.GetManager();
                    List<Telerik.Sitefinity.News.Model.NewsItem> items = newsManager.GetNewsItems().Where(i => i.Status == ContentLifecycleStatus.Live && i.Visible == true).ToList();
                    Telerik.Sitefinity.News.Model.NewsItem item = items.FirstOrDefault(newsItem => newsItem.ItemDefaultUrl == pageUrl);

                    if (item == null)
                        return false;

                    //Default
                    openGraphModel.ogTitle = item.Title.ToString();
                    //Given property
                    if (item.DoesFieldExist(ogModel.TitlePropertyName))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>(ogModel.TitlePropertyName).ToString()))
                            openGraphModel.ogTitle = item.GetValue<Lstring>(ogModel.TitlePropertyName).ToString();
                            
                    //Default OG prop
                    if (item.DoesFieldExist("OpenGraphTitle"))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>("OpenGraphTitle").ToString()))
                            openGraphModel.ogTitle = item.GetValue<Lstring>("OpenGraphTitle").ToString();
                    //OG prop
                    if (item.DoesFieldExist("LgszOpenGraphTitle"))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>("LgszOpenGraphTitle").ToString()))
                            openGraphModel.ogTitle = item.GetValue<Lstring>("LgszOpenGraphTitle").ToString();



                    //Default
                    openGraphModel.ogDescription = item.Summary != null ? item.Summary.ToString() : openGraphDefaultDescription;
                    //Given property
                    if (item.DoesFieldExist(ogModel.DescriptionPropertyName))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>(ogModel.DescriptionPropertyName).ToString()))
                            openGraphModel.ogDescription = item.GetValue<Lstring>(ogModel.DescriptionPropertyName).ToString();
                    //Default OG prop
                    if (item.DoesFieldExist("OpenGraphDescription"))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>("OpenGraphDescription").ToString()))
                            openGraphModel.ogDescription = item.GetValue<Lstring>("OpenGraphDescription").ToString();
                    //OG prop
                    if (item.DoesFieldExist("LgszOpenGraphDescription"))
                        if (!String.IsNullOrEmpty(item.GetValue<Lstring>("LgszOpenGraphDescription").ToString()))
                            openGraphModel.ogDescription = item.GetValue<Lstring>("LgszOpenGraphDescription").ToString();



                    Telerik.Sitefinity.Libraries.Model.Image image = null;
                    IDataItem newsImage = null;

                    openGraphModel.ogImage = openGraphDefaultImage;
                    if (item.DoesFieldExist(ogModel.ImagePropertyName))
                        if (item.GetRelatedItems(ogModel.ImagePropertyName).FirstOrDefault() != null)
                            newsImage = item.GetRelatedItems(ogModel.ImagePropertyName).FirstOrDefault();

                    if (item.DoesFieldExist("OpenGraphImage"))
                        if (item.GetRelatedItems("OpenGraphImage").FirstOrDefault() != null)
                            newsImage = item.GetRelatedItems("OpenGraphImage").FirstOrDefault();

                    if (item.DoesFieldExist("LgszOpenGraphImage"))
                        if (item.GetRelatedItems("LgszOpenGraphImage").FirstOrDefault() != null)
                            newsImage = item.GetRelatedItems("LgszOpenGraphImage").FirstOrDefault();

                    if (newsImage != null)
                        image = _liberariesManager.GetImages().FirstOrDefault(ogImg => ogImg.Id == newsImage.Id);
                    if (image != null)
                        openGraphModel.ogImage = image.Url;

                    return true;

                default:
                    DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();

                    try
                    {
                        string redirectUrl;
                        DynamicContent currentDynamicItem = dynamicModuleManager.Provider.GetItemFromUrl(itemsType, pageUrl, true, out redirectUrl) as DynamicContent;

                        if (currentDynamicItem == null)
                            return false;


                        //Given property
                        openGraphModel.ogTitle = openGraphDefaultTitle;
                        
                        if (!String.IsNullOrEmpty(ogModel.TitlePropertyName))
                        {
                            openGraphModel.ogTitle = ParseFieldTags(ogModel.TitlePropertyName, currentDynamicItem);
                        }
                        //Default OG prop
                        if (currentDynamicItem.DoesFieldExist("OpenGraphTitle"))
                            if (!String.IsNullOrEmpty(currentDynamicItem.GetValue<Lstring>("OpenGraphTitle").ToString()))
                                openGraphModel.ogTitle = currentDynamicItem.GetValue<Lstring>("OpenGraphTitle").ToString();
                        //OG prop
                        if (currentDynamicItem.DoesFieldExist("LgszOpenGraphTitle"))
                            if (!String.IsNullOrEmpty(currentDynamicItem.GetValue<Lstring>("LgszOpenGraphTitle").ToString()))
                                openGraphModel.ogTitle = currentDynamicItem.GetValue<Lstring>("LgszOpenGraphTitle").ToString();


                        //Default
                        openGraphModel.ogDescription = openGraphDefaultDescription;

                        if (!String.IsNullOrEmpty(ogModel.DescriptionPropertyName))
                        {
                            openGraphModel.ogDescription = ParseFieldTags(ogModel.DescriptionPropertyName, currentDynamicItem);
                        }

                        //Default OG prop
                        if (currentDynamicItem.DoesFieldExist("OpenGraphDescription"))
                            if (!String.IsNullOrEmpty(currentDynamicItem.GetValue<Lstring>("OpenGraphDescription").ToString()))
                                openGraphModel.ogDescription = currentDynamicItem.GetValue<Lstring>("OpenGraphDescription").ToString();
                        //OG prop
                        if (currentDynamicItem.DoesFieldExist("LgszOpenGraphDescription"))
                            if (!String.IsNullOrEmpty(currentDynamicItem.GetValue<Lstring>("LgszOpenGraphDescription").ToString()))
                                openGraphModel.ogDescription = currentDynamicItem.GetValue<Lstring>("LgszOpenGraphDescription").ToString();



                        Telerik.Sitefinity.Libraries.Model.Image img = null;
                        IDataItem dynamicContentImage = null;

                        openGraphModel.ogImage = openGraphDefaultImage;
                        if (currentDynamicItem.DoesFieldExist(ogModel.ImagePropertyName))
                            if (currentDynamicItem.GetRelatedItems(ogModel.ImagePropertyName).FirstOrDefault() != null)
                                dynamicContentImage = currentDynamicItem.GetRelatedItems(ogModel.ImagePropertyName).FirstOrDefault();

                        if (currentDynamicItem.DoesFieldExist("OpenGraphImage"))
                            if (currentDynamicItem.GetRelatedItems("OpenGraphImage").FirstOrDefault() != null)
                                dynamicContentImage = currentDynamicItem.GetRelatedItems("OpenGraphImage").FirstOrDefault();

                        if (currentDynamicItem.DoesFieldExist("LgszOpenGraphImage"))
                            if (currentDynamicItem.GetRelatedItems("LgszOpenGraphImage").FirstOrDefault() != null)
                                dynamicContentImage = currentDynamicItem.GetRelatedItems("LgszOpenGraphImage").FirstOrDefault();

                        if (dynamicContentImage != null)
                            img = _liberariesManager.GetImages().FirstOrDefault(ogImg => ogImg.Id == dynamicContentImage.Id);
                        if (img != null)
                            openGraphModel.ogImage = img.Url;


                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
            }
        }


        #region Parsers & Fixers

        /// <summary>
        /// Parses string to field tags
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="currentDynamicItem">DC item</param>
        /// <returns>string parsed string</returns>
        private string ParseFieldTags(string field, DynamicContent currentDynamicItem)
        {
            var tokens = new List<string>();

            //Get fieldnames
            Regex regex = new Regex(@"\{\{(.+?)\}\}");
            foreach (Match match in regex.Matches(field))
            {
                tokens.Add(match.Groups[0].Value);
            }

            //Loop given fields
            string result = field;
            foreach (string key in tokens)
            {
                string fieldName = key;
                fieldName = fieldName.Replace("{{", string.Empty);
                fieldName = fieldName.Replace("}}", string.Empty);

                //Skip if it is a unknown field
                if (!currentDynamicItem.DoesFieldExist(fieldName))
                    continue;

                //get value of key
                string fieldValue = currentDynamicItem.GetValue<Lstring>(fieldName).ToString();
                //Append
                if (!String.IsNullOrEmpty(fieldValue))
                {
                    result = result.Replace(key, fieldValue);
                }
            }

            return removeHtml(result);
        }


        /// <summary>
        /// Removes html tags
        /// </summary>
        /// <param name="stringWithHtml">Html</param>
        /// <returns>String</returns>
        private string removeHtml(string stringWithHtml)
        {
            if (String.IsNullOrEmpty(stringWithHtml))
                return String.Empty;

            return Regex.Replace(stringWithHtml, @"<[^>]*>", String.Empty);
        }


        /// <summary>
        /// Parses special characters, avoiting getting ? in tekst.
        /// </summary>
        /// <param name="value">The value with special characters</param>
        /// <returns>Safe string</returns>
        private string FixSpecialCharacters(string value)
        {
            if (value == null)
                return String.Empty;

            value = HttpUtility.HtmlDecode(value);
            value = removeHtml(value);

            return value;
        }


        #endregion

    }
}