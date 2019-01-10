using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using SitefinityWebApp.Logisz.Modules.Yoast.Models;
using Telerik.Sitefinity.Modules.GenericContent.Web.UI;
using System.Text.RegularExpressions;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Modules.Yoast;
using Telerik.Sitefinity.Web;
using SitefinityWebApp.Logisz.Core.Configurations;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Model;

namespace SitefinityWebApp.Logisz.Modules
{
    public class PageValidationMethods
    {
        #region Attributes

        private static readonly string defaultValidMessage = "Yes";
        private static readonly string defaultInvalidMessage = "No";
        private static readonly string defaultValidCssClass = "LogiszDebugValidText";
        private static readonly string defaultInvalidCssClass = "LogiszDebugInvalidText";
        private static readonly string defaultWarningCssClass = "LogiszDebugWarning";
        private LogiszConfig config { get; set; }

        private readonly ILogiszConfigManager _logiszConfigManager;

        #endregion

        #region Constructor

        public PageValidationMethods(ILogiszConfigManager logiszConfigManager) {
            this._logiszConfigManager = logiszConfigManager;
            this.config = _logiszConfigManager.GetConfig();
        }

        #endregion

        #region Public methods


        /// <summary>
        /// Core validation function
        /// </summary>
        /// <param name="page"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public PageValidationResult ValidatePage(PageSiteNode node, Page page)
        {

            PageValidationResult res = new PageValidationResult();

            res.Keyword = GetPageKeyword(node);
            res.PageSiteNode = node;
            res.PageNode = GetPageNode(node);
            res.Page = page;

            //Validate for empty keyword, if empty. IT's added and we return!
            res = ValidateKeywordEmpty(res);
            if (res.FatalError)
                return res;

            res = CheckForbiddenMetaTags(res);

            ///Calling the validation methods
            res = KeywordIsInPageTitle(res);
            res = ValidateKeywordCharacters(res);
            res = KeywordIsInHeader(res);
            res = KeywordIsInUrl(res);
            res = KeywordIsInMetaTitle(res);
            res = KeywordIsInMetaDescription(res);
            res = CheckContentHeaders(res);

            return res;

        }

        #endregion

        #region Privates

        /// <summary>
        /// Check if page title contains the keyword
        /// </summary>
        /// <param name="page"></param>
        private PageValidationResult KeywordIsInPageTitle(PageValidationResult res)
        {
            if (res.Page.Title.ToLower().Contains(res.Keyword.ToLower()))
            {
                res.KeywordIsInTitle.Valid = true;
                res.KeywordIsInTitle.Message = defaultValidMessage;
                res.KeywordIsInTitle.CssClass = defaultValidCssClass;
            } else
            {
                res.KeywordIsInTitle.Message = defaultInvalidMessage;
                res.KeywordIsInTitle.CssClass = defaultInvalidCssClass;
            }

            return res;
        }

        /// <summary>
        /// Check if the keyword is in the header
        /// </summary>
        private PageValidationResult KeywordIsInHeader(PageValidationResult res)
        {
            List<ContentBlock> blocks = res.Page.Controls.OfType<ContentBlock>().Cast<ContentBlock>().ToList();

            foreach(ContentBlock block in blocks)
            {
                string html = block.Html;
            }
            return res;
        }

        /// <summary>
        /// Check if the keyword is in the url
        /// </summary>
        private PageValidationResult KeywordIsInUrl(PageValidationResult res)
        {
            //Check
            if(res.Page.Request.Url.AbsoluteUri.ToLower().Contains(res.Keyword.ToLower().Trim())) { 
                res.KeywordIsInUrl.Valid = true;
                res.KeywordIsInUrl.Message = defaultValidMessage;
                res.KeywordIsInUrl.CssClass = defaultValidCssClass;
            } else
            {
                res.KeywordIsInUrl.Message = defaultInvalidMessage;
                res.KeywordIsInUrl.CssClass = defaultInvalidCssClass;
            }
            return res;
        }

        /// <summary>
        /// Check if the keyword is in the meta description
        /// </summary>
        private PageValidationResult KeywordIsInMetaDescription(PageValidationResult res)
        {
            if(String.IsNullOrEmpty(res.Page.MetaDescription))
            {
                res.KeywordIsInMetaDescription.Valid = false;
                res.KeywordIsInMetaDescription.Message = "Is not set!";
                res.KeywordIsInMetaDescription.CssClass = defaultInvalidCssClass;
                return res;
            }

            //Check
            if (res.Page.MetaDescription.ToLower().Contains(res.Keyword.ToLower()))
            {
                res.KeywordIsInMetaDescription.Valid = true;
                res.KeywordIsInMetaDescription.Message = defaultValidMessage;
                res.KeywordIsInMetaDescription.CssClass = defaultValidCssClass;
            }
            else
            {
                res.KeywordIsInMetaDescription.Message = defaultInvalidMessage;
                res.KeywordIsInMetaDescription.CssClass = defaultInvalidCssClass;
            }

            return res;
        }

        /// <summary>
        /// Check if the keyword is in the title
        /// </summary>
        private PageValidationResult KeywordIsInMetaTitle(PageValidationResult res)
        {
            if(String.IsNullOrEmpty(res.Page.MetaKeywords))
            {
                res.KeywordIsInMetaTitle.Valid = false;
                res.KeywordIsInMetaTitle.Message = "Is not set!";
                res.KeywordIsInMetaTitle.CssClass = defaultInvalidCssClass;
                return res;
            }

            //Check
            if (res.Page.MetaDescription.ToLower().Contains(res.Keyword.ToLower()))
            {
                res.KeywordIsInMetaTitle.Valid = true;
                res.KeywordIsInMetaTitle.Message = defaultValidMessage;
                res.KeywordIsInMetaTitle.CssClass = defaultValidCssClass;
            }
            else
            {
                res.KeywordIsInMetaTitle.Message = defaultInvalidMessage;
                res.KeywordIsInMetaTitle.CssClass = defaultInvalidCssClass;
            }

            return res;
        }

