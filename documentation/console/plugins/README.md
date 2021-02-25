# Birdsnest Explorer Console Plugins

* [Birdsnest Explorer Console Plugins](#birdsnest-explorer-console-plugins)
  * [Overview](#overview)
  * [Files](#files)
  * [High Level Schema](#high-level-schema)
  * [Data Types](#data-types)
    * [Sub-Types](#sub-types)
    * [Icons](#icons)
    * [Properties](#properties)
    * [Data Type Schema Definition](#data-type-schema-definition)
    * [Example Data Type](#example-data-type)
    * [Styling](#styling)
  * [Reports](#reports)
    * [Query](#query)
      * [Returned data](#returned-data)
    * [Property Filters](#property-filters)
    * [Report Schema](#report-schema)
    * [Example Report](#example-report)

## Overview

After data is ingested into the database by a scanner, a console plugin is required to define the new types of data available. Each console plugin consists of a .json file and a .css file that contains the following:

* Definition of new data types available for search functions
* Definition of properties for each new data type
* Custom Reports
* Styling and icons for new data types in the Visualizer
  
---
## Files
A plugin consists of two files:
1. plugin-_plugin_name_.json
2. plugin-_plugin_name_.css

These files are placed in 

**_$install_path\Console\\$version\Plugins_**

e.g.

**_C:\birdsnest\Console\1.0\Plugins_**

The .json file contains definitions of the various properties of the plugin, and the .css file contains standard css [styling](#styling) for items added to the visualizer.

The [high level schema](#high-level-schema) of the .json file is covered in the following section, and [data types](#data-types) and [reports](#reports) covered in more detail following that. 

The css classes applied to items in the visualizer are dependant on the data types of the item. This is covered in more detail in [Data Types](#data-types).

---
## High Level Schema
The following outlines the high level schema of the _plugin-$pluginname.json_ file. It should be noted that in the backend and code, a relationship is known by the term **edge** (from mathematical graphs). The _edgeDataTypes_ field below relates to relationships in the visualizer.

Fields denoted with a **$** are items to be configured.
See [Reports](#reports) and [Data Types](#data-types) for details on reports and edge/node data types configuration respectively.

```json
{
   "name": "$plugin_name",
    "displayName": "$plugin_display_name",
    "reports": {
        "$reports_config..."
    },
    "edgeDataTypes": {
        "$edge_data_types_config..."
    },
    "nodeDataTypes": {
        "$node_data_types_config..."
    }
}
```

---

## Data Types
Each node or edge in the database must have one or more data types defined e.g AD_User, AD_Object, AD_MEMBER_OF. In the Neo4j database these are called 'labels'.

When a node or edge is added to the visualizer, it's data types are added as css classes to the HTML/SVG element e.g. 

```html
class="AD_User AD_Object"
class="AD_MEMBER_OF"
```

Additionally, if any [sub-types](#sub-types) are defined, they are also added as classes to the HTML/SVG element. 

These classes can be used to apply [styling](#styling) to the items using the plugin .css file. 

### Sub-Types
In some cases, it is useful to further break up a data-type into smaller groups. An example of this is Active Directory groups which can be Security or Distribution, and additionally Domain Local, Global, or Universal. 

If one or more sub-types are required, a property is chosen as a 'sub-type property'. The value of this property will become a sub-type. The plugin can define a list of sub-types.

The data-type and each sub-type are joined together with a dash (-) and added as a CSS class on the HTML/SVG element. 

For example, on an AD_Group, the sub-type property is 'grouptype'. The value of the 'grouptype' property may have a value of 'security_global', 'security_domainlocal', etc. 

An AD_Group node with a 'grouptype' of 'security_domainlocal' will have the following css class added:

```
AD_Group-security_domainlocal*
```

_*Note: Any characters found in the sub-type value that are [css selectors](https://www.w3schools.com/cssref/css_selectors.asp), e.g. period, tilde, colon, will be automatically replaced with a '-' character. For example a value of 'demo.local' will be replaced in the css class name with 'demo-local'._

This new css class allows the node or edge to be [styled](#styling) differently from the parent type or other sub-types. 

### Icons
The icons used on nodes within the Visualizer are provided by FontAwesome. Each data type can specify the unicode value for a FontAwesome icon. Note that only free icons are available in Birdsnest Explorer at this time.

FontAwesome icons are available from https://fontawesome.com/icons?d=gallery&m=free.

### Properties

Each property for the data type must be defined in the **properties** object. Each property has two options that may be defined:

**type:** Defines the type of property as a string (default), number, or boolean. This will dictate the options available to the user when creating/editing conditions in the console [Advanced Search](/documentation/console/visualizer/advanced-search/README.md)

**indexEnforced:** Specifies that an index set on that property cannot be edited/disabled in the [index editor](/documentation/console/admin/README.md#index-editor). The index may be required for a scan task to perform correctly.

### Data Type Schema Definition
Fields denoted with a $ are items to be configured. 

```json
"nodeDataTypes": {
    "$data_type": {
        "displayName": "$name_to_appear_in_search_list",
        "description": "$description_to_appear_in_tooltip",
        "properties": {
            "$property_name": {
                "type": "$OPTIONAL - (string (default) | number | boolean)",
                "indexEnforced": false
            }
        },
        "default": "$default_property_name_for_search",
        "subTypes": ["$sub-type-property1", "$sub-type-property2"],
        "icon": "$font_awesome_icon_unicode"
    }
}
```

### Example Data Type

```json
    "nodeDataTypes": {
        "AD_Group": {
            "displayName": "AD Group",
            "description": "Active Directory group",
            "properties": {
                "description": {},
                "dn": { 
                    "indexEnforced": true 
                },
                "domainid": {},
                "grouptype": {},
                "id": { 
                    "indexEnforced": true 
                },
                "info": {},
                "lastscan": {},
                "member_count": {
                    "type": "number"
                },
                "name": {},
                "path": {},
                "rid": {},
                "samaccountname": {},
                "scope": {
                    "type": "number"
                }
            },
            "default": "samaccountname",
            "subTypes": ["grouptype"],
            "icon": "f500"
        }
    },
    ...
```

### Styling
Each plugin includes a .css file containing any custom styling to be applied for the defined data types. 

The [Data Types](#data-types) and [Sub-Types](#sub-types) sections provide details on the css classes that are applied to items when added to the Visualizer. 

The following diagram shows the areas to configure to apply custom colours to the items in the Visualizer. $dataType represents the data type name configured above e.g. AD_Group or AD_Group-security_domainlocal.

![Styling](/documentation/image/console/styling.png)

As an example, the basic css for styling an AD_User node would look like this:

```css
.AD_User {
    stroke: crimson;
    fill: #ffe2e7;
}

.AD_User.icon {
    fill: crimson;
}
```

When applying styling to sub types, normal css rules apply i.e. last write wins. If you apply styling lower in the css file for your subtype, it will override any conflicting css above it.

---

## Reports

### Query
The value of the query field defines the database query that builds the report.

neo4j uses a query language called **Cypher**. Detailed usage of the Cypher language is beyond the scope of this document, however some places to start are outlined below.


Firstly, the cypher [documentation](https://neo4j.com/docs/cypher-manual/3.5/) is an excellent resource.

Secondly, the visualizer advanced search function provides the Cypher query when you click the **Share** button. If you have a search you want to run regularly you can take the query from the search as your starting point. The report view also provides a **Show Query** option in the menu.

Once an initial query is created, it is worth testing the query in the neo4j web console. On the Birdsnest Explorer server, this will be available at http://localhost:7474

#### Returned data
One thing worth noting when working with a query is the uniqueness of the data returned. For example lets look at the query below which forms the basis of the **AD Deep Paths** report. For reference _p_ stands for _path_, _n_ stands for _node_.

```cypher
MATCH p=(n:AD_Group)-[:AD_MEMBER_OF*5..]->(:AD_Group) RETURN p
```

If any group is part of multiple paths returned by this query, it will appear multiple times (i.e. in multiple rows) in the resulting report, which is often undesireable. 

Additionally, we don't need to return the edges/relationships in our report. 

To remedy this, we can unwind the resulting list of nodes and then return only distinct (unique) nodes. We do this by replacing this:

```cypher
RETURN p
```

With this:

```cypher
WITH nodes(p) AS items UNWIND items as item RETURN DISTINCT item
```

This translates to: "With all the nodes in the resulting path *p* collected together, which we will assign to a variable *items*, unwind that list into individual items, with each item assigned to a variable *item*. Return to me each distinct/unique *item* (discard duplicates)."

This approach works well in most cases. Replace _item_ and _items_ from the Cypher above with more descriptive terms as desired. 

The full query from above ends up as follows:
```cypher
MATCH p=(n:AD_Group)-[:AD_MEMBER_OF*5..]->(:AD_Group) WITH nodes(p) AS groups UNWIND groups as group RETURN DISTINCT group
```
---

### Property Filters
The **_propertyFilters_** field defines the names of the properties that will be displayed as columns in the report view by default, and the order in which they appear. 

The property names may come from multiple data types. If a row of a report is for a node that doesn't contain the specified property, the report will show an empty cell.

Displayed properties can be altered in the report view by the user, but the order the properties is listed will remain the same.

---

### Report Schema

```json
"reports": {
    "$report_name": {
      "displayName": "$report_display_name",
      "description": "$report_description",
      "query": "$report_cypher_query",
      "propertyFilters": [ "$list_displayed_property_name1", "$list_displayed_property_name2" ]
    },
    ....
}
```

### Example Report
```json
"reports": {
    "ad-deeppaths": {
      "displayName": "AD Deep Paths",
      "description": "Lists Active Directory groups that are part of 'Member Of' chains with 5 hops or more",
      "query": "MATCH p=(n:AD_Group)-[:AD_MEMBER_OF*5..]->(:AD_Group) WITH nodes(p) AS groups UNWIND groups as group RETURN DISTINCT group",
      "propertyFilters": [ "name", "dn", "description", "scope" ]
    }
}
```