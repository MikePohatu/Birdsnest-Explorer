import * as $ from 'jquery';
import { drawGraph } from './Visualizer/script/visualizer';
import AdvancedSearchCoordinator from './Visualizer/script/AdvancedSearchCoordinator';

var paramdata = $("viewdataLoadIDs").value;
drawGraph('drawingpane', paramdata);

new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");