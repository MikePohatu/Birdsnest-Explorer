# Troubleshooting

* [Troubleshooting](#troubleshooting)
  * [Dependency installation](#dependency-installation)
  * [Authentication](#authentication)

## Dependency installation

Q - A dependent component installer i.e. OpenJDK or .Net Hosting Bundle has failed to install.\
A - If the server has not been rebooted, try rebooting and starting again. If still no better, try running the installers manually. These can be found in the **Content** folder in your install package. If install finishes correctly, click the refresh link to continue.

## Authentication

Q - I cannot login and see _'Error: A referral was returned from the server.'_ in the console log file\
A - This indicates an issue connecting correctly to the domain. Check the *birdsnest\Console\version\appsettings.json* file for typos, especially in your **ContainerDN** setting. 