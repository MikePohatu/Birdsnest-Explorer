# Advanced Search

* [Overview](#Overview)
    * [Advanced Search Items](#Advanced-Search-Items)
    * [Advanced Search Box](#Advanced-Search-Box)
        * [Search Path Items](#Search-Path-Items)
        * [Condition Items](#Condition-Items)
        * [General Controls](#General-Controls)
* [Multi-Hop Searches](#Multi-Hop-Searches)
* [Conditions](#Conditions)
* [Results](#Results)
    * [Adding Search Results Graph](#Adding-Search-Results-Graph)
    * [Search Result Relationships](#Search-Result-Relationships)
* [Sharing Searches](#Sharing-Searches)

## Overview
The BirdsNest advanced search tool is designed to allow searching for specific 'paths' within the interconnected data stored in the database.

### Advanced Search Items
The advanced search box is designed to visually represent your search in a way that is similar to the graph view.

When the add button is clicked, boxes are added to the search to represent nodes and relationships.

* The boxes with round sides represent the round items in the graph i.e. nodes
* The boxes with straight sides represent the straight items in the graph i.e. relationships

Additionally, when additional search properties are chosen e.g. 'nodes of type X', styling is applied to the items to match the styling in the graph.

![Single-Hop-Search](/documentation/image/console/search/single-hop-search.png)

### Advanced Search Box

![Adv-Search-Box](/documentation/image/console/search/adv-search.png)

#### Search Path Items

1. Move selected item up or down
2. Delete selected item from search
3. Edit selected item
4. Add a new node (a new relationship will be added automatically)
5. Node item. This is the selected item (note the bold border). This node has a type selected so has styling applied. Double click or click edit button to edit.
6. Relationship item. Double click or select and click edit button to edit.
7. Node item. This node doesn't have a type selected so has no styling applied.

#### Condition Items

8. Move selected condition up or down
9. Delete selected condition from search
10. Edit selected condition
11. Add a new condition
12. Condition item
13. Add new condition within existing AND/OR condition

#### General Controls

14. Minimise search
15. Toggle [simple search](/documentation/console/visualizer/simple-search/README.md)
16. Clear search
17. Share search
18. Run Search

## Multi-Hop Searches

![Multi-Hop-Search](/documentation/image/console/search/multi-hop-search.png)

## Conditions

Conditions will almost always be required when creating searches to filter results to a manageable size. 

## Results

Search results will be listed at the bottom of the Advanced Search box. Search results may be added as a whole, or individually.

### Adding Search Results Graph

To add all search results to the graph, click **Add to view**. 

![Adv-Search-Results](/documentation/image/console/search/adv-search-results.png)

To add individual results to the graph, click the number e.g. 10 in the screenshot above. A listing of all nodes will be displayed. Click the **(+)** icon next to the item to add the node to the graph.

### Search Result Relationships

It is important to note that you may see additional relationships in the graph after adding your results. For example if your search only included _GIVES_ACCESS_TO_ relationships, you may also see _AD_MEMBER_OF_ relationships between groups and AD objects, or _FS_APPLIES_INHERITANCE_TO_ relationships between folders. 

This is because the visualizer will search for any relationships between all the nodes that exist in the graph. If the nodes have relationships in addition to the ones that you searched for, then they will also be added. 

## Sharing Searches

It is often useful to share a search you have created with other users, or with an admin to turn into a report. You can do this by clicking the _share_ button

![Adv-Search-Results](/documentation/image/console/search/adv-search-share.png)

This will display a dialog with two items:

1. A URL containing an encoded version of the search. Note that the full URL is not displayed in the text due to its length. Right click the URL and select _Copy Link_ or the equivalent for your browser. 
2. A Cypher database query. This can be used by the BirdsNest administrator to create a report based on that query in a [Console Plugin](/documentation/console/plugins/README.md#Reports)