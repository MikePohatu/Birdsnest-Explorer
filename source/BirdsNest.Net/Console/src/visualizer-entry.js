// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
import * as vis from './visualizer/js/visualizer';
import AdvancedSearchCoordinator from './visualizer/ts/AdvancedSearchCoordinator';

import $ from 'jquery';
import { webcrap } from "./shared/webcrap/webcrap";

//temporary to dealing with legacy crap
global.vis = vis;

var paramdata = $("viewdataLoadIDs").value;
vis.drawGraph('drawingpane', paramdata);

var searchcoordinator = new AdvancedSearchCoordinator("searchPathSvg", "searchConditionSvg");


var sharedsearchstring = document.getElementById("sharedSearchString").value;
//console.log("sharedsearchstring: " + sharedsearchstring);
if (webcrap.misc.isNullOrWhitespace(sharedsearchstring) === false) {
    try {
        var json = JSON.parse(webcrap.misc.decodeUrlB64(sharedsearchstring));
        //console.log(json);
        if (json) {
			searchcoordinator.LoadSearchJson(json);
        }
    }
    catch {
        console.error("Unable to parse shared search string");
    }
}

