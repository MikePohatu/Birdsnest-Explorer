
function ResultsQueue(procFunc) {
    this.QueuedData = new Object();
    this.IsProcessing = false;
    this.AreResultsPending = false;
    this.Timeout;
    this.ProcessingFunction = procFunc;
}

ResultsQueue.prototype.QueueResults = function (json) {
    //console.log("ResultsQueue.prototype.QueueResults start: ");
    //console.log(json);
    var me = this;
    me.AreResultsPending = true;

    for (var propertyName in json) {
        if (me.QueuedData.hasOwnProperty(propertyName) === false) {
            //console.log("New nodes");
            me.QueuedData[propertyName] = json[propertyName];
        }
        else {
            //console.log("Concat nodes");
            me.QueuedData[propertyName] = me.QueuedData[propertyName].concat(json[propertyName]);
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
};

ResultsQueue.prototype.Process = function () {
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
};
