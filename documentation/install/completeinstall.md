# Birdsnest Explorer Installation

* [Birdsnest Explorer Installation](#birdsnest-explorer-installation)
  * [Architecture](#architecture)
  * [System Requirements](#system-requirements)
  * [Dependencies](#dependencies)
  * [Database](#database)
    * [Account Setup](#account-setup)
    * [Database Password Change](#database-password-change)
    * [Logout Current User](#logout-current-user)
    * [DB Configuration and Importing Default Data](#db-configuration-and-importing-default-data)
  * [Firewall](#firewall)
    * [Inbound](#inbound)
    * [Outbound](#outbound)
  * [Anti-Virus Exclusions](#anti-virus-exclusions)
  * [Console](#console)
    * [Authorization](#authorization)
      * [Active Directory vs LDAP](#active-directory-vs-ldap)
      * [Active Directory](#active-directory)
      * [LDAP](#ldap)
      * [LocalServer](#localserver)
      * [Neo4j Database Connection](#neo4j-database-connection)
      * [Full Configuration File](#full-configuration-file)
  * [Scanners](#scanners)

## Architecture

The following diagram outlines the components and their interaction with each other and external systems. The Active Directory and file system scanners are shown as examples, but additional scanners will access other external systems as needed.

![Architecture-Diagram](/documentation/image/architecture.png)

## System Requirements

* 2 core CPU
* 8GB RAM (minimum 16GB recommended)
* 10GB disk space
* Windows Server 2016 or newer (Nano & Hyper-V versions not supported)

## Dependencies

The following dependencies are required to run Birdsnest Explorer. The managed installer provides automated install of these components in addition to Birdsnest Explorer itself.

* Web Server Role (IIS) installed
* Java 1.8
* .Net Core Server Hosting Runtime 3.1.6

Additionally, a modern browser is required for use with the Neo4j web console. Neo4j provides an article [here](https://neo4j.com/developer/kb/explanation-of-error-security-error-18-when-using-internet-explorer-and-neo4j-browser/) that outlines changes to make IE11 work, but this has shown to be problematic in our testing. Microsoft Edge or Chrome are recommended.

## Database

Birdsnest Explorer uses a [Neo4j](https://neo4j.com/) graph database at its core. Neo4j 3.5 is installed and configured as part of the managed install, and default data imported i.e. Windows builtin groups/users definitions.

Full documentation for Neo4j is available from the [Neo4j documentation site](https://neo4j.com/docs/operations-manual/3.5/).

During installation, the password for the default Neo4j database account must be set, and the details for the Birdsnest Explorer service account must be set. The Birdsnest Explorer service account will be used later during setup of the [Console](#Console) and [Scanners](#Scanners).

### Account Setup

If using the managed installer, the Neo4j and Birdsnest Explorer service account will be configured during install. For manual acccount setup you can use the process below.

To setup Neo4j accounts, on the Birdsnest Explorer server open a browser (Edge or Chrome recommneded) at <http://localhost:7474/>

If the Neo4j password was not set prior to service start, you will be prompted on first login to update the default **neo4j** user account password.

To create the Birdsnest Explorer service account, enter the following command into the command bar and click the **play** button:

```cypher
:server user add
```

![New-User](/documentation/image/new_user.png)

Enter the new credentials, clear the **Force password change** box and click **Add User**

Record the neo4j and service account credentials in a secure location.

### Database Password Change

To change the password of the currently logged in user, run the following command:

```cypher
CALL dbms.security.changePassword($new_secret_password)
```

Record the updated credentials in a secure location.

### Logout Current User

To logout the current user, run the following command:

```cypher
:server disconnect
```

_Official Neo4j documentation for user management is available [here](https://neo4j.com/docs/operations-manual/3.5/reference/user-management-community-edition/)._

### DB Configuration and Importing Default Data

Before starting data ingestion, the default data set should be imported that includes definitions of BuiltIn users and groups, as well as configuration of database indexes and constraints.

On a Windows machine the following command will import the import the .cypher file (hosted at _source\neo4j_ in the source repository):

```cmd
type "%path_to_db_file%\DB_Setup.cypher" | "%path_to_neo4j%\bin\cypher-shell" -u %neo4j_username% -p %neo4j_password% --format plain
```

## Firewall

The following firewall rules are required on the Birdsnest Explorer server. Border firewalls will also require configuration if the Birdsnest Explorer server traverses security zones. 

### Inbound

In addition to any normal admin functions e.g. RDP, ping, the following ports are required for the Birdsnest Explorer Console to function

| Service | Port | Protocol | Description |
|---------|------|----------|-------------|
| HTTPS | 443 | TCP | User access to Birdsnest Explorer web console |
| HTTP | 80 | TCP | User access to Birdsnest Explorer web console (optional redirect to HTTPS) |

### Outbound

<ins>Console</ins>

In addition to any core functionality e.g. DNS, Domain Member services etc, the following ports are required for the Birdsnest Explorer Console to function

| Destination | Service | Port | Protocol | Description |
|-------------|---------|------|----------|-------------|
| Login Domain Controllers | LDAP | 389 | TCP | Console Authentication |
| Login Domain Controllers | LDAPS | 636 | TCP | Console Authentication |

<ins>Scanners</ins>

In addition to any core functionality e.g. DNS, Domain Member services etc, the following ports are required for Birdsnest Explorer Scanners to function\*

| Destination | Service | Port | Protocol | Description |
|-------------|---------|------|----------|-------------|
| Scanned Domain Controllers | LDAPS | 636 | TCP | Active Directory Scanner |
| Scanned Domain Controllers | LDAP | 389 | TCP | Active Directory Scanner |
| Scanned File Servers | SMB | 139, 445 | TCP | File System Scanner |

\*Appropriate firewall rules will be need to be added for any additional scanners.

## Anti-Virus Exclusions

It is strongly recommended to exclude the Birdsnest Explorer install path e.g. C:\birdsnest

## Console

The Birdsnest Explorer console is installed to the following path.

**_$install_path\Console\\$version_**

e.g.

**_C:\birdsnest\Console\1.0_**

Inside the console install folder is the **appsettings.json** configuration file. This file must be updated to configure the connection to the database and to authentication services e.g. Active Directory.

### Authorization

The **Authorization** section includes one or more authentication providers. Authentication is done against the selected provider at each login. No credentials are cached within Birdsnest Explorer. When authentication and authorization is successful i.e. the user is a member of the configured group or user list, a session cookie is returned to the browser allowing the connection.

The authorization provider options are:

* ActiveDirectory
* LDAP
* LocalServer

The default configuration file includes a section for each provider type. Delete the sections that you do not wish to use. The order of "Authorization" sections in the config will be reflected in the order of providers listed in the BirdNest Console login page.

#### Active Directory vs LDAP

The Active Directory and LDAP providers differ in that they use different libraries within the codebase. If using Active Directory as your authentication source, Active Directory is usually the best option. The following outlines the differences between the two in practical terms:

* LDAP requires the configuration of a **bind** service account. This account is used to create the initial connection to the directory before processing the user login. ActiveDirectory doesn't need this if running on a domain joined server.
* The LDAP provider uses generic LDAP functionality, so is more suitable if configuring authentication against another directory product e.g. Open Directory.
* ActiveDirectory will only work on Windows. If creating a custom build of the Birdsnest Explorer console to work on a Unix based system, you will need to use the LDAP provider

#### Active Directory

The following fields are required in an ActiveDirectory authorization configuation. Two domain groups are required, one for Birdsnest Explorer admins, and one for normal users.

**Type**: Must be "ActiveDirectory" \
**Name**: The display name to appear in the Birdsnest Explorer Console login page\
**Domain**: Fully qualified Active Directory domain name\
**ContainerDN**: Root OU distinguished name containing users and groups authenticating to the Console.\
**SSL**: Whether to use LDAPS or not.\
**AdminGroup**: The SamAccountName of the Birdsnest Explorer admin group. Admin users must be a direct member of this group.  Nested groups are not supported.\
**UserGroup**: The SamAccountName of the Birdsnest Explorer user group. Users must be a direct member of this group. Nested groups are not supported.\
**TimeoutSeconds**: Console session timeout in seconds. This is the time between requests to the server after which the session is expired.

Example "Authorization" section for Active Directory authorization:

```json
  "Authorization": [
    {
      "Type": "ActiveDirectory",
      "Name": "AD-DisplayName",
      "Domain": "domain.local",
      "ContainerDN": "DC=domain,DC=local",
      "SSL": false,
      "AdminGroup": "Birdsnest Explorer Admins",
      "UserGroup": "Birdsnest Explorer Users",
      "TimeoutSeconds": 900
    },
    ...
  ]
```

#### LDAP

The following fields are required in an LDAP authorization configuation. Two domain groups are required, one for Birdsnest Explorer admins, and one for normal users.

**Type**: Must be "LDAP" \
**Name**: The display name to appear in the Birdsnest Explorer Console login page\
**Domain**: Fully qualified domain name\
**BindUser**: The service account user principal name used to bind to LDAP\
**BindPassword**: The service account password used to bind to LDAP\
**Server**: The server/DNS alias to connect to for LDAP\
**SearchBase**: Root OU distinguished name containing users and groups authenticating to the Console.\
**SSL**: Whether to use LDAPS or not.\
**AdminGroupDN**: The distinguished name of the Birdsnest Explorer admin group. Admin users must be a direct member of this group. Nested groups are not supported.\
**UserGroupDN**: The distinguished name of the Birdsnest Explorer user group. Users must be a direct member of this group. Nested groups are not supported.\
**TimeoutSeconds**: Console session timeout in seconds. This is the time between requests to the server after which the session is expired.


Example "Authorization" section for LDAP authorization:

```json
  "Authorization": [
    {
      "Type": "LDAP",
      "Name": "LDAP-DisplayName",
      "Domain": "domain.local",
      "BindUser": "svc_birdsnest@domain.local",
      "BindPassword": "my_secret_password",
      "Server": "domaincontroller.domain.local",
      "SearchBase": "DC=domain,DC=local",
      "SSL": false,
      "AdminGroupDN": "CN=Birdsnest Explorer Admins,OU=Groups,DC=domain,DC=local",
      "UserGroupDN": "CN=Birdsnest Explorer Users,OU=Groups,DC=domain,DC=local",
      "TimeoutSeconds": 900
    },
    ...
  ]
```

#### LocalServer

The following fields are required in an LocalServer authorization configuation. Local server authenticates against the local users of the Birdsnest Explorer server

**Type**: Must be "LocalServer" \
**Name**: The display name to appear in the Birdsnest Explorer Console login page\
**AdminUsers**: A comma separated list of usernames who are Birdsnest Explorer admins. This is a list of user accounts, not groups\
**Users**: A comma separated list of usernames who are Birdsnest Explorer users. This is a list of user accounts, not groups\
**TimeoutSeconds**: Console session timeout in seconds. This is the time between requests to the server after which the session is expired.

Example "Authorization" section for Local Server authorization:

```json
  "Authorization": [
    {
      "Type": "LocalServer",
      "Name": "Local-DisplayName",
      "AdminUsers": [
        "Birdsnest ExplorerAdmin1",
        "Birdsnest ExplorerAdmin2"
      ],
      "Users": [
        "Birdsnest ExplorerUser1",
        "Birdsnest ExplorerUser2"
      ],
      "TimeoutSeconds": 900
    },
    ...
  ]
```

#### Neo4j Database Connection

The following outlines the fields to configure the connection to the Neo4j database.

**dbURI**: The database server URI. The entry listed below is the default for the Neo4j instance installed on the Birdsnest Explorer server\
**dbUsername**: The Neo4j connection username\
**dbPassword**: The Neo4j connection password\
**dbTimeout**: The Neo4j connection timeout in seconds

```json
  "neo4jSettings": {
    "dbURI": "bolt://localhost:7687",
    "dbUsername": "svc_birdsnest_db",
    "dbPassword": "my_secret_db_password",
    "dbTimeout": 15
  }
```

#### Full Configuration File

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Authorization": [
    {
      "Type": "ActiveDirectory",
      "Name": "AD-DisplayName",
      "Domain": "domain.local",
      "ContainerDN": "DC=domain,DC=local",
      "SSL": false,
      "AdminGroup": "Birdsnest Explorer Admins",
      "UserGroup": "Birdsnest Explorer Users",
      "TimeoutSeconds": 900
    },
    {
      "Type": "LocalServer",
      "Name": "Local-DisplayName",
      "AdminUsers": ["Birdsnest ExplorerAdmin"],
      "Users": [ "Birdsnest ExplorerUser"],
      "TimeoutSeconds": 900
    },
    {
      "Type": "LDAP",
      "Name": "LDAP-DisplayName",
      "Domain": "domain.local",
      "BindUser": "svc_birdsnest@domain.local",
      "BindPassword": "my_secret_password",
      "Server": "domaincontroller.domain.local",
      "SearchBase": "DC=domain,DC=local",
      "SSL": false,
      "AdminGroupDN": "CN=Birdsnest Explorer Admins,OU=Groups,DC=domain,DC=local",
      "UserGroupDN": "CN=Birdsnest Explorer Users,OU=Groups,DC=domain,DC=local",
      "TimeoutSeconds": 900
    }
  ],
  "neo4jSettings": {
    "dbURI": "bolt://localhost:7687",
    "dbUsername": "neo4j",
    "dbPassword": "my_secret_db_password",
    "dbTimeout": 15
  }
}
```

## Scanners

A list of scanners and links to documentation is maintained in the [Scanner Documentation](/documentation/scanners/README.md).
