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
import webcrap from "@/assets/ts/webcrap/webcrap";

export default class Mappings {
    mappings: object;
    defaultvalue: object;

    constructor(jsondata, defaultval) {
        this.mappings = jsondata;
        this.defaultvalue = defaultval;
    }

    getMappingFromValue(value) {
        if (Object.prototype.hasOwnProperty.call(this.mappings, value)) {
            if (webcrap.misc.isNullOrWhitespace(this.mappings[value])) {
                return this.defaultvalue;
            } else {
                return this.mappings[value];
            }
        }

        return this.defaultvalue;
    }

    getMappingFromArray(array) {
        //console.log("Mappings.prototype.getMappingFromArray start:");
        //console.log(array);
        //console.log(this);
        let finalmapping = this.defaultvalue;

        const BreakException = {};
        try {
            //double loop because the order of this.mappings is the order that matters
            Object.keys(this.mappings).forEach((mapping) => {
                array.forEach((val) => {
                    //console.log(mapping);
                    //console.log(val);
                    if (val === mapping) {
                        if (!webcrap.misc.isNullOrWhitespace(this.mappings[mapping])) {
                            finalmapping = this.mappings[mapping];
                            throw BreakException;
                        } 
                    }

                });
            });
        }
        catch (e) {
            if (e !== BreakException) throw e;
        }
        //console.log("finalmapping" + finalmapping);
        return finalmapping;
    }
}