# BirdsNest Installation

## Architecture

## System Requirements

* 2 core CPU
* 8GB RAM (minimum 16GB recommended)
* 5GB disk space
* Windows Server 2016 or newer (Nano & Hyper-V versions not supported)

## Dependencies

The following dependencies are installed with BirdsNest if not already discovered on the system.

* Web Server Role (IIS) installed
* Java 1.8
* .Net Core Server Hosting Runtime 3.1.3
* Modern Browser (for neo4j web console) - Microsoft Edge or Chrome recommended

## Database


## Firewall

The following firewall rules are required on the BirdsNest server:

### Inbound
In addition to any normal admin functions e.g. RDP, ping, the following ports are required for the BirdsNest Console to function


| Service | Port | Protocol | Description |
|---------|------|----------|-------------|
| HTTPS | 443 | TCP | Console |
| HTTP | 80 | TCP | Console (optional redirect to HTTPS) |


### Outbound

<ins>Console</ins>

In addition to any core functionality e.g. DNS, Domain Member services etc, the following ports are required for the BirdsNest Console to function

| Destination | Service | Port | Protocol | Description |
|-------------|---------|------|----------|-------------|
| Login Domain Controllers | LDAP | 389 | TCP | Console Authentication |
| Login Domain Controllers | LDAPS | 636 | TCP | Console Authentication |


<ins>Scanners</ins>

In addition to any core functionality e.g. DNS, Domain Member services etc, the following ports are required for BirdsNest Scanners to function\*

| Destination | Service | Port | Protocol | Description |
|-------------|---------|------|----------|-------------|
| Scanned Domain Controllers | LDAPS | 636 | TCP | Active Directory Scanner |
| Scanned Domain Controllers | LDAP | 389 | TCP | Active Directory Scanner |
| Scanned File Servers | SMB | 139, 445 | TCP | File System Scanner |

\*Appropriate firewall rules will be need to be added for any additional scanners.
