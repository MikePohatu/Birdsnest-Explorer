import * as $ from 'jquery';
import Foundation from 'foundation-sites';
import { drawGraph } from './Visualizer/visualizer';
import AdvancedSearchCoordinator from './Visualizer/script/AdvancedSearchCoordinator';



$(document).foundation();

var paramdata = $("viewdataLoadIDs").value;
drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

