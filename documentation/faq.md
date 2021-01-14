# FAQ

* [FAQ](#faq)
  * [Pre-Sales FAQ](#pre-sales-faq)
  * [Usage](#usage)

## Pre-Sales FAQ

A general pre-sales FAQ is maintained on the [20Road website](https://www.20road.com/birdsnest-explorer-faq/).

---

## Usage

Q - Why does Birdsnest Explorer shows fewer permissions connected to my folder than the permissions tab in Windows?\
A - Birdsnest Explorer will only connect permissions where they have been set on that particular folder. The other permissions listed will be inherited from a parent folder. You will find the group/user listed on the permissions tab connected to the relevant parent folder.

\
Q - Why can't I see all folders from a file system in the Visualizer?\
A - Birdnest Explorer only records the folders that have permissions set on them. This is to reduce the clutter that would come from showing all the folders in a file system and the associated performance impact

\
Q - Can I scan a file server in a remote data center without traversing the WAN?\
A - Currently no, but doing a remote scan is a planned feature in a future release

\
Q - Does the file system scanner record files?\
A - Currently no, only folders are scanned for performance reasons. Enabling file scanning as an option is planned for the next release

\
Q - Why are some AD_MEMBER_OF relationships orange?\
A - These relationships come from a foreign domain. The member is not from the same domain as the group

\
Q - The Active Directory scanner doesn't record an attribute I'm interested in. Can I add it?\
A - Yes, a tutorial on doing this is being developed. In the meantime, an example script to injest a users mobile and manager from AD is available [here](/source/utils/ExtendAD-Example.ps1), and example console plugin to add the items to the Visualizer is available [here](/source/BirdsNest.Net/Console/Plugins/plugin-activedirectory-extended-example.json)

\
Q - Can Birdsnest Explorer scan Azure AD?\
A - This is currently under development. You can check current status [here](/documentation/scanners/azuread/README.md)