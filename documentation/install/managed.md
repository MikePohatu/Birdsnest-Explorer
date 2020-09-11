# Birdsnest Explorer Managed Install

* [Birdsnest Explorer Managed Install](#birdsnest-explorer-managed-install)
  * [Overview](#overview)
  * [TLDR version](#tldr-version)
  * [Step by Step](#step-by-step)

## Overview
The 20Road Managed Installer is provided to Birdsnest Explorer customers to make installation as simple and quick as possible. The installer a step by step, guided install for the following:

* Install of the IIS (Web Server) role on the Windows server
* Install of Java and .Net Core 
* Install of Birdsnest Explorer
  * Install of the Neo4j database and pre-populate data for built-in data types 
  * IIS configuration
  * Active Directory authentication setup for the Birdsnest Explorer Console
  * Active Directory scanner setup
  * Optional steps to enable Windows long path support and cleanup of IIS default site

<img src="/documentation/image/install/installer-welcome.png" width="800">

## TLDR version
For those in a hurry, here is the abbreviated version. Note also there is a quick start video that will step you through the process.

Before starting: 
* Create two Active Directory groups, one for Birdsnest Explorer users, and one for admins. You will be prompted for these during install
* Create and save password for the neo4j account (equivalent of the sa account in SQL)
* Create and save password for the svc_birdsnest_db account (to connect Birdsnest Explorer to the Neo4j database). 
* It is recommended to install a modern browser to use with the Neo4j admin console. Microsoft Edge of Chrome is recommended.

Now open **ManagedInstaller.exe** (you will be prompted for elevation), and click through the tabs from left to right. Accept the licenses, then on each tab, run through each step from top to bottom. If the component is not installed, click the action link e.g. install/update. You will be prompted to reboot and enter appropriate information as needed. 

Once complete, should have a functional Birdsnest Explorer server, and you should be able to connect to your web console (https://localhost from the local server).

From here you can run an [Active Directory](/documentation/scanners/active-directory/README.md) scan and configure your
[File System Scanner](/documentation/scanners/file-system/README.md).

## Step by Step

