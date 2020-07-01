# Scanners

* [Overview](#Overview)
* [Shared Configuration](#Shared-Configuration)
  * [Configuring neoconfig.json](#Configuring-neoconfig.json)
* [Available Scanners](#Available-Scanners)
* [Scanners Under Development](#Scanners-Under-Development)

## Overview
A BirdsNest scanner is responsible for querying an external system, and tranlating the data into a format that is then ingested into the BirdsNest database.


## Shared Configuration
Each scanner will reference two configuration files:
1. The neo4j database connection config file
2. The scanner specific config file

The neo4j configuration file is the same format as the [neo4j Database Connection](/documentation/install/README.md#neo4j-Database-Connection) section of the install documentation. 

```
{
  "DB_URI": "bolt://localhost:7687",
  "DB_Username": "svc_birdsnest",
  "DB_Password": "my_secret_db_password",
  "DB_Timeout": 15
}
```

The scanner specific configuration will be covered in the documentation for that scanner.  

**neoconfig.json** is shared across multiple scanners, and configures access to write data to the neo4j database used by BirdsNest. 


### Configuring neoconfig.json

An example neoconfig.json file will already exist in the Scanners folder and consists of four fields:

**DB_URI** (required) - This is the path to the neo4j database. This normally running on the BirdsNest server which also runs the scanners, so the default is usually fine. The bolt protocol running on port 7687 is the default neo4j connection protocol.\
**DB_Username** (required) - The neo4j username for accessing the database\
**DB_Password** (required) -  The neo4j password for accessing the database\
**DB_Timeout** (optional) - The neo4j connection timeout in seconds

```json
{
  "DB_URI": "bolt://localhost:7687",
  "DB_Username": "svc_birdsnest",
  "DB_Password": "my_secret_db_password",
  "DB_Timeout": 15
}
```

## Available Scanners

* [Active Directory](/documentation/scanners/active-directory/README.md)
* [File System](/documentation/scanners/file-system/README.md)

## Scanners Under Development

The following scanners are currently under development:

* [AzureAD](/documentation/scanners/azuread/README.md)
* [Custom Data Importer](/documentation/scanners/custom-importer/README.md)
* [ConfigMgr/SCCM](/documentation/scanners/configmgr/README.md)
* [VMWare UEM](/documentation/scanners/uem/README.md)
