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

//object to store datums for constant time insertion and delete and to abstract the handling of this data
export default class DatumStore<T> {
    private datumObject = new Object();
    idName = "dbId"; 
    Array: T[] = [];
    IDs: string[] = [];

    Add(d: T): void {
        //console.log("datumStore.Add: " + d.dbId);
        this.datumObject[d[this.idName]] = d;
        this.RefreshArrays();
    }

    Remove(d: T): void {
        if (this.DatumExists(d)) {
            delete this.datumObject[d[this.idName]];
        }
        this.RefreshArrays();
    }

    RemoveId(id: string): void {
        if (this.KeyExists(id)) {
            delete this.datumObject[id];
        }
        this.RefreshArrays();
    }

    Clear(): void {
        this.datumObject = new Object();
        this.Array = [];
        this.IDs = [];
        this.RefreshArrays();
    }

    private RefreshArrays = webcrap.misc.debounce(() => { 
        this.IDs = Object.keys(this.datumObject);
        this.Array = this.IDs.map((key)=> {
            //console.log("datumStore.GetArray map: " + key);
            //console.log(o);
            return this.datumObject[key];
        });
        console.log({source: "RefreshArrays", datumObject: this.datumObject, IDs: this.IDs, Array: this.Array});
    }, 10);

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
