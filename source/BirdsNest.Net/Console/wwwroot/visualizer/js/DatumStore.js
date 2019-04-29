//object to store datums for constant time insertion and delete and to abstract the handling of this data
function datumStore() {
    this.datumObject = new Object();
    this.datumArray = [];
    this.idArray = [];
    this.objectUpdated = false;
}

datumStore.prototype.GetDatum = function (id) {
    return this.datumObject[id];
};

datumStore.prototype.GetIDs = function (id) {
    if (this.objectUpdated === true) {
        //onsole.log(this.datumObject);
        this.RefreshArrays();
    }
    return this.idArray;
};

datumStore.prototype.GetArray = function () {

    if (this.objectUpdated === true) {
        //onsole.log(this.datumObject);
        this.RefreshArrays();
    }
    return this.datumArray;
};

datumStore.prototype.RefreshArrays = function () {
    let o = this.datumObject;
    this.idArray = Object.keys(o);
    this.datumArray = this.idArray.map(function (key) {
        //console.log("datumStore.GetArray map: " + key);
        //console.log(o);
        return o[key];
    });
    this.objectUpdated = false;
};

datumStore.prototype.Add = function (d) {
    //console.log("datumStore.Add: " + d.db_id);
    this.datumObject[d.db_id] = d;
    this.objectUpdated = true;
};

datumStore.prototype.Remove = function (d) {
    delete this.datumObject[d.db_id];
    this.objectUpdated = true;
};

datumStore.prototype.DatumExists = function (d) {
    if (this.datumObject.hasOwnProperty(d.db_id)) { return true; }
    else { return false; }
};


datumStore.prototype.KeyExists = function (key) {
    if (this.datumObject.hasOwnProperty(key)) { return true; }
    else { return false; }
};