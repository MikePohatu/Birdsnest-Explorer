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
import { SimulationNodeDatum } from "d3-force";
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import { RelatedDetails } from '@/assets/ts/dataMap/visualizer/RelatedDetails';

export interface SimNode extends SimulationNodeDatum {
    currentX: number;   //d3 works on x and y values, sometimes other during transitions.
    currentY: number;   //'current' values are referenced by vue to get control of timing
    currentSize: number;

    itemType: string;
    name: string;
    labels: string[];
    scope: number;
    properties: Dictionary<string | number | boolean>;
    size: number;
    dbId: string;
    y: number;
    x: number;
    cy: number;
    cx: number;
    k: number;
    pinned: boolean;
    dragged: boolean;
    startx: number;
    starty: number;
    radius: number;
    selected: boolean;
    enabled: boolean;
    relatedDetails: RelatedDetails;
    scale: number;
    isTreeRoot: boolean;
    isConnected: boolean;
}