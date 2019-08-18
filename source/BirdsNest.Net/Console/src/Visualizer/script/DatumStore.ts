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

    GetDatum (id) {
        return this.datumObject[id];
    }

    GetIDs (id) {
        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.idArray;
    }

    GetArray () {

        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.datumArray;
    }

    RefreshArrays () {
        let o = this.datumObject;
        this.idArray = Object.keys(o);
        this.datumArray = this.idArray.map(function (key) {
            //console.log("datumStore.GetArray map: " + key);
            //console.log(o);
            return o[key];
        });
        this.objectUpdated = false;
    }

    Add (d) {
        //console.log("datumStore.Add: " + d.db_id);
        this.datumObject[d.db_id] = d;
        this.objectUpdated = true;
    }

    Remove (d) {
        delete this.datumObject[d.db_id];
        this.objectUpdated = true;
    }

    DatumExists (d) {
        if (this.datumObject.hasOwnProperty(d.db_id)) { return true; }
        else { return false; }
    }


    KeyExists (key) {
        if (this.datumObject.hasOwnProperty(key)) { return true; }
        else { return false; }
    }
}
