import * as vis from './Visualizer/js/visualizer';
import AdvancedSearchCoordinator from './Visualizer/ts/AdvancedSearchCoordinator';

import $ from 'jquery';
import 'foundation-sites';
//import 'foundation-sites/dist/css/foundation.css';


import * as log from 'loglevel';

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

