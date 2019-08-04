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
    newNode.Name = "n" + me.AddedNodes;

    if (me.Nodes.length > 1) {
        var newEdge = new SearchEdge();
        this.Edges.push(newEdge);
        newEdge.Name = "r" + (me.AddedNodes - 1);
    }

    return newNode;
    //console.log("Search.prototype.AddNode end:");
    //console.log(me);
};

Search.prototype.RemoveNode = function (node) {
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



//requires d3js
function AdvancedSearchController(elementid) {
    this.Search = new Search();
    this.ElementID = elementid;
}

AdvancedSearchController.prototype.AddNode = function () {
    //console.log("AdvancedSearchController.prototype.onAddButtonPressed started");
    var me = this;
    var radius = 30;
    var spacing = 140;
    //console.log(me);
    var newNode = me.Search.AddNode();

    var viewel = d3.select("#" + me.ElementID);
    var newnodeg = viewel.selectAll(".searchnode")
        .data(me.Search.Nodes, function (d) { return d.Name; })
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchnode_" + d.Name; })
        .classed("nodes", true)
        .classed("searchnode",true)
        .attr("width", radius)
        .attr("height", radius)
        .attr("transform", function (d) { return "translate(" + (spacing * me.Search.AddedNodes - spacing * 0.5) + "," + spacing + ")"; })
        .attr("data-open", "searchNodeDetails")
        .on("click", me.onSearchItemClicked("searchNodeDetails"));

    newnodeg.append("circle")
        .attr("id", function (d) { return "searchnodebg_" + d.Name; })
        .attr("r",radius);

    newnodeg.append("text")
        .text(function (d) { return d.Name; })
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "central");
        //.attr("transform", function (d) { return "translate(0," + (radius + 10) + ")"; });

    if (me.Search.Nodes.length > 1) {
        var rectwidth = radius;
        var rectheight = radius *0.7;
        var subspacingx = ((me.Search.AddedNodes - 0.5) * spacing - rectwidth / 2 - spacing * 0.5);
        var subspacingy = spacing - rectheight / 2;

        var newedgeg = viewel.selectAll(".searchedge")
            .data(me.Search.Edges, function (d) { return d.Name; })
            .enter()
            .append("g")
            .attr("id", function (d) { return "searchedge_" + d.Name; })
            .classed("nodes", true)
            .classed("searchedge", true)
            .on("click", me.onSearchItemClicked("searchEdgeDetails"))
            .attr("transform", function (d) { return "translate(" + subspacingx + "," + subspacingy + ")"; })
            .attr("data-open", "searchEdgeDetails");

        newedgeg.append("rect")
            .attr("id", function (d) { return "searchnodebg_" + d.Name; })
            .attr("width", rectwidth)
            .attr("height", rectheight);

        newedgeg.append("text")
            .text(function (d) { return d.Name; })
            .attr("text-anchor", "middle")
            .attr("dominant-baseline", "middle")
            .attr("x", rectwidth / 2)
            .attr("y", rectheight / 2);
    }
};


AdvancedSearchController.prototype.onSearchItemClicked = function (elementid) {
    console.log("AdvancedSearchController.prototype.onSearchItemClicked started");
    console.log(this);
    console.log(elementid);
    var details = d3.select("#" + elementid);
    var datum = d3.select(this).datum();
    console.log(datum);
    details.datum(datum);
};

AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked = function () {
    console.log("AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);
    var node = d3.select("#searchNodeDetails").datum();
    node.Name = node.Name + "testsave";
    node.Label = node.Label + "testsavelabel";
};

//AdvancedSearchController.prototype.onSearchEdgeClicked = function () {
//    //console.log("AdvancedSearchController.prototype.onSearchEdgeClicked started");
//    //console.log(this);
//    var details = d3.select("#searchEdgeDetails");
//    var datum = d3.select(this).datum();
//    console.log(datum);
//    details.datum(datum);
//};

AdvancedSearchController.prototype.onSearchEdgeSaveBtnClicked = function () {
    console.log("AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);
    var node = d3.select("#searchNodeDetails").datum();
    node.Name = node.Name + "testsave";
    node.Label = node.Label + "testsavelabel";
};