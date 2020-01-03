using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

namespace CMScanner.CmConverter
{
    public class Connector
    {
        public WqlConnectionManager Connection { get; private set; }


        private static Connector _instance;

        private Connector()
        {
            this.Connection = new WqlConnectionManager();
        }

        public static Connector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Connector();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Connect using the current application user context
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool Connect(string server)
        {
            try { this.Connection.Connect(server); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Connect using the specified connection credentials
        /// </summary>
        /// <param name="authuser"></param>
        /// <param name="authpw"></param>
        /// <param name="authdomain"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool Connect(string authuser, string authpw, string authdomain, string server)
        {
            try { this.Connection.Connect(server, authdomain + "\\" + authuser, authpw); }
            catch { return false; }
            return true;
        }
    }
}
