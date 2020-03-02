## Contents

* [Nodes](#Nodes)
* [Relationships](#Relationships)
* [Hops](#Hops)

## Nodes
A node in BirdsNest is an object or configuration item in a system. This could an Active Directory user or group, a network folder, or a deployed application. These are represented by circular icons within the visualizer. 

Each type of node has an icon and colour to help identify types of data quickly. Additionally, hovering over an icon will list what type of node it is e.g. AD_User, AD_Object. 

![Nodes](/documentation/image/intro/nodes.png "Nodes")


## Relationships
Nodes are connected by relationships. Each relationship has a type to describe the relationship e.g. AD_MEMBER_OF, and a direction e.g. an Active Directory user node is a member of a group node, not the other way around. 

![Relationships](/documentation/image/intro/relationships.png "Relationships")


## Hops
The number of relationships between two connected nodes is it's 'hop count'. For the relationships example above, the Mike Pohatu user node is connected to the Administrators group via two paths:

* Mike Pohatu -> SCCM Admins -> Server Admins -> Administrators. This path is 3 hops
* Mike Pohatu -> Domain Admins -> Administrators. This path is 2 hops. 


 
