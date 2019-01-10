using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using SitefinityWebApp.Logisz.Core.Configurations;
using SitefinityWebApp.Logisz.Core.Extensions.Security;
using SitefinityWebApp.Logisz.Core.System.Dependency;

namespace SitefinityWebApp.Logisz.Modules
{
    public class TokenizedStream : Stream
    {
        private Stream _stream;
        private long _length;
        private long _position;
        private readonly ILogiszConfigManager _logiszConfigManager;
        private LogiszConfig config;
        public string output = String.Empty;

        public TokenizedStream(Stream stream)
        {
            this._logiszConfigManager = LogiszDependencyContainer.Resolve<ILogiszConfigManager>();
            this.config = _logiszConfigManager.GetConfig();
            _stream = stream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            bool isDeveloper = LogiszDependencyContainer.Resolve<ILogiszUserManager>().GetLoggedOnUser().IsDeveloper;

            Encoding encoding = HttpContext.Current.Response.ContentEncoding;
            string output = encoding.GetString(buffer);

            var tokens = new List<string>();

            string seperatorOpeningTag = config.Modules.Shortcoder.SeperatorOpeningTag;
            string seperatorCloseTag = config.Modules.Shortcoder.SeperatorCloseTag;

            //Match paterns to find out with tokens are used
            Regex regex = new Regex(@""+ seperatorOpeningTag + "(.+?)"+ seperatorCloseTag);
            foreach (Match match in regex.Matches(output))
            {
                tokens.Add(match.Groups[0].Value);
            }

            if (tokens.Count > 0)
            {

                var modified = false;

                for (var i = 0; i < tokens.Count; i++)
                {
                    //Get manager & Type
                    DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();
                    Type type = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Shortcoder.Shortcode");

                    //Replace brackets
                    string token = tokens[i];
                    token = token.Replace(seperatorOpeningTag, string.Empty);
                    token = token.Replace(seperatorCloseTag, string.Empty);

                    //Get token based on token key
                    DynamicContent dynamicModuleToken = dynamicModuleManager.GetDataItems(type).FirstOrDefault(t => t.GetValue<string>("key") == token);


                    if (dynamicModuleToken != null)
                    {
                        string value = dynamicModuleToken.GetValue<Lstring>("value");

                        if (config.Modules.Shortcoder.Debug && isDeveloper)
                        {

                            value = DebugValue(value, token);
                        }


                        output = output.Replace(tokens[i], value);
                        modified = true;
                    }
                }

                //Change buffer
                if (modified)
                {
                    buffer = Encoding.ASCII.GetBytes(output);
                }
            }

            this.output += output;
            _stream.Write(buffer, offset, buffer.Length);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ajust value by debug
        /// </summary>
        /// <param name="value">The current value</param>
        /// <returns>string</returns>
        private string DebugValue(string value, string key)
        {
            return config.Modules.Shortcoder.SeperatorOpeningTag + key + config.Modules.Shortcoder.SeperatorCloseTag + "<span class='lgszshortcoder_highlight'>" + value + "</span>";
        }
    }
}