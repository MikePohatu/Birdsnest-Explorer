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

//object to store datums for constant time insertion and delete and to abstract the handling of this data
export default class DatumStore {
    datumObject: object;
    datumArray: object[];
    idArray: string[];
    objectUpdated: boolean;

    constructor() {
        this.datumObject = new Object();
        this.datumArray = [];
        this.idArray = [];
        this.objectUpdated = false;
    }

    GetDatum(id): object {
        return this.datumObject[id];
    }

    GetIDs(): string[] {
        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.idArray;
    }

    GetArray(): object[] {

        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.datumArray;
    }

    RefreshArrays(): void {
        let o = this.datumObject;
        this.idArray = Object.keys(o);
        this.datumArray = this.idArray.map(function (key) {
            //console.log("datumStore.GetArray map: " + key);
            //console.log(o);
            return o[key];
        });
        this.objectUpdated = false;
    }

    Add(d): void {
        //console.log("datumStore.Add: " + d.db_id);
        this.datumObject[d.db_id] = d;
        this.objectUpdated = true;
    }

    Removes(d): void {
        if (this.DatumExists(d)) {
            delete this.datumObject[d.db_id];
            this.objectUpdated = true;
        }
    }

    DatumExists(d): boolean {
        if (this.datumObject.hasOwnProperty(d.db_id)) { return true; }
        else { return false; }
    }


    KeyExists(key): boolean {
        if (this.datumObject.hasOwnProperty(key)) { return true; }
        else { return false; }
    }
}
