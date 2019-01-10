using SitefinityWebApp.Logisz.Core.Configurations.Config;
using SitefinityWebApp.Logisz.Core.Configurations.Config.Shared;
using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class YoastConfig : LogiszModuleConfigElement
    {
        public YoastConfig(ConfigElement parent) : base(parent) { }

        [ObjectInfo(Title = "Keyword fieldname", Description = "Name of the custom page field")]
        [ConfigurationProperty("KeywordFieldName", DefaultValue = "LgszSeoKeyword", IsRequired = true)]
        public string KeywordFieldName
        {
            get { return (string)this["KeywordFieldName"]; }
            set { this["KeywordFieldName"] = value; }
        }

        [ObjectInfo(Title = "Minimum keyword length", Description = "Minimum length of characters for the keyword")]
        [ConfigurationProperty("MinNumberOfCharactersForKeyword", DefaultValue = 6, IsRequired = true)]
        public int MinNumberOfCharactersForKeyword
        {
            get { return (int)this["MinNumberOfCharactersForKeyword"]; }
            set { this["MinNumberOfCharactersForKeyword"] = value; }
        }

        [ObjectInfo(Title = "Maximum keyword length", Description = "Maximum length of characters for the keyword")]
        [ConfigurationProperty("MaxNumberOfCharactersForKeyword", DefaultValue = 12, IsRequired = true)]
        public int MaxNumberOfCharactersForKeyword
        {
            get { return (int)this["MaxNumberOfCharactersForKeyword"]; }
            set { this["MaxNumberOfCharactersForKeyword"] = value; }
        }

        [ObjectInfo(Title = "Unallowed metatag names", Description = "Tagnames which are unallowed")]
        [ConfigurationProperty("UnallowedMetaTagNames")]
        public ConfigElementDictionary<string, LogiszSingleKeyElement> UnallowedMetaTagNames
        {
            get
            {
                return (ConfigElementDictionary<string, LogiszSingleKeyElement>)this["UnallowedMetaTagNames"];
            }
        }

        [ObjectInfo(Title = "Maximum SEO score", Description = "Max score which can be reached for a SEO")]
        [ConfigurationProperty("MaxScore", DefaultValue = 10, IsRequired = true)]
        public int MaxScore
        {
            get { return (int)this["MaxScore"]; }
            set { this["MaxScore"] = value; }
        }

        #region Methods
        /// <summary>
        /// Checks if the metatag is unallowed
        /// </summary>
        /// <param name="metaTag">The tagname</param>
        /// <returns>boolean</returns>
        public bool MetaTagIsUnallowed(string metaTag)
        {
            bool unallowed = this.UnallowedMetaTagNames.ContainsKey(metaTag);

            return unallowed;
        }
        #endregion

    }
}