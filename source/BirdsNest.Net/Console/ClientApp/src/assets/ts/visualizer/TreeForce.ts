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

import { SimLink } from "./SimLink";
import { SimNode } from "./SimNode";

export default function (links?: SimLink<SimNode>[]) {
    if (links == null) { links = []; }

    function force (alpha: number) {
        //check the tree nodes and shunt up or down to get into a tree layout. Exit the function if 
        //node is fixed i.e. has a fy (fixed y) property (https://github.com/d3/d3-force)
        links.forEach((d: SimLink<SimNode>) => {
            if (d.enabled) {
                const src = d.source as SimNode;
                const tar = d.target as SimNode;
                const targetY = src.y + (src.radius * 5);

                if (tar.fy == null) {
                    if (targetY > tar.y) {
                        tar.y = targetY;
                    }
                    const diffY = tar.y - targetY;

                    tar.vy = diffY * alpha;
                    tar.y -= tar.vy;
                }

                if (tar.fx == null) {
                    const diffx = Math.abs(tar.x - src.x);
                    if (diffx > (tar.size + src.size)) {
                        tar.vx -= (1/diffx) * alpha;
                        tar.x += tar.vx;
                    }
                }
            }
        });
    }

    force.links = function(_) {
        return arguments.length ? (links = _, force) : links;
    };

    return force;
}