using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.Configurations.Config;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Core.Inharitances;
using SitefinityWebApp.Logisz.Modules.Yoast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitefinityWebApp.Logisz.Modules.Yoast
{
    public class YoastCalculationService
    {
        #region Attributes

        private readonly ILogiszConfigManager _logiszConfigManager;
        private LogiszConfig config { get; set; }

        #endregion

        #region Constructor

        public YoastCalculationService(ILogiszConfigManager logiszConfigManager)
        {
            this._logiszConfigManager = logiszConfigManager;
            this.config = _logiszConfigManager.GetConfig();
        }

        #endregion

        /// <summary>
        /// Gets the SEO score of a page based on the validationresults
        /// </summary>
        /// <param name="seoResult">The seo result</param>
        /// <returns>The score</returns>
        public double GetPageSEOScore(ValidationResult seoResult)
        {
            double Score = 0;
            List<ValidationProperty> toValidate = new List<ValidationProperty>();
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInTitle);
            toValidate.Add(seoResult.pageValidationResult.KeywordLengthIsValid);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInHeader);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInUrl);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInMetaTitle);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInMetaDescription);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInArticleHeading);
            toValidate.Add(seoResult.pageValidationResult.KeywordIsInContent);

            //Instant 1!
            if (!seoResult.pageValidationResult.MetaTagsAreValid.Valid)
                Score = 1;
            if (String.IsNullOrEmpty(seoResult.pageValidationResult.Keyword))
                Score = 1;

            int items = toValidate.Count;
            double scorePerElement = (double)((double)config.Modules.Yoast.MaxScore / (double)items);

            foreach (ValidationProperty element in toValidate)
            {
                if (element.Valid)
                    Score += scorePerElement;
            }

            return Score;
        }
    }
}