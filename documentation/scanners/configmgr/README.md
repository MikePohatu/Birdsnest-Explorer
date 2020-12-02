# ConfigMgr Scanner (BETA)

* [ConfigMgr Scanner (BETA)](#configmgr-scanner-beta)
  * [Overview](#overview)
  * [Setup](#setup)
    * [Dependencies](#dependencies)
    * [Configuration Files](#configuration-files)
    * [Configuring cmconfig.json](#configuring-cmconfigjson)
  * [Command Line Options](#command-line-options)
  * [Results](#results)
    * [Connection with Active Directory Objects](#connection-with-active-directory-objects)
  * [Reports](#reports)

## Overview

The ConfigMgr (aka SCCM, MEMCM) scanner provides ingestion of data from Microsoft Endpoint Manager Configuration Manager. 

---

## Setup

### Dependencies

The following files are required for the ConfigMgr scanner to function. These libraries are part of ConfigMgr and can't currently be distributed with Birdsnest Explorer. 

* AdminUI.WqlQueryEngine.dll
* Microsoft.ConfigurationManagement.ManagementProvider.dll

These files are available on a machine with the ConfigMgr console from the following location: **_%ConfigMgrInstallPath%\AdminConsole\bin_**

Copy these files next to **CMScanner.exe** in the Scanners folder.

### Configuration Files

After install, Birdsnest Explorer will have a **Scanners\config** folder in the root install directory. For the rest of this documentation, the default **c:\birdsnest** will be assumed. The folder will contain a number of json config files. For the CMScanner, the **cmconfig.json** and **neoconfig.json** files need to be configured.

* **cmconfig.json** configures access to the ConfigMgr site server the scanner will be reading from.
* **neoconfig.json** is shared across multiple scanners, and configures access to write data to the neo4j database used by Birdsnest Explorer. See [Scanners](/documentation/scanners/README.md) for more details on configuring this file. 

### Configuring cmconfig.json

An example **cmconfig.json** file will already exist in the config folder and consists of five fields:

```json
{
  "ScannerID": "cm_scanner1",
  "SiteServer": "site_server_fqdn",
  "Domain": "",
  "Username": "",
  "Password": ""
}
```

* **ScannerID** (required) - this field identifies the scanner and is recorded on nodes within the database. This ensures that any cleanup done by the scanner doesn't accidentally alter or delete anything it is not responsible for. This can be any unique string, and is usually best set to the site server or site code you are scanning. 

* **SiteServer** (required) - the FQDN of the ConfigMgr site server to connect to for the scan. 
* **Domain** (optional) - if credentials are required to connect to the server, set the domain of the credentials here
* **Username** (optional) - if credentials are required to connect to the server, set the username here. If this field is blank the scanner will use the context of the logged in user (or user specified in a Scheduled Task)
* **Password** (optional) - if credentials are required to connect to the server, set the password here

---

## Command Line Options

    Usage: CMScanner.exe /config:<configfile> /batch

By default, ADScanner.exe will search for **cmconfig.json** in the config directory, and will pause when finished so the user can see the resulting output. These options may be overridden with the **/config** and **/batch** options.

`/config:%path_to_config_file%` - override the default location of the config file. 

`/batch` - Normally CMScanner.exe will pause at the end and prompt the user to press a key (see screenshot below. /batch removes this pause and exits immediately. This option is required when running CMScanner from a scheduled task or other automated process where no user interaction is required. 

`/?` - show command line options and exit.

---

## Results

The connections in the database can be viewed in the visualizer, and will look similar to the screenshot below. 

![results](/documentation/image/configmgr/results.png)

### Connection with Active Directory Objects

Where a ConfigMgr object i.e. user or computer is a representation of an object in Active Directory, it will be connected to a match AD_Object data type in Birdsnest Explorer. In the example below the nodes on the furthest left are an AD_User and AD_Computer data type. These are connected to the ConfigMgr object with a CM_HAS_OBJECT relationship. 

![results](/documentation/image/configmgr/results-adobjects.png)

---

## Reports
There are currently no reports created for this plugin