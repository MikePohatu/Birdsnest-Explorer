* **neoconfig.json** is shared across multiple scanners, and configures access to write data to the neo4j database used by BirdsNest. 


### Configuring neoconfig.json
An example neoconfig.json file will already exist in the Scanners folder and consists of three fields:

```json
{
	"DB_URI": "bolt://localhost:7687",
	"DB_Username": "svc_neo4j",
	"DB_Password": "P@ssword1"
}
```

**DB_URI** (required) - This is the path to the neo4j database. This normally running on the BirdsNest server which also runs the scanners, so the default is usually fine. The bolt protocol running on port 7687 is the default neo4j connection protocol.

**DB_Username** (required) - The neo4j username for accessing the database (created during setup)

**DB_Password** (required) -  The neo4j password for accessing the database (created during setup)