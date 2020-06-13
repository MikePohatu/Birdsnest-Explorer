#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using Neo4j.Driver.V1;

namespace Console.neo4jProxy
{
    public static class Neo4jConnector
    {
        public static IDriver ConnectToNeo(NeoConfiguration config)
        {
            try
            {
                var dbconfig = new Config { ConnectionTimeout = TimeSpan.FromSeconds(config.DB_Timeout) };
                if (string.IsNullOrWhiteSpace(config.DB_Password) || string.IsNullOrWhiteSpace(config.DB_Username))
                { return GraphDatabase.Driver(config.DB_URI, dbconfig); }
                else
                { return GraphDatabase.Driver(config.DB_URI, AuthTokens.Basic(config.DB_Username, config.DB_Password), dbconfig); }

            }
            catch (Exception e)
            {
                //Console.WriteLine("Error connecting to Neo4j: " + e.Message);
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
                //Console.WriteLine("Error connecting to Neo4j: " + e.Message);
                Environment.Exit(1002);
            }
            return null;
        }
    }
}
