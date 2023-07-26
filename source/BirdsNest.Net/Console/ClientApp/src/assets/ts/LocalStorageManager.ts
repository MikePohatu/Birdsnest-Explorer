// Copyright (c) 2019-2023 "20Road"
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
import { ResultSet } from "./dataMap/ResultSet";
import { ApiNode } from "./dataMap/ApiNode";
import { Notify } from "./Notifications";

const types = {
    PendingResults: "birdsnest.pendingResults",
    PendingNodeList: "birdsnest.pendingNodeList",
    AppendResults: "birdsnest.appendResults",
}

export default class LStore {

    // pending ResultSet gets imported into pendingResults in the store on page load
    static storePendingResultSet(results: ResultSet): void {
        if (window.localStorage) {
            try {
                localStorage.setItem(types.PendingResults, JSON.stringify(results));
            }
            catch {
                Notify.Error("Error saving results to local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error( "No Web Storage support");
        }
    }

    static popPendingResultSet(): ResultSet {
        let results: ResultSet = null;
        if (window.localStorage) {
            try {
                results = JSON.parse(localStorage.getItem(types.PendingResults));
                localStorage.removeItem(types.PendingResults);
                return results;
            }
            catch {
                Notify.Error("Error retrieving results from local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error("No Web Storage support");
        }

        return results;
    }

    // pending node list gets imported into pendingResults in the store on page load
    static storePendingNodeList(nodes: ApiNode[]): void {
        if (window.localStorage) {
            try {
                localStorage.setItem(types.PendingNodeList, JSON.stringify(nodes));
            }
            catch {
                Notify.Error("Error saving node list to local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error("No Web Storage support");
        }
    }

    static popPendingNodeList(): ApiNode[] {
        let results: ApiNode[] = null;
        if (window.localStorage) {
            try {
                results = JSON.parse(localStorage.getItem(types.PendingNodeList));
                localStorage.removeItem(types.PendingNodeList);
                return results;
            }
            catch {
                Notify.Error("Error retrieving results from local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error( "No Web Storage support");
        }

        return results;
    }


    // append ResultSet is for appending to an existing visualizer/graph instance
    static storeAppendResultSet(results: ResultSet): void {
        if (window.localStorage) {
            try {
                localStorage.setItem(types.AppendResults, JSON.stringify(results));
            }
            catch {
                Notify.Error("Error saving results to local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error("No Web Storage support");
        }
    }

    static popAppendResultSet(): ResultSet {
        let results: ResultSet = null;
        if (window.localStorage) {
            try {
                results = JSON.parse(localStorage.getItem(types.AppendResults));
                localStorage.removeItem(types.AppendResults);
                return results;
            }
            catch {
                Notify.Error("Error retrieving results from local store");
            }
        } else {
            // eslint-disable-next-line
            console.log("No Web Storage support..");
            Notify.Error("No Web Storage support");
        }

        return results;
    }
}