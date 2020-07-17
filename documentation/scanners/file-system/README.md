# File System Scanner

* [Overview](#Overview)
* [Concepts](#Concepts)
    * [Threads](#Threads)
    * [Device](#Device)
    * [Datastore](#Datastore)
    * [File System](#File-System)

## Overview
The File System Scanner is a multi-threaded Windows file system 'crawler', traversing one or more file systems interrogating the permissions and mapping them to Active Directory or Builtin objects.

* The scanner does not interrogate files by default, only folders. This is done for performance reasons, as having to scan files as well could potentially increase scan time by multiple orders of magnitude. 

* The scanner can only interrogate items it has permissions to. Make sure that the service account running the scanner has the appropriate permissions to read the permissions of all the items you wish to scan.

## Concepts

The scanner has a inbuilt structure designed to both represent how a file system is attached underlying infrastructure, and control load. 

When you create a configuration for the scanner, it will contain a structure like this:

<pre>
Device
    Datastore
        File System 1
        File System 2
        ...
</pre>

The following sections outline each item. Pay special attention to [Datastore](#Datastore) and [Threads](#Threads), as these have implications to how the scanner will apply load to your infrastructure. 

### Threads

The File System Scanner is a multi-threaded application. The number of threads the scanner will use is set in the configuration file. Some testing and tuning may be required to find the ideal number of threads to apply to each [Datastore](#Datastore) for best performance. 

It should be noted that each time the scanner is run, it is completely independant and unaware of any already running scanners.

### Device

The Device item is used to create a connection to a device within the the database e.g. an Active Directory computer object. 

### Datastore

The Datastore item represents a set of disks on which one or more file systems are located. 

Each scan job is independant and will create the number of threads specified in the configuration. 

If a set of disks contains 5 file systems, and you create an individual scanner configuration for each file system, each with 8 threads, if you run them all at the same time there will be 40 scanner threads all running on the same disks at once. This may add more load than expected.

Instead it is recommended to only create one scanner configuration for each datastore, with all relevant file systems listed under that datastore within the configuration. This way only the specified number of scanning threads can apply load to those disks. 

### File System

