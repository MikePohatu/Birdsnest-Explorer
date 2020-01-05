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

export default class DataQueue {
    QueuedData: object;
    IsProcessing: boolean;
    AreResultsPending: boolean;
    Timeout: any;
    ProcessingFunction: any;

    constructor (procFunc) {
        this.QueuedData = new Object();
        this.IsProcessing = false;
        this.AreResultsPending = false;
        this.Timeout;
        this.ProcessingFunction = procFunc;
    }

    QueueResults (json) {
        //console.log("ResultsQueue.prototype.QueueResults start: ");
        //console.log(json);
        var me = this;
        me.AreResultsPending = true;

        for (var propertyName in json) {
            if (me.QueuedData.hasOwnProperty(propertyName) === false) {
                //console.log("New nodes");
                me.QueuedData[propertyName] = json[propertyName];
            }
            else if (Array.isArray(me.QueuedData[propertyName])) {
                //console.log("Concat nodes: " + propertyName);
                me.QueuedData[propertyName] = me.QueuedData[propertyName].concat(json[propertyName]);
            }
            else {
                me.QueuedData[propertyName] = json[propertyName];
            }
        }

        //console.log("QueuedData");
        //console.log(me.QueuedData);
        if (me.IsProcessing === false) {
            clearTimeout(me.Timeout);
            me.Timeout = setTimeout(function () {
                me.Process();
            }, 1000);
        }
    }

    Process () {
        //console.log("ResultsQueue.prototype.Process start:");
        //console.log(this.QueuedData);
        var me = this;
        me.IsProcessing = true;
        var jsonProcessing = me.QueuedData;
        me.QueuedData = new Object();
        me.AreResultsPending = false;
        me.ProcessingFunction(jsonProcessing, function () {
            me.IsProcessing = false;
            if (me.AreResultsPending === true) {
                me.Process();
            }
        });
    }
}
