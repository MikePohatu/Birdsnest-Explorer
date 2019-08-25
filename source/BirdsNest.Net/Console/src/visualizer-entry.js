import * as vis from './Visualizer/js/visualizer';
import AdvancedSearchCoordinator from './Visualizer/ts/AdvancedSearchCoordinator';

import $ from 'jquery';

setTimeout(function () {
    $(document).foundation();
}, 200);

//temporary to dealing with legacy crap
global.vis = vis;

var paramdata = $("viewdataLoadIDs").value;
vis.drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

