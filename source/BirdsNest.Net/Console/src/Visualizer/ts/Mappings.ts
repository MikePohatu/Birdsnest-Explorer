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
export default class Mappings {
    mappings: object;
    defaultvalue: object;

    constructor(jsondata, defaultval) {
        this.mappings = jsondata;
        this.defaultvalue = defaultval;
    }

    getMappingFromValue(value) {
        var me = this;
        var finalmapping = this.defaultvalue;

        if (me.mappings.hasOwnProperty(value)) {
            //console.log("found: " + me.mappings[value]);
            finalmapping = me.mappings[value];
        }

        return finalmapping;
    }

    getMappingFromArray(array) {
        //console.log("Mappings.prototype.getMappingFromArray start:");
        //console.log(array);
        //console.log(this);
        var me = this;
        var finalmapping = this.defaultvalue;

        var BreakException = {};
        try {
            //double loop because the order of this.mappings is the order that matters
            Object.keys(me.mappings).forEach(function (mapping) {
                array.forEach(function (val) {
                    //console.log(mapping);
                    //console.log(val);
                    if (val === mapping) {
                        finalmapping = me.mappings[mapping];
                        throw BreakException;
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