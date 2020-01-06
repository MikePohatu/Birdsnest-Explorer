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

// Steps through x,y coordinates so the points end up in a spiral/grid around the initial x,y
export default class Spiral {
    StepDistance: number;
    x: number = 0;
    y: number = 0;
    private maincounter: number = -1;
    private directionalcounter: number = -1;
    private inverter: number = -1; //this is either 1 or -1 to reverse direction
    private xy: boolean = false; //working on x or y. x is true

    constructor(distance: number) {
        this.StepDistance = distance;
    }

    Step() {
        if (this.xy === true) {
            if (this.maincounter === this.directionalcounter) {
                this.directionalcounter = 0;
                this.xy = !this.xy;
                this.y = this.y + (this.StepDistance * this.inverter);
            }
            else {
                this.directionalcounter++;
                this.x = this.x + (this.StepDistance * this.inverter);
            }
        }
        else {
            if (this.maincounter === this.directionalcounter) {
                this.directionalcounter = 0;
                this.inverter = this.inverter * -1;
                this.xy = !this.xy;
                this.maincounter++;
                this.x = this.x + (this.StepDistance * this.inverter);
            }
            else {
                this.directionalcounter++;
                this.y = this.y + (this.StepDistance * this.inverter);
            }
        }
    }

    Reset() {
        this.x = 0;
        this.y = 0;
        this.maincounter = -1;
        this.directionalcounter = -1;
        this.inverter = -1;
        this.xy = false;
    }
}