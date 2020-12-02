# Birdsnest Explorer Administration

* [Birdsnest Explorer Administration](#birdsnest-explorer-administration)
  * [Overview](#overview)
  * [Reload Plugins](#reload-plugins)
  * [Index Editor](#index-editor)

## Overview

The Birdsnest Explorer Admin page will give admin users access to functions that will control the backend. This includes tools to reload [Console Plugins](/documentation/console/plugins/README.md) and add/remove database [indexes](https://neo4j.com/docs/cypher-manual/3.5/schema/index/). Additionally, the active plugins are listed. Where available, the displayed plugin will link to the relevant documentation.

![admin-page](/documentation/image/console/admin-page.png)

## Reload Plugins

If you have changed a console plugin, you can reload the plugins without restarting the Birdsnest Explorer application pool in IIS. 

## Index Editor

The index editor lists the data types defined in the loaded [console plugins](/documentation/console/plugins/README.md), and gives an option to enable/disable an index. An index cannot be edited if it is enforced by the [plugin](/documentation/console/plugins/README.md) (e.g. because it is required for the scanner to perform correctly), or because it exists because it is defined as a [constraint](https://neo4j.com/docs/cypher-manual/3.5/schema/constraints/). 

![index-editor](/documentation/image/console/index-editor.png)