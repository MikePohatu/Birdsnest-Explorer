using System;
using System.Collections.Generic;
using System.Text;
using common;
using Neo4j.Driver.V1;

namespace CustomImporter
{
    public static class Processor
    {
        public static void ProcessCustomNode(CustomNode node, IDriver driver, string scannerid, string scanid)
        {
            object prop;
            if (node.Properties.TryGetValue(node.PrimaryProperty, out prop) == false)
            {
                Console.WriteLine("Primary property does not have a value");
                Environment.Exit(10);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"MERGE (n:{node.PrimaryType} {{{node.PrimaryProperty}:\"{prop}\"}})");

            if (node.Types.Count > 1)
            {
                foreach (string type in node.Types)
                {
                    if (type != node.PrimaryType)
                    {
                        builder.AppendLine($"SET n:{type}");
                    }
                }
            }

            foreach (string key in node.Properties.Keys)
            {
                object o = node.Properties[key];
                string s = o as string;
                if (s == null)
                {
                    builder.AppendLine($"SET n.{key}={o}");
                }
                else
                {
                    builder.AppendLine($"SET n.{key}=\"{s}\"");
                }

            }
            builder.AppendLine("RETURN n");

            string query = builder.ToString();
            NeoQueryData data = new NeoQueryData();
            data.ScanID = scanid;
            data.ScannerID = scannerid;
            data.Properties = new List<object> { node };

            using (ISession session = driver.Session())
            {
                NeoWriter.RunQuery(query, data, driver, true);
            }
        }

        public static string GetExistingNodeQuery(ExistingNode node, string nodeprefix)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"MATCH ({nodeprefix}:{node.Type}) ");

            if (node.Matches.Count > 0)
            {
                List<string> matchclaues = new List<string>();

                foreach (string key in node.Matches.Keys)
                {
                    string valParam;
                    object val = node.Matches[key];
                    if (val is string) { valParam = "\"" + val + "\""; }
                    else { valParam = val.ToString(); }

                    matchclaues.Add($"{nodeprefix}.{key}={valParam} ");
                }

                builder.Append("WHERE " + String.Join(" AND ", matchclaues));
            }

            return builder.ToString();
        }

        public static void ProcessCustomEdge(CustomEdge edge, IDriver driver, string scannerid, string scanid)
        {

        }
    }
}
