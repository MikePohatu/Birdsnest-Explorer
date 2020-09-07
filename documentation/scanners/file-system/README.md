# File System Scanner

* [Overview](#Overview)
* [Concepts](#Concepts)
    * [Threads](#Threads)
    * [Device](#Device)
    * [Datastore](#Datastore)
    * [File System](#File-System)
        * [Blocked Folders](#Blocked-Folders)
## Overview
The File System Scanner is a multi-threaded Windows file system 'crawler', traversing one or more file systems interrogating the permissions and mapping them to Active Directory or Builtin objects.

The following points should be noted:

* The scanner does not interrogate files, only folders. This is done for performance reasons, as having to scan files as well could potentially increase scan time by several orders of magnitude. Scanning files will be added as an option in a future version.

* The scanner can only interrogate items it has permissions to. Make sure that the service account running the scanner has the appropriate permissions to read the permissions of all the items you wish to scan.

* The scanner does not record every folder it finds in the database, only ones that have permissions set on them. This is done to reduce the number of nodes in the Visualizer, and the associated 'clutter' and performance impact.

## Concepts

The scanner has a inbuilt structure designed to both represent how a file system is attached underlying infrastructure, and control load. 

When you create a configuration for the scanner, it will contain a structure like this:

```
"credentials": ...,
"maxthreads": 8,
"datastores": [
    {
        "name": "datastore1",
        "host": "server",
        "filesystems": [
            {
                ...  file system 1
            },
            {
                ...  file system 2
            }
        ]
    }
]
```

The following sections outline each item. Pay special attention to [Datastore](#Datastore) and [Threads](#Threads), as these have implications to how the scanner will apply load to your infrastructure. 

### Threads

The File System Scanner is a multi-threaded application. The number of threads the scanner will use is set in the configuration file. Some testing and tuning may be required to find the ideal number of threads to apply to each [Datastore](#Datastore) for best performance. 

It should be noted that each time the scanner is run, it is completely independant and unaware of any already running scanners.

### Device

The Device item is used to create a connection to a device within the the database e.g. an Active Directory computer object. 

### Datastore

The Datastore item represents a set of disks on which one or more file systems are located. 

If a set of disks contains 5 file systems, and you create an individual scanner configuration for each file system, each with 8 threads, if you run them all at the same time there will be 40 scanner threads all running on the same disks at once. 

Instead it is recommended to only create one scanner configuration for each datastore, with all relevant file systems listed under that datastore within the configuration. This way only the specified number of scanning threads can apply load to those physical disks. 

### File System

A File System item represents a Windows network file system. The scanner will crawl the file system and read the security permissions of each folder. 

Where a folder cannot be read for whatever reason, it will be recorded as a [blocked folder](#Blocked-Folders) in the database.

Note that the share permissions are not read.

#### Blocked folders
A blocked folder is one where the scanner cannot read it's details, and therefore is blocked from scanning it and it's child folders. The folder will be recorded by the scanner and the property 'blocked' will be set to **true** on the item in the database to allow for searching and reporting.

A blocked usually represents a folder where inheritance is disabled, and there aren't appropriate permissions for the account running the file system scanner.

## Configuration Details

```
"credentials": [
    {
        "id": "cred1",
        "username": "domain\\username",
        "password": "P@ssword1"
    }
],
"maxthreads": 10,
"datastores": [
    {
        "name": "datastore1",
        "host": "server",
        "comment": "san1",
        "filesystems": [
            {
                "id": "123",
                "credentialid": "cred1",
                "path": "\\\\server\\share1",
                "scanfiles": false,
                "comment": "san1-share1"
            },
            {
                "id": "456",
                "credentialid": "cred1",
                "path": "\\\\server\\share2",
                "comment": "san1-share2"
            }
        ]
    }
]
```