/* Logisz Opengraph (Crab)
 * 
 * Custom logisz opengraph supports opengraph for the dynamic module builder
 * All modules are configured in Settings -> Advanced -> Opengraph.
 * 
 * VERSION 1.0.12.
 */

using SitefinityWebApp.Logisz.Core.Inharitances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Services;

namespace SitefinityWebApp.Logisz.Modules
{
    public class OpenGraphModel : LogiszAutoValidationModel
    {
        public string ogLocale { get; set; }
        public string ogSiteName { get; set; }
        public string ogUrl { get; set; }
        public string ogType { get; set; }
        public string ogTitle { get; set; }
        public string ogImage { get; set; }
        public string ogDescription { get; set; }
        public string ogFbAppId { get; set; }

        public List<string> pageControlTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>

        // public Guid currentPage { get; private set; }
        public PageNode page { get; private set; }
        public PageData currentPageData { get; private set; }
        public Page pageHandler { get; private set; }

        public OpenGraphModel()
        {
            //Initialize list
            pageControlTypes = new List<string>();
        }

        /// <summary>
        /// Sets the current page
        /// </summary>
        /// <param name="page">The pagenode</param>
        public void SetActivePage(PageNode page)
        {
            try
            {
                this.page = page;
                this.currentPageData = page.GetPageData();
                this.pageHandler = (Page)SystemManager.CurrentHttpContext.CurrentHandler;
            } catch(Exception e)
            {
                SetModelInvalid("Failed to set active page");
            }

        }

        protected override bool AutoValidate()
        {
            if (page == null)
                return SetModelInvalid("Active page is not set");

            if (currentPageData == null)
                return SetModelInvalid("The current page data is unknown");

            if (pageHandler == null)
                return SetModelInvalid("The pagehandler is empty");

            return true;
        }
    }

    public class OpenGraphType
    {
        public OpenGraphType(string key, string type, string ogTitleProperty, string ogDescriptionProperty, string ogImageProperty)
        {
            this.key = key;
            this.type = type;
            this.ogTitleProperty = ogTitleProperty;
            this.ogDescriptionProperty = ogDescriptionProperty;
            this.ogImageProperty = ogImageProperty;
        }

        public string key { get; set; }
        public string type { get; set; }
        public string ogTitleProperty { get; set; }
        public string ogDescriptionProperty { get; set; }
        public string ogImageProperty { get; set; }

    }
}