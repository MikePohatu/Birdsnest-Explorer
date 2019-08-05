function AdvancedSearch() {
    this.Condition;
    this.Nodes = [];
    this.Edges = [];
    this.AddedNodes = 0;
}

AdvancedSearch.prototype.AddNode = function () {
    //console.log("AdvancedSearch.prototype.AddNode start:");
    //console.log(this);
    var me = this;
    me.AddedNodes++;
    var newNode = new SearchNode();
    this.Nodes.push(newNode);
    newNode.Name = "n" + me.AddedNodes;

    if (me.Nodes.length > 1) {
        var newEdge = new SearchEdge();
        this.Edges.push(newEdge);
        newEdge.Name = "r" + (me.AddedNodes - 1);
    }

    return newNode;
    //console.log("AdvancedSearch.prototype.AddNode end:");
    //console.log(me);
};

AdvancedSearch.prototype.RemoveNode = function (node) {
    var me = this;
    var found = false;
    for (i = 0; i < me.Nodes.length; i++) {
        if (found) { //if found already, start shifting nodes back on in the array
            me.Nodes[i - 1] = me.Nodes[i];
            if (i < me.Edges.length) { me.Edges[i - 1] = me.Edges[i]; }
        }
        else {
            if (me.Nodes[i] === node) {
                found = true;
                if (i === 0) {
                    me.Nodes.shift(); //remove the first node
                    if (me.Edges.length > 0) { me.Edges.shift(); } //if there is an edge, remove that too

                    //we're done
                    return found;
                }
            }
        }
    }

    if (found) {
        // pop off the end if the node wasn't first i.e hasn't been removed with shift()
        me.Nodes.pop();
        me.Edges.pop();
    }

    return found;
};

AdvancedSearch.prototype.MoveNodeRight = function (node) {
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

AdvancedSearch.prototype.MoveNodeLeft = function (node) {
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
    this.Min = -1;
    this.Max = -1;
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
    this.Left;
    this.Right;
}

function OrCondition() {
    this.Type = "OR";
    this.Left;
    this.Right;
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