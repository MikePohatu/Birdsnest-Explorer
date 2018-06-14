using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4j.Driver.V1;

namespace neo4jlink
{
    public static class Writer
    {
        public static void CreateNode(INode node, IDriver driver)
        {
            string query = "CREATE(n: Person { name: 'Andres', title: 'Developer' })";

            using (var session = driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run(query);
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(greeting);
            }
        }
    }
}
