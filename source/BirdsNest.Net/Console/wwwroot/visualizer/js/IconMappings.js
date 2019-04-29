function IconMappings(jsondata) {
    this.mappings = jsondata;
    this.keys = Object.keys(this.mappings);
}

IconMappings.prototype.getIconClass = function (datum) {
    //console.log("IconMappings.prototype.getIconClass start:");
    //console.log(datum);
    //console.log(this.mappings);
    var me = this;
    var finalmapping = "fas fa-question";
    var mappinglabel = "";
    var datumlabel = "";
    var found = false;
    for (i = 0; i < this.keys.length; i++) {
        mappinglabel = this.keys[i];
        for (j = 0; j < datum.labels.length; j++) {
            datumlabel = datum.labels[j];
            if (datumlabel === mappinglabel) {
                //console.log("return");
                finalmapping = me.mappings[mappinglabel];
                //console.log(finalmapping);
                found = true;
                break;
            }
        }
        if (found === true) { break; }
    }

    return finalmapping;
};