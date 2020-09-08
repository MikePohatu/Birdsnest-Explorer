# Scanners

* [Scanners](#scanners)
  * [Overview](#overview)
  * [Shared Configuration](#shared-configuration)
    * [Configuring neoconfig.json](#configuring-neoconfigjson)
  * [Available Scanners](#available-scanners)
  * [Scanners Under Development](#scanners-under-development)

## Overview
A Birdsnest Explorer scanner is responsible for querying an external system, and tranlating the data into a format that is then ingested into the Birdsnest Explorer database.


## Shared Configuration
Each scanner will reference two configuration files:
1. The neo4j database connection config file
2. The scanner specific config file

The neo4j configuration file is the same format as the [neo4j Database Connection](/documentation/install/README.md#neo4j-Database-Connection) section of the install documentation. 

```json
{
  "dbURI": "bolt://localhost:7687",
  "dbUsername": "svc_birdsnest",
  "dbPassword": "my_secret_db_password",
  "dbTimeout": 15
}
```

The scanner specific configuration will be covered in the documentation for that scanner.  

**neoconfig.json** is shared across multiple scanners, and configures access to write data to the neo4j database used by Birdsnest Explorer. 


### Configuring neoconfig.json

An example neoconfig.json file will already exist in the Scanners folder and consists of four fields:

**dbURI** (required) - This is the path to the neo4j database. This normally running on the Birdsnest Explorer server which also runs the scanners, so the default is usually fine. The bolt protocol running on port 7687 is the default neo4j connection protocol.\
**dbUsername** (required) - The neo4j username for accessing the database\
**dbPassword** (required) -  The neo4j password for accessing the database\
**dbTimeout** (optional) - The neo4j connection timeout in seconds

```json
{
  "dbURI": "bolt://localhost:7687",
  "dbUsername": "svc_birdsnest",
  "dbPassword": "my_secret_db_password",
  "dbTimeout": 15
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
