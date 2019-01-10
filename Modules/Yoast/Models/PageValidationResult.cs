using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Web;
using System.Web.UI;


namespace SitefinityWebApp.Logisz.Modules.Yoast.Models
{
    public class PageValidationResult
    {
        public ValidationProperty KeywordIsInTitle { get; set; }
        public ValidationProperty KeywordLengthIsValid { get; set; }
        public ValidationProperty KeywordIsInHeader { get; set; }
        public ValidationProperty KeywordIsInUrl { get; set; }
        public ValidationProperty KeywordIsInMetaTitle { get; set; }
        public ValidationProperty KeywordIsInMetaDescription { get; set; }
        public ValidationProperty KeywordIsInArticleHeading { get; set; }
        public ValidationProperty KeywordIsInContent { get; set; }
        public ValidationProperty MetaTagsAreValid { get; set; }
        public string Keyword { get; set; }
        public bool FatalError { get; private set; }
        public string FatalErrorMessage { get; set; }
        public PageNode PageNode { get; set; }
        public Page Page { get; set; }
        public PageSiteNode PageSiteNode { get; set; }

        /// <summary>
        /// Initializer
        /// </summary>
        public PageValidationResult()
        {
            FatalError = false;
            KeywordIsInHeader = new ValidationProperty();
            KeywordLengthIsValid = new ValidationProperty();
            KeywordIsInTitle = new ValidationProperty();
            KeywordIsInUrl = new ValidationProperty();
            KeywordIsInMetaTitle = new ValidationProperty();
            KeywordIsInMetaDescription = new ValidationProperty();
            KeywordIsInArticleHeading = new ValidationProperty();
            KeywordIsInContent = new ValidationProperty();
            MetaTagsAreValid = new ValidationProperty();
        }

        /// <summary>
        /// Upon failure
        /// </summary>
        /// <param name="message"></param>
        public void Fail(string message)
        {
            FatalError = true;
            FatalErrorMessage = message;
        }
    }
}