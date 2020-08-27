# Advanced Search

* [Overview](#Overview)
    * [Advanced Search Items](#Advanced-Search-Items)
    * [Advanced Search Box](#Advanced-Search-Box)
        * [Search Path Items](#Search-Path-Items)
        * [Condition Items](#Condition-Items)
        * [General Controls](#General-Controls)
* [Editing Relationships](#Editing-Relationships)
    * [Multiple Hop Relationships](#Multiple-Hop-Relationships)
        * [Unlimited Hops](#Unlimited-Hops)
    * [Relationship Types](#Relationship-Types)
* [Conditions](#Conditions)
* [Results](#Results)
    * [Adding Search Results to Graph](#Adding-Search-Results-to-Graph)
    * [Search Result Relationships](#Search-Result-Relationships)
* [Sharing Searches](#Sharing-Searches)

## Overview
The Birdsnest Explorer advanced search tool is designed to allow searching for specific 'paths' within the interconnected data stored in the database.

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

## Editing Relationships

To change the details of a relationship in the search, click it and then click the _Edit_ button, or double click it. The Relationship edit dialog will appear.

![Multi-Hop-Search](/documentation/image/console/search/adv-search-rel-condition.png)

From here you can edit the various options:

1. The relationship type
2. The identifier of the relationship within the search
3. Whether there are multiple hops of this relationship type
4. If hops are limited, the minimum and maximum hop count
5. The direction of the relationship relative to the nodes that surround it

### Multiple Hop Relationships

It is often the case that multiple hops exist between two nodes you are interested in. For the following examples we will look at paths connecting (node1) to (node2).

By default, when a relationship is added to the search, it is limited to a single hop<sup>*</sup> e.g.

```cypher
(node1)-[hop1]->(node2)
```

_<sup>*</sup> This is done for performance reasons. The more 'open ended' a search is, the more likely it is that it will slow down. Where possible, always make your searches as specific as possible._


However you may be looking for ways that node1 is related to node2 indirectly with other nodes in between, e.g.

```cypher
(node1)-[hop1]->(some_other_node)-[hop2]->(node2)
```

In this instance you could change the minimum & maximum hop count to 2 to represent this in your search. In the screenshot below, _node1_ maps to the Bob.Demo node, _node2_ to the Administrators node, and _hop1_ maps to the two **AD_MEMBER_OF** relationships.

![Multi-Rel-Search](/documentation/image/console/search/adv-search-multi-rel.png)

_Note the '2..2' in the relationship box in the screenshot above. This represents the Min and Max selected in the edit dialog box. This will shown as * if the hop count is not limited_

#### Unlimited Hops

If you don't know how many hops there will be between node1 and node2, you can either set a large range e.g. Min=1 and Max=20, or uncheck _Limit hops_. 

```cypher
(node1)-[hop1]->.......-[hop_n]->(node2)
```

### Relationship Types

Until now we have discussed relationships that could be of any type. However sometimes we want to be specific about the type of relationship/hop e.g.

```cypher
(node1)-[hop1:AD_MEMBER_OF]->(some_other_node)-[hop2:GIVES_ACCESS_TO]->(node2)
```

![Relationship-Types](/documentation/image/console/search/adv-search-rel-types.png)

In this instance you need to set the **Type** in the edit dialog. Note that you cannot set multiple types e.g. relationship/hop is AD_MEMBER_OF _or_ GIVES_ACCESS_TO.

## Conditions

Conditions will almost always be required when creating searches to filter results to a manageable size. 

Note that you cannot create a condition for an item for which you have not set a type. This is for performance reasons as not limiting the search by type would likely result in a full database search (very slow).

## Results

Search results will be listed at the bottom of the Advanced Search box. Search results may be added as a whole, or individually.

### Adding Search Results to Graph

To add all search results to the graph, click **Add to view**. 

![Adv-Search-Results](/documentation/image/console/search/adv-search-results.png)

To add individual results to the graph, click the number e.g. 10 in the screenshot above. A listing of all nodes will be displayed. Click the **(+)** icon next to the item to add the node to the graph.

### Search Result Relationships

It is important to note that you may see additional relationships in the graph after adding your results. For example if your search only includeds _GIVES_ACCESS_TO_ relationships, you may also see _AD_MEMBER_OF_ relationships between groups and AD objects, or _FS_APPLIES_INHERITANCE_TO_ relationships between folders. 

This is because the visualizer will search for any relationships between all the nodes that exist in the graph. If the nodes have relationships in addition to the ones that you searched for, then they will also be added. 

## Sharing Searches

It is often useful to share a search you have created with other users, or with an admin to turn into a report. You can do this by clicking the _share_ button

![Adv-Search-Results](/documentation/image/console/search/adv-search-share.png)

This will display a dialog with two items:

1. A URL containing an encoded version of the search to open. Note that the full URL is not displayed in the text due to its length. Right click the URL and select _Copy Link_ or the equivalent for your browser. 
2. A Cypher database query. This can be used by the Birdsnest Explorer administrator to create a report based on that query in a [Console Plugin](/documentation/console/plugins/README.md#Reports)
