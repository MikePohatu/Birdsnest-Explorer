using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class Property
    {
        public string Description { get; set; } = string.Empty;

        private string _type = "string";
        public string Type
        {
            get { return this._type; }
            set
            {
                if (value == "string" || value == "number" || value == "boolean")
                {
                    this._type = value;
                }
                else
                {
                    throw new ArgumentException("Invalid property type: " + value);
                }
            }
        }
    }
}
