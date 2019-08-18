"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
//object to store datums for constant time insertion and delete and to abstract the handling of this data
var DatumStore = /** @class */ (function () {
    function DatumStore() {
        this.datumObject = new Object();
        this.datumArray = [];
        this.idArray = [];
        this.objectUpdated = false;
    }
    DatumStore.prototype.GetDatum = function (id) {
        return this.datumObject[id];
    };
    DatumStore.prototype.GetIDs = function (id) {
        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.idArray;
    };
    DatumStore.prototype.GetArray = function () {
        if (this.objectUpdated === true) {
            //onsole.log(this.datumObject);
            this.RefreshArrays();
        }
        return this.datumArray;
    };
    DatumStore.prototype.RefreshArrays = function () {
        var o = this.datumObject;
        this.idArray = Object.keys(o);
        this.datumArray = this.idArray.map(function (key) {
            //console.log("datumStore.GetArray map: " + key);
            //console.log(o);
            return o[key];
        });
        this.objectUpdated = false;
    };
    DatumStore.prototype.Add = function (d) {
        //console.log("datumStore.Add: " + d.db_id);
        this.datumObject[d.db_id] = d;
        this.objectUpdated = true;
    };
    DatumStore.prototype.Remove = function (d) {
        delete this.datumObject[d.db_id];
        this.objectUpdated = true;
    };
    DatumStore.prototype.DatumExists = function (d) {
        if (this.datumObject.hasOwnProperty(d.db_id)) {
            return true;
        }
        else {
            return false;
        }
    };
    DatumStore.prototype.KeyExists = function (key) {
        if (this.datumObject.hasOwnProperty(key)) {
            return true;
        }
        else {
            return false;
        }
    };
    return DatumStore;
}());
exports.default = DatumStore;
//# sourceMappingURL=DatumStore.js.map