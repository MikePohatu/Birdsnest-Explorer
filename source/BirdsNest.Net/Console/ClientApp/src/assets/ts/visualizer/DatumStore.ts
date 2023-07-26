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

//object to store datums for constant time insertion and delete and to abstract the handling of this data
export default class DatumStore<T> {
    private datumObject = new Object();
    Updated = false;
    idName = "dbId"; 
    Array: T[] = [];
    IDs: string[] = [];


    get Count(): number {
        return Object.keys(this.datumObject).length;
    }

    //Commit function must be called when changes have been finished to update the arrays
    Add(d: T): DatumStore<T> {
        //console.log("datumStore.Add: " + d.dbId);
        this.datumObject[d[this.idName]] = d;
        this.Updated = true;
        return this;
    }

    //Commit function must be called when changes have been finished to update the arrays
    Remove(d: T): DatumStore<T> {
        if (this.DatumExists(d)) {
            delete this.datumObject[d[this.idName]];
            this.Updated = true;
        }
        return this;
    }

    //Commit function must be called when changes have been finished to update the arrays
    RemoveId(id: string): DatumStore<T> {
        if (this.KeyExists(id)) {
            delete this.datumObject[id];
            this.Updated = true;
        }
        return this;
    }

    Clear(): DatumStore<T> {
        this.datumObject = new Object();
        this.Array = [];
        this.IDs = [];
        this.Updated = false;
        return this;
    }

    Commit(): void { 
        if (this.Updated) {
            this.IDs = Object.keys(this.datumObject);
            this.Array = this.IDs.map((key)=> {
                //console.log("datumStore.GetArray map: " + key);
                //console.log(o);
                return this.datumObject[key];
            });
            this.Updated = false;
        } 
    }

    GetDatum(id: string): T {
        if (this.KeyExists(id)) {
            return this.datumObject[id];
        } else {
            return null;
        } 
    }

    DatumExists(d: T): boolean {     
        if (Object.prototype.hasOwnProperty.call(this.datumObject, d[this.idName])) { return true; }
        else { return false; }
    }


    KeyExists(key): boolean {
        if (Object.prototype.hasOwnProperty.call(this.datumObject, key)) { return true; }
        else { return false; }
    }
}
