import * as $ from 'jquery';
import 'foundation-sites';
import { drawGraph } from './Visualizer/script/visualizer';
import AdvancedSearchCoordinator from './Visualizer/script/AdvancedSearchCoordinator';



var paramdata = $("viewdataLoadIDs").value;
drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");

$(document).foundation();
