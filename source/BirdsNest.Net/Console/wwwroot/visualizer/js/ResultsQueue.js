
function ResultsQueue(procFunc) {
    this.jsonQueued = new Object();
    this.processing = false;
    this.pendingResults = false;
    this.timeout;
    this.processingFunction = procFunc;
}

ResultsQueue.prototype.QueueResults = function (json) {
    //console.log("ResultsQueue.prototype.QueueResults start: ");
    //console.log(json);
    var me = this;
    me.pendingResults = true;
    if (me.jsonQueued.hasOwnProperty('Edges') === false) {
        me.jsonQueued.Edges = json.Edges;
    }
    else {
        me.jsonQueued.Edges = me.jsonQueued.Edges.concat(json.Edges);
    }

    if (me.jsonQueued.hasOwnProperty('Nodes') === false) {
        //console.log("New nodes");
        me.jsonQueued.Nodes = json.Nodes;
    }
    else {
        //console.log("Concat nodes");
        me.jsonQueued.Nodes = me.jsonQueued.Nodes.concat(json.Nodes);
    }

    //console.log("jsonQueued");
    //console.log(me.jsonQueued);
    if (me.processing === false) {
        clearTimeout(me.timeout);
        me.timeout = setTimeout(function () {
            me.Process();
        }, 1000);
    }
};

ResultsQueue.prototype.Process = function () {
    //console.log("ResultsQueue.prototype.Process start:");
    //console.log(this.jsonQueued);
    var me = this;
    me.processing = true;
    var jsonProcessing = me.jsonQueued;
    me.jsonQueued = new Object();
    me.pendingResults = false;
    me.processingFunction(jsonProcessing, function () {
        me.processing = false;
        if (me.pendingResults === true) {
            me.Process();
        }
    });
};
