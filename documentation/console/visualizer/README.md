# Visualizer


# Contents
* [Overview](#Overview)
* [Items](#Items)
* [Controls](#Controls)

## Overview

The main part of the visualizer is the <a href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)" target="_blank">graph</a>, a view that lays out connected data. Additionally, there are controls to manipulate the layout of the graph, and search functionality to find and add data to the graph. 

![Visualizer](/documentation/image/console/visualizer-layout.png)

## Items

* [Nodes](#Nodes)
* [Relationships](#Relationships)
* [Hops](#Hops)


### Nodes
A node in BirdsNest represents an object or configuration item in a system. This could an Active Directory user or group, a network folder, or a deployed application. These are represented by circular icons within the visualizer. 

Each type of node has an icon and colour to help identify types of data quickly. Additionally, hovering over an icon will list what type of node it is e.g. AD_User, AD_Object. 

![Nodes](/documentation/image/console/nodes.png "Nodes")

#### Moving Nodes
Nodes can be moved around the view using click and drag. As soon as a node is moved, it becomes pinned, and won't dynamically move if you [refresh the view](#Refresh-view). Click the thumbtack icon to unpin the node and allow it to dynamically move again. 

#### Node Details
More details about a specific node can be shown by clicking a node to select it. A node details pane will appear showing additional properties for the node, and any related nodes i.e. connected nodes that are one [hop](#Hops) away

![Node Details](/documentation/image/console/node-details.png)

Multiple node details may be shown at once by hold CTRL and clicking the desired nodes.

### Relationships
Nodes are connected by relationships. Each relationship has a type to describe the relationship e.g. AD_MEMBER_OF, and a direction e.g. an Active Directory user node is a member of a group node, not the other way around. 

![Relationships](/documentation/image/console/relationships.png)


### Hops
The number of relationships between two connected nodes is it's 'hop count'. For the relationships example above, the Mike Pohatu user node is connected to the Administrators group via two paths:

* Mike Pohatu -> SCCM Admins -> Server Admins -> Administrators. This path is 3 hops
* Mike Pohatu -> Domain Admins -> Administrators. This path is 2 hops. 

The hop count is used in [Advanced Search](/documentation/console/visualizer/advanced-search/README.md) to limit or refine searches. 


## Controls
There are a number of controls at the bottom left of the visualizer to help manipulate the view. From left to right these are:

* [Refresh view](#Refresh-view)
* [Play mode](#Play-mode)
* [Select](#Select)
* [Invert selection](#Invert-Selection)
* [Crop](#Crop)
* [Delete item](#Delete-item)
* [Center view](#Center-view)
* [Hide/show items](#Hide/show-items)
* [Export to report](#Export-to-report)
* [Find](#Find)
* [Clear view](#Clear-view)

![Controls](/documentation/image/console/controls.png)
 
### Refresh view
The visualizer uses an algorithm to layout the nodes on the screen in a (hopefully) sensible way. Refreshing the view restarts this algorithm to update the location of the nodes based on any changes e.g. the user manually moving certain nodes.

If the number of nodes on screen exceeds 300, animation will be disabled to improve performance. 

![Refresh-Tool](/documentation/image/console/refresh.gif)

### Play mode
In play mode, the view will automatically be refreshed as soon as a node is moved. If another node is moved before the layout has finished calculating, this process will cancel and be restarted.

### Select
The select tool is used to select multiple nodes within the specified area. Click and drag the desired area in the visualizer. All nodes within the area will be selected. Note that [node detail](#Node-Details) cards will not be displayed using this method as you may be selecting dozens or even hundreds of nodes at a time.

![Select](/documentation/image/console/select.png)

### Invert Selection
The invert tool inverts the selection of all enabled nodes in the view. Like the [select](#Select) tool, [node detail](#Node-Details) cards will not be displayed using this method.

### Crop
The crop tool uses click and drag to select an area within the view similar to the select tool, but its purpose is to zoom in on an area of interest. Click and drag to select an area within the view, and it will zoom and re-center appropriately.

![Crop-Tool](/documentation/image/console/crop.gif)

### Delete item
The delete item button will delete any selected nodes within the view. Note that this tool will only delete the nodes from the view. It has no impact on data stored in the database. 

### Center view
The center view tool will re-center the view based on the nodes currently displayed. This can be useful if zooming or a refresh causes all nodes to be moved off screen. If the window is resized, or if the visualizer detects that all nodes are currently off screen at the end of a refresh, this function will be run automatically.

### Hide/show items
The 'eye' tool is used to disable information within the view, thereby allowing other information to be more prominent. 

Additionally, disabled items cannot be selected when using the [select](#Select) tool. This allows you to delete all data of a specific type by disabling all other types, then selecting everything. Only the enabled items will be selected, so you can then move or delete them.

Clicking the appropriate 'eye' icon for a type will disable or enable any nodes of that type in the view. 

Note that some nodes may be of multiple types e.g. AD_Object & AD_Group. Because of this you may find the current state of some nodes may not reflect the state of the eye icon for that type.  

![Eye-Tool](/documentation/image/console/eye-tool.gif)

### Export to report
Export the nodes in the current view to the report view in a new tab. The report view will allow you to save the listing out to csv file.

### Find
The find tool searches within the nodes currently in the view, and selects any matches. This search only looks at the name of the nodes, i.e. the name below the circular icon in the view. 

### Clear view
Clears all nodes from the view. Note this only effects the current visualizer view has no impact on the backend database. 