import * as vis from './Visualizer/js/visualizer';
import AdvancedSearchCoordinator from './Visualizer/ts/AdvancedSearchCoordinator';

import $ from 'jquery';

//temporary to dealing with legacy crap
global.vis = vis;

var paramdata = $("viewdataLoadIDs").value;
vis.drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

$(document).foundation();
