using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.System.Dependency;
using SitefinityWebApp.Logisz.Modules.Config;
using System;
using System.Collections.Generic;

namespace SitefinityWebApp.Logisz.Modules.Yoast.Models
{
    public class ValidationResult
    {
        public PageValidationResult pageValidationResult { get; set; }

        public YoastConfig config { get { return LogiszDependencyContainer.Resolve<ILogiszConfigManager>().GetConfig().Modules.Yoast; } }

        public double Score { get; set; }

        /// <summary>
        /// Auto initialize model
        /// </summary>
        public ValidationResult()
        {
            pageValidationResult = new PageValidationResult();
            Score = 1;
        }

    }
}