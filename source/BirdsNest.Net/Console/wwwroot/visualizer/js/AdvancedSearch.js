function Search() {
    this.Condition;
    this.Nodes = [];
    this.Edges = [];
}

Search.prototype.AddNode = function () {
    //console.log("Search.prototype.AddNode start:");
    //console.log(this);
    var me = this;
    var newNode = new SearchNode();
    this.Nodes.push(newNode);
    newNode.Name = "n" + this.Nodes.length;

    if (me.Nodes.length > 1) {
        var newEdge = new SearchEdge();
        this.Edges.push(newEdge);
        newEdge.Name = "r" + this.Edges.length;
    }

    //console.log("Search.prototype.AddNode end:");
    //console.log(me);
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

