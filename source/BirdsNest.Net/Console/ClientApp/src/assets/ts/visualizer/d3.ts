// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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

//https://github.com/d3/d3-selection/issues/212

//this script looks after pulling in the parts of d3 that are needed for visualizer.
//this helps reduce the size of d3

import { event, select, selectAll, mouse } from "d3-selection";
import { zoom, zoomTransform } from "d3-zoom";
import { drag } from "d3-drag";
import { forceLink, forceSimulation, forceCollide, forceManyBody } from "d3-force";
import { transition } from "d3-transition";
import { easeCubicInOut, easeCubic } from "d3-ease";
import { interpolateNumber } from "d3-interpolate";

export const d3 = {
    get event() { return event; }, select, selectAll, mouse,
    zoom, zoomTransform,
    drag,
    forceLink, forceSimulation, forceCollide, forceManyBody, 
    transition,
    easeCubicInOut, easeCubic,
    interpolateNumber
}