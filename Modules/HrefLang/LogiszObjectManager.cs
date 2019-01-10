using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Localization.UrlLocalizationStrategies;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.News;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web;
using Telerik.Sitefinity.Web.Events;

namespace SitefinityWebApp.Logisz.Managers
{
    public class LogiszObjectManager
    {
        private PageManager pageManager;
        private DynamicModuleManager dynamicModuleManager;

        public List<Type> ContentTypes { get; set; }


        private LogiszObjectManager()
        {
            dynamicModuleManager = DynamicModuleManager.GetManager();
        }

        /// <summary>
        /// Gets a item by type
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns></returns>
        public object GetDataItemByUrlName(string url, List<Type> AllowedContentTypes)
        {
            url = System.Net.WebUtility.UrlDecode(url);

            object o = null;


            #region SF Content
            //Default SF content

            NewsManager newsManager = NewsManager.GetManager();
            List<Telerik.Sitefinity.News.Model.NewsItem> items =
                newsManager.GetNewsItems().Where(i => i.Status == ContentLifecycleStatus.Live).ToList();

            o = items.FirstOrDefault(q => q.UrlName == url);
            if (o != null)
                return o;

            #endregion


            #region Dynamic content

            foreach (Type cType in AllowedContentTypes)
            {
                try
                {
                    o = dynamicModuleManager.GetDataItems(cType).FirstOrDefault(q => q.UrlName == url);
                    if (o != null)
                        return o;
                }
                catch { }

            }


            #endregion


            return null;
        }


        #region Type handling

        /// <summary>
        /// Gets all content types of type T.
        /// </summary>
        /// <typeparam name="T">The required base type</typeparam>
        /// <returns>List<Type></returns>
        public List<Type> GetTypes<T>()
        {
            var manager = MetadataManager.GetManager();

            List<MetaType> types = manager.GetMetaTypes().ToList();

            List<Type> allTypes = new List<Type>();

            foreach (MetaType type in types)
            {
                string fullTypeName = type.FullTypeName;

                try
                {
                    Type t = TypeResolutionService.ResolveType(fullTypeName);
                    if (t.BaseType is T || t.BaseType == typeof(T))
                    {
                        allTypes.Add(t);
                    }
                }
                catch { }
            }

            return allTypes;
        }

        #endregion

        #region Singleton

        private static LogiszObjectManager instance;
        public static LogiszObjectManager GetManager()
        {
            instance = instance ?? new LogiszObjectManager();
            return instance;
        }

        #endregion


    }
}