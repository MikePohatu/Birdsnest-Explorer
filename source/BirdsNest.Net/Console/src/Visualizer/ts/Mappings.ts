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