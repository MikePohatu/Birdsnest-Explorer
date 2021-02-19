# Birdsnest Explorer Managed Install

* [Birdsnest Explorer Managed Install](#birdsnest-explorer-managed-install)
  * [Overview](#overview)
  * [Before starting](#before-starting)
  * [TLDR version](#tldr-version)
  * [Step by Step](#step-by-step)
    * [License Acceptance](#license-acceptance)
    * [Install](#install)
    * [Post Install](#post-install)
      * [Console Authentication](#console-authentication)
      * [AD Scanner Configuration](#ad-scanner-configuration)
      * [Optional Post Install Actions](#optional-post-install-actions)
    * [Complete](#complete)
  * [Troubleshooting](#troubleshooting)

## Overview
The 20Road Managed Installer is provided to Birdsnest Explorer customers to make installation as simple and quick as possible. The installer is a step by step, guided install for the following:

* Install of the IIS (Web Server) role on the Windows server
* Install of Java and .Net Core 
* Install of Birdsnest Explorer
  * Install of the Neo4j database and pre-populate data for built-in data types 
  * IIS configuration
  * Active Directory authentication setup for the Birdsnest Explorer Console
  * Active Directory scanner setup
  * Optional steps to enable Windows long path support and cleanup of IIS default site

In addition to this documentation, be sure to review videos available on [YouTube](https://www.youtube.com/playlist?list=PLbymiOxRQJvL01dXHwlRDcM0koccbGEro) for more quickstart and how-to information. 

![Installer-Diagram](/documentation/image/install/installer-welcome.png)


## Before starting

Complete these steps prior starting your Birdsnest Explorer install:

* Create two Active Directory groups, one for Birdsnest Explorer users, and one for admins. You will be prompted for these during install
* Create and save password for the neo4j account (equivalent of the sa account in SQL)
* Create and save password for the svc_birdsnest_db account (to connect Birdsnest Explorer to the Neo4j database). 
* It is recommended to install a modern browser to use with the Neo4j admin console. Microsoft Edge of Chrome is recommended.

In addition, make sure you have details on your Active Directory domain such as it's FQDN, distinguished name, and if desired, the name of a specific domain controller you wish to scan and/or authenticate against.

## TLDR version
For those in a hurry, here is the abbreviated version. Note also there is a quick start video that will step you through the process.

Open **ManagedInstaller.exe** (you will be prompted for elevation), and click through the tabs from left to right. Accept the licenses, then on each tab, run through each step from top to bottom. If the component is not installed, click the action link e.g. install/update. You will be prompted to reboot and enter appropriate information as needed. 

Once complete, you should have a functional Birdsnest Explorer server, and you should be able to connect to your web console (https://localhost from the local server). You will be prompted to accept the default self-signed SSL certificate. The certificate for the site can be changed to one from a trusted CA in IIS in the normal way.

From here you can run an [Active Directory](/documentation/scanners/active-directory/README.md) scan and configure your [File System Scanner](/documentation/scanners/file-system/README.md). 

Be sure to view the [Console](/documentation/console/README.md) and [Scanners](/documentation/scanners) documentation for more details on using the Birdsnest Explorer Console and ingesting your data. 

---
## Step by Step

Run **ManagedInstaller.exe** and accept the elevation prompt. 

There are four tabs that work through the install from right to left: Welcome, License, Install, and Post Install. 


### License Acceptance

Click the License tab, review and accept the license details. Birdsnest Explorer makes use of third party open source libraries. The licenses for these products are available by clicking the 'third party' link.

![Licenses page](/documentation/image/install/licenses.png)

---
### Install

Click the Install tab. You will see a listing of dependency components that are required prior to the Birdsnest Explorer installation. 

Where a 'hard' dependency exists that isn't installed, the install action for the dependent component will be disabled and the missing dependency noted. To find out which dependency is missing, hover the cursor over the 'Missing dependency' message. 

It should be noted that some dependencies will not block the installer even if they are missing. For example Java 1.8 is required, but if you would prefer to use another distribution of Java e.g. from Oracle, this will work fine. As such the installer will let you continue if OpenJDK isn't installed.

If you have installed a dependency manually outside of the installer, click the 'Refresh' option to recheck the status in the installer. 

![Install page](/documentation/image/install/install-page.png)

You will note in the output window a message telling you to reboot after installing the .Net Core Hosting component. The install will finish successfully without a reboot, but the web console may not function until the server is rebooted. 

![Reboot message](/documentation/image/install/install-page-reboot.png)

Proceed through each component from top to bottom. You will be prompted to enter an install path for Birdsnest Explorer when you click the 'Create' action. The folder will be created at this location and the permissions set appropriately. 

![Root path prompt](/documentation/image/install/install-page-rootprompt.png)

When you click the Neo4j install action and after it has finished installing, the Neo4j post-install action will automatically be triggered. This will prompt you for passwords for two database accounts:

1. The neo4j admin account. This is the equivalent of the SQL sa account
2. The svc_birdsnest_db account. This is the account that Birdsnest Explorer will use to connect to the database. 

![Neo4j Credentials](/documentation/image/install/install-page-neo4jcreds.png)

![DB Svc Account Credentials](/documentation/image/install/install-page-svccreds.png)

Save these credentials in a secure location

The Birdsnest Explorer Scanners and Console install actions will run through without any user input

---
### Post Install

After the components have been installed, the 'Post Install' tab will run you through configuring the remaining core components. 

If you entered the Neo4j database credentials above, you will see that the Neo4j Database Setup and Neo4j Scanners Configuration components are already installed/complete. If not you can trigger them from here. 

![Post Install page](/documentation/image/install/post-install-page.png)

---
#### Console Authentication

When you click the 'Birdsnest Explorer Console Configuration' configure action, you will be prompted to enter details about your Active Directory domain that users will login against when using the Birdsnest Explorer web console. 

* Name - the name that will appear in the web console
* Domain - the FQDN of the domain or domain controller you wish to authenticate against
* ContainerDN - the distiguished name of the root container/OU containing the user accounts. Normally this will be the root of the domain
* SSL - Whether to connect to the domain using LDAPS (enabled is recommended if supported)
* AdminGroup - the SamAccountName of the group containing Birdsnest Explorer admin users. Nested groups are not supported
* UserGroup - the SamAccountName of the group containing Birdsnest Explorer users. Nested groups are not supported
* TimeoutSeconds - the session inactivity timeout for the Birdsnest Explorer web console. 

![Console authentication prompt](/documentation/image/install/post-install-consoleauth.png)

---
#### AD Scanner Configuration

On clicking the 'AD Scanner Configuration' configure action, you will be prompted to enter the details of the domain you are scanning. 

* ID - the ID that identifies items relating to this domain in the Birdsnest Explorer database
* Domain - the FQDN of the domain or domain controller to scan against
* ContainerDN - the distinguished name of the root of the scan job. The scan will start here and scan sub containers/OUs
* SSL - Whether to connect to the domain using LDAPS (enabled is recommended if supported)
* Username (optional) - the username to connect to AD with to complete the scan. The scanner will default to use the context of the user running the scanner process
* Password (optional) - see above

![AD connection prompt](/documentation/image/install/post-install-adconnection.png)

---
#### Optional Post Install Actions

In addition to the required install tasks performed above, the installer also has options to complete the following common tasks:

**Delete 'Default Web Site' from IIS**

Birdsnest Explorer creates it's own **birdsnest-console** site in IIS. As such the 'Default Web Site' is not required and can be deleted if not used for anything else

**Enable Windows Long Path Support** 

This will enable scanning of file systems even if the path is longer than the default limit of 260 characters. This is highly recommended. _Reboot required_

**Gather database memory recommendations**

Run the Neo4j memory recommendations tool to get recommended configuration settings for the memory in your server

![DB recommendations](/documentation/image/install/post-install-dbrecommendations.png)

---
### Complete

Your Birdsnest Explorer install is now finished and you should be able to connect to your web console (https://localhost from the local server). You will be prompted to accept the default self-signed SSL certificate. The certificate for the site can be changed to one from a trusted CA in IIS in the normal way.

From here you can run an [Active Directory](/documentation/scanners/active-directory/README.md) scan and configure your [File System Scanner](/documentation/scanners/file-system/README.md). 

Be sure to view the [Console](/documentation/console/README.md) and [Scanners](/documentation/scanners) documentation for more details on using the Birdsnest Explorer Console and ingesting your data.

## Troubleshooting

In addition to logging to the output window in the installer, logs are also written to the **Logs** directory in the same folder as the installer executable. You can review these for clues as to the cause of any issues. 20Road Support may also request these logs for any support enquiries. 

Additional troubleshooting steps can be found [here](/documentation/install/troubleshooting.md)