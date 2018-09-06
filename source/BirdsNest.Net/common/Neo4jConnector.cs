using System;
using Neo4j.Driver.V1;

namespace common
{
    public static class Neo4jConnector
    {
        public static IDriver ConnectToNeo(NeoConfiguration config)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(config.DB_Password) || string.IsNullOrWhiteSpace(config.DB_Username))
                { return GraphDatabase.Driver(config.DB_URI); }
                else
                { return GraphDatabase.Driver(config.DB_URI, AuthTokens.Basic(config.DB_Username, config.DB_Password)); }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to Neo4j: " + e.Message);
                Environment.Exit(1002);
            }
            return null;
        }

        public static IDriver ConnectToNeo(NeoConfiguration config, Config neo4jconfig)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(config.DB_Password) || string.IsNullOrWhiteSpace(config.DB_Username))
                { return GraphDatabase.Driver(config.DB_URI,neo4jconfig); }
                else
                { return GraphDatabase.Driver(config.DB_URI, AuthTokens.Basic(config.DB_Username, config.DB_Password), neo4jconfig); }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to Neo4j: " + e.Message);
                Environment.Exit(1002);
            }
            return null;
        }
    }
}
