namespace SitefinityWebApp.Logisz.Modules.Yoast.Models
{
    public class ValidationProperty
    {
        public ValidationProperty()
        {
            CssClass = "LogiszDebugWarning";
            Message = "Unchecked";
            Valid = false;
        }

        /// <summary>
        /// Css class which is used in the validation result
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// The message which is used in the validation result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// If this is valid
        /// </summary>
        public bool Valid { get; set; }
    }
}