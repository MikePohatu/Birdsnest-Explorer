import * as $ from 'jquery';
import Foundation from 'foundation-sites';
import * as vis from './Visualizer/visualizer';
import AdvancedSearchCoordinator from './Visualizer/script/AdvancedSearchCoordinator';
import * as log from 'loglevel';

log.setLevel('trace', false);


$(document).foundation();

var paramdata = $("viewdataLoadIDs").value;
vis.drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

