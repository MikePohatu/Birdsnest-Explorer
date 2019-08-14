function Search() {
    this.Condition;
    this.Nodes = [];
    this.Edges = [];
    this.AddedNodes = 0;
}

Search.prototype.AddNode = function () {
    //console.log("Search.prototype.AddNode start:");
    //console.log(this);
    var me = this;
    me.AddedNodes++;
    var newNode = new SearchNode();
    this.Nodes.push(newNode);
    newNode.Name = "node" + me.AddedNodes;

    if (me.Nodes.length > 1) {
        var newEdge = new SearchEdge();
        this.Edges.push(newEdge);
        newEdge.Name = "hop" + (me.AddedNodes - 1);
    }

    return newNode;
    //console.log("Search.prototype.AddNode end:");
    //console.log(me);
};


//remove the node and return the index of the node that was removed
Search.prototype.RemoveNode = function (node) {
    var me = this;
    var foundindex = -1;
    for (i = 0; i < me.Nodes.length; i++) {
        if (foundindex !== -1) { //if found already, start shifting nodes back on in the array
            me.Nodes[i - 1] = me.Nodes[i];
            if (i < me.Edges.length) { me.Edges[i - 1] = me.Edges[i]; }
        }
        else {
            if (me.Nodes[i] === node) {
                foundindex = i;
                if (i === 0) {
                    me.Nodes.shift(); //remove the first node
                    if (me.Edges.length > 0) { me.Edges.shift(); } //if there is an edge, remove that too

                    //we're done
                    return foundindex;
                }
            }
        }
    }

    if (foundindex !== -1) {
        // pop off the end if the node wasn't first i.e hasn't been removed with shift()
        me.Nodes.pop();
        me.Edges.pop();
    }

    return foundindex;
};

Search.prototype.MoveNodeRight = function (node) {
    var me = this;
    var i;
    for (i = 0; i < me.Nodes.length; i++) {
        if (me.Nodes[i] === node) {
            if (i === me.Nodes.length - 1) {
                return false; //can't move any further
            }
            else {
                me.Nodes[i] = me.Nodes[i + 1];
                me.Nodes[i + 1] = node;
                return true;
            }
        }
    }
    return false;
};

Search.prototype.MoveNodeLeft = function (node) {
    var me = this;
    var i;

    for (i = 0; i < me.Nodes.length; i++) {
        if (me.Nodes[i] === node) {
            if (i === 0) {
                return false; //can't move any further
            }
            else {
                me.Nodes[i] = me.Nodes[i - 1];
                me.Nodes[i - 1] = node;
                return true;
            }
        }
    }
    return false;
};

function SearchNode() {
    this.Name = "";
    this.Label = "";
}

function SearchEdge() {
    this.Name = "";
    this.Label = "";
    this.Direction = ">";
    this.Min = 1;
    this.Max = 1;
}

function MathCondition() {
    this.Type = "MATH";
    this.Name = "";
    this.Property = "";
    this.Value = 0;
    this.Operator = "=";
}

function AndCondition() {
    this.Type = "AND";
    this.Name = "";
    this.Conditions = [];
}

function OrCondition() {
    this.Type = "OR";
    this.Name = "";
    this.Conditions = [];
}

function StringCondition() {
    this.Type = "STRING";
    this.Name = "";
    this.Property = "";
    this.Value = "";
    this.Comparator = "=";
}

function RegExCondition() {
    this.Type = "REGEX";
    this.Name = "";
    this.Property = "";
    this.Value = "";
}