# Contents
* [Items](#Items)
* [Controls](#Controls)


## Items

* [Nodes](#Nodes)
* [Relationships](#Relationships)
* [Hops](#Hops)


### Nodes
A node in BirdsNest is an object or configuration item in a system. This could an Active Directory user or group, a network folder, or a deployed application. These are represented by circular icons within the visualizer. 

Each type of node has an icon and colour to help identify types of data quickly. Additionally, hovering over an icon will list what type of node it is e.g. AD_User, AD_Object. 

![Nodes](/documentation/image/intro/nodes.png "Nodes")

#### Moving Nodes
Nodes can be moved around the view using click and drag. As soon as a node is moved, it becomes pinned, and won't dynamically move if you [refresh the view](#Refresh-view). Click the thumbtack icon to unpin the node and allow it to dynamically move again. 

#### Node Details
More details about a specific node can be shown by clicking a node to select it. A node details pane will appear showing additional properties for the node, and any related nodes i.e. connected nodes that are one [hop](#Hops) away

![Node Details](/documentation/image/intro/node-details.png)

Multiple node details may be shown at once by hold CTRL and clicking the desired nodes.

### Relationships
Nodes are connected by relationships. Each relationship has a type to describe the relationship e.g. AD_MEMBER_OF, and a direction e.g. an Active Directory user node is a member of a group node, not the other way around. 

![Relationships](/documentation/image/intro/relationships.png)


### Hops
The number of relationships between two connected nodes is it's 'hop count'. For the relationships example above, the Mike Pohatu user node is connected to the Administrators group via two paths:

* Mike Pohatu -> SCCM Admins -> Server Admins -> Administrators. This path is 3 hops
* Mike Pohatu -> Domain Admins -> Administrators. This path is 2 hops. 


## Controls
There are a number of controls at the bottom left of the visualizer to help manipulate the view. From left to right these are:

* [Refresh view](#Refresh-view)
* [Play mode](#Play-mode)
* [Select](#Select)
* [Crop](#Crop)
* [Delete item](#Delete-item)
* [Center view](#Center-view)
* [Hide/show items](#Hide/show-items)
* [Export to report](#Export-to-report)
* [Find](#Find)
* [Clear view](#Clear-view)

![Controls](/documentation/image/intro/controls.png)
 
### Refresh view
The visualizer uses an algorithm to layout the nodes on the screen in a (hopefully) sensible way. Refreshing the view restarts this algorithm to update the location of the nodes based on any changes e.g. the user manually moving certain nodes.

If the number of nodes on screen exceeds 300, animation will be disabled to improve performance. 

### Play mode
In play mode, the view will automatically be refreshed as soon as a node is moved. If another node is moved before the layout has finished calculating, this process will cancel and be restarted.

### Select
### Crop
### Delete item
### Center view
### Hide/show items
### Export to report
### Find
### Clear view