        /// <summary>
        /// Checks if the meta tags contain unallowed meta tags.
        /// </summary>
        /// <param name="page"></param>
        private PageValidationResult CheckForbiddenMetaTags(PageValidationResult res)
        {
            res.MetaTagsAreValid.Valid = true;
            //Loop controls
            foreach (Control header in res.Page.Header.Controls)
            {
                Type headerType = header.GetType();

                if (headerType == typeof(System.Web.UI.HtmlControls.HtmlMeta))
                {
                    System.Web.UI.HtmlControls.HtmlMeta control = (System.Web.UI.HtmlControls.HtmlMeta)header;

                    string TagName = control.Name;
                    if (config.Modules.Yoast.MetaTagIsUnallowed(TagName.ToLower()))
                    {
                        res.MetaTagsAreValid.Valid = false;
                        res.MetaTagsAreValid.Message = TagName + " meta tag found, this is unallowed!";
                        res.MetaTagsAreValid.CssClass = defaultInvalidCssClass;
                        res.Fail(res.MetaTagsAreValid.Message);
                        return res;
                    }
                } else
                {
                    if(headerType == typeof(System.Web.UI.LiteralControl))
                    {
                        System.Web.UI.LiteralControl control = (LiteralControl)header;

                        //Match paterns to find out with tokens are used
                        Regex fullNameRegex = new Regex("name=\"(.+?)\"");
                        Regex nameRegex = new Regex("\"(.+?)\"");
                        Regex contentRegex = new Regex("content=\"(.+?)\"");
                        Regex metaTags = new Regex(@"<meta(.+?)>");
                        foreach (Match metaTag in metaTags.Matches(control.Text))
                        {
                            string fullName = fullNameRegex.Match(metaTag.Value).Value;
                            string name = nameRegex.Match(fullName).Value;
                            name = name.Replace("\"", "");
                            string content = contentRegex.Match(metaTag.Value).Value;

                            if(config.Modules.Yoast.MetaTagIsUnallowed(name.ToLower()))
                            {
                                res.MetaTagsAreValid.Valid = false;
                                res.MetaTagsAreValid.Message = name + " meta tag found, this is unallowed!";
                                res.MetaTagsAreValid.CssClass = defaultInvalidCssClass;
                                res.Fail(res.MetaTagsAreValid.Message);
                                return res;
                            }

                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Validate if we have to many characters
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private PageValidationResult ValidateKeywordCharacters(PageValidationResult res)
        {
            string value = "Your keyword is {0} characters long. This is good!";

            int recommandedMinimumLength = config.Modules.Yoast.MinNumberOfCharactersForKeyword;
            int recommandedMaximumLength = config.Modules.Yoast.MaxNumberOfCharactersForKeyword;

            res.KeywordLengthIsValid.Valid = true;
            res.KeywordLengthIsValid.CssClass = defaultValidCssClass;

            if (res.Keyword.Count() < recommandedMinimumLength)
            {
                res.KeywordLengthIsValid.Valid = false;
                res.KeywordLengthIsValid.CssClass = defaultWarningCssClass;
                value = "Your keyword is {0} characters long. This is too short, its recommanded to get a keyword between {1} and {2} characters.";
            }
            else if (res.Keyword.Count() > recommandedMaximumLength)
            {
                res.KeywordLengthIsValid.Valid = false;
                res.KeywordLengthIsValid.CssClass = defaultWarningCssClass;
                value = "Your keyword is {0} characters long. This is too long, its recommanded to get a keyword between {1} and {2} characters.";
            }

            value = String.Format(value, res.Keyword.Count(), recommandedMinimumLength, recommandedMaximumLength);
            res.KeywordLengthIsValid.Message = value;

            return res;
        }

        /// <summary>
        /// Check if keyword is empty
        /// </summary>
        private PageValidationResult ValidateKeywordEmpty(PageValidationResult res)
        {

            if (String.IsNullOrEmpty(res.Keyword))
            {
                res.Fail("No keyword set");
            }

            return res;
        }

        private PageValidationResult CheckContentHeaders(PageValidationResult res)
        {
            return res;
        }

        /// <summary>
        /// Gets page node
        /// </summary>
        /// <param name="pageSiteNode">Page site node</param>
        /// <returns>PageNode</returns>
        private PageNode GetPageNode(PageSiteNode pageSiteNode)
        {
            PageManager pageManager = PageManager.GetManager();

            PageNode pageNode = pageManager.GetPageNode(pageSiteNode.Id);

            return pageNode;
        }

        /// <summary>
        /// Gets the page keyword
        /// </summary>
        /// <returns>Keyword</returns>
        private string GetPageKeyword(PageSiteNode pageSiteNode)
        {
            //Receive page data
            PageNode pageNode = GetPageNode(pageSiteNode);
            if (pageNode == null)
                return String.Empty;

            string keyword = GetPageKeyword(pageNode);

            return keyword;
        }
        
        /// <summary>
        /// Gets the keyword
        /// </summary>
        /// <param name="page">From page</param>
        /// <returns>string keyword</returns>
        private string GetPageKeyword(PageNode page) {
            //Check for SEO keyword field
            string keywordFieldName = config.Modules.Yoast.KeywordFieldName;

            try
            {
                object keywordObject = page.GetValue(keywordFieldName);
                return keywordObject == null ? "" : keywordObject.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        #endregion

    }
}