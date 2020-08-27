# Active Directory Scanner

Active Directory is the core of most Windows/Microsoft environments, and is the first Birdsnest Explorer scanner that was built. The ADScanner reads Active Directory Users, Groups, and Computers, and connects the membership relationships. 

The AD objects are used to create connections with scanners such as the File System Scanner, so it is usually best to run this scanner first after installing Birdsnest Explorer.

## Setup
After install, Birdsnest Explorer will have a **Scanners** folder in the root install folder. for the rest of this documentation, the default **c:\birdsnest** will be assumed. The folder will contain a number of json config files. For the ADScanner, the **adconfig.json** and **neoconfig.json** files need to be configured.

* **adconfig.json** configures access to the Active Directory domain the scanner will be reading from, and other ADScanner specific settings.
* **neoconfig.json** is shared across multiple scanners, and configures access to write data to the neo4j database used by Birdsnest Explorer. See [Scanners](/documentation/scanners/README.md) for more details on configuring this file. 


### Configuring adconfig.json
An example adconfig.json file will already exist in the Scanners folder and consists of four fields:

```json
{
    "ID": "domain.local",
    "AD_DomainPath": "LDAP://domain.local/DC=domain,DC=local",
    "AD_Username": "domain\\user",
    "AD_Password": "Password1"
}
```

* **ID** (required) - this field identifies the scanner and is recorded on nodes within the database. This ensures that any cleanup done by the scanner doesn't accidentally alter or delete anything it is not responsible for. This can be any unique string, and is usually best set to the domain name you are scanning. 

* **AD_DomainPath** (required) - this is the LDAP path for the root of the domain you are scanning. If you wish to connect securely over LDAPS, change **LDAP://** in the path to **LDAPS://**. The port number may also be set e.g. LDAPS://domain.local:636/DC=domain,DC=local

* **AD_Username** (optional) - if you wish to connect to Active Directory using an account other than the one running the ADScanner.exe process, you can enter the credentials here. Note the double slash due to the json format. If you delete or set the value to be empty for AD_Username or AD_Password, the scanner will use the identity of the running ADScanner.exe process.

* **AD_Password** (optional, see AD_Username) 


## Command Line Options

    Usage: ADScanner.exe /config:<configfile> /batch

By default, ADScanner.exe will search for **adconfig.json** in the same directory as itself, and will pause when finished so the user can see the resulting output. These options may be overridden with the **/config** and **/batch** options.

`/config %path_to_config_file%` - override the default location of the config file, for example if you are scanning multiple domains and require multiple configs. 

`/batch` - Normally ADScanner.exe will pause at the end and prompt the user to press a key (see screenshot below. /batch removes this pause and exits immediately. This option is required when running ADScanner from a scheduled task or other automated process where no user interaction is required. 

`/?` - show command line options and exit.

![Output](/documentation/image/active-directory/output.png)


## Results
The connections in the database can be viewed in the visualizer, and will look similar to the screenshot below. User objects are pink, computers blue, and groups green. 

Different types of group have slightly differnet colours. Domain Local groups are lighter green, universal groups darker.

You will also notice some group nodes will be larger than others in the visualizer. This is dictated by the 'scope' property on the node. A group's scope is the count of users or computers that are a member of that group, either directly or indirectly (via group nesting). A larger node in the visualizer means a larger scope relative to other nodes in the display, which indicates it contains more members.  


![Basic Path](/documentation/image/active-directory/basic-path.png)


## Reports
The Active Directory visualizer plugin includes the following reports:

|Report            |Description|
|------------------|:---|
|AD Deep Paths     |Lists Active Directory groups that are part of 'member of' chains with 5 hops or more|
|AD Empty Groups   |Lists Active Directory groups that contain no users or computers, either directly or indirectly|
|AD Group Loops    |Lists Active Directory groups that have membership loops back to themselves|