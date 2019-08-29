using System.Collections.Generic;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchTokens
    {
        private int _tokencount = 0;
        private int _valuecount = 0;
        private Dictionary<string, string> _valuetokens = new Dictionary<string, string>();

        //public string FieldName { get; set; } = "props";
        public Dictionary<string, string> NameTokens { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Get a token name for a given string. If the string already has a token then return it, otherwise 
        /// return a new token
        /// </summary>
        /// <param name="currentname"></param>
        /// <returns></returns>
        public string GetNameToken(string currentname)
        {
            string token;
            if (this.NameTokens.TryGetValue(currentname, out token))
            {
                return token;
            }
            this._tokencount++;
            token = "token" + this._tokencount;
            this.NameTokens.Add(currentname, token);
            return token;
        }

        /// <summary>
        /// Get a token name for a given object. If the object already has a token then return it, otherwise 
        /// return a new token
        /// </summary>
        /// <param name="currentvalue"></param>
        /// <returns></returns>
        public string GetValueToken(object currentvalue)
        {
            string token;
            string val = currentvalue.ToString();
            if (!this._valuetokens.TryGetValue(val, out token))
            {
                this._valuecount++;
                token = "val" + this._valuecount;
                this._valuetokens.Add(val, token);
                this.Properties.Add(token, currentvalue);
            }

            return "$" + token;
        }
    }
}
