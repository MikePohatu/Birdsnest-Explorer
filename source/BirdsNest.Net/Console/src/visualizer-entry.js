import $ from 'jquery';
import 'foundation-sites';
import AdvancedSearchCoordinator from './Visualizer/ts/AdvancedSearchCoordinator';
import * as log from 'loglevel';
import * as vis from './Visualizer/js/visualizer';
import '@fortawesome/fontawesome-free/js/fontawesome';
import '@fortawesome/fontawesome-free/js/solid';
import '@fortawesome/fontawesome-free/js/regular';

log.setLevel('trace', false);

$(document).foundation(); 

//temporary to dealing with legacy crap
global.vis = vis;

var paramdata = $("viewdataLoadIDs").value;
vis.drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

