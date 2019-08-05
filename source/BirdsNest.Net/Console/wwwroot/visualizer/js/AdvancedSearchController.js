//requires d3js
function AdvancedSearchController(elementid) {
    this.AdvancedSearch = new AdvancedSearch();
    this.ElementID = elementid;
}

AdvancedSearchController.prototype.AddNode = function () {
    //console.log("AdvancedSearchController.prototype.onAddButtonPressed started");
    var me = this;
    var radius = 30;

    var xspacing = 150;
    var yspacing = 70;
    //console.log(me);
    me.AdvancedSearch.AddNode();

    var viewel = d3.select("#" + me.ElementID);
    var newnodeg = viewel.selectAll(".searchnode")
        .data(me.AdvancedSearch.Nodes, function (d) { return d.Name; })
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchnode_" + d.Name; })
        .classed("searchnode",true)
        .attr("width", radius)
        .attr("height", radius)
        .attr("transform", function (d) { return "translate(" + (xspacing * me.AdvancedSearch.AddedNodes - xspacing * 0.5) + "," + yspacing + ")"; })
        .attr("data-open", "searchNodeDetails")
        .on("click", me.onSearchNodeClicked);

    newnodeg.append("circle")
        .attr("id", function (d) { return "searchnodebg_" + d.Name; })
        .attr("r",radius);

    newnodeg.append("text")
        .text(function (d) { return d.Name; })
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "central");
        //.attr("transform", function (d) { return "translate(0," + (radius + 10) + ")"; });

    if (me.AdvancedSearch.Nodes.length > 1) {
        var rectwidth = radius;
        var rectheight = radius *0.7;
        var subspacingx = ((me.AdvancedSearch.AddedNodes - 0.5) * xspacing - rectwidth / 2 - xspacing * 0.5);
        var subspacingy = yspacing - rectheight / 2;
        var relarrowwidth = 20;

        var newedgeg = viewel.selectAll(".searchedge")
            .data(me.AdvancedSearch.Edges, function (d) { return d.Name; })
            .enter()
            .append("g")
            .attr("id", function (d) { return "searchedge_" + d.Name; })
            .classed("searchedge", true)
            .on("click", me.onSearchEdgeClicked)
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

        viewel.selectAll(".searchleftarrow")
            .data(me.AdvancedSearch.Edges, function (d) { return d.Name; })
            .enter()
            .append("i")
            .attr("class", function (d) {
                if (d.Direction === ">") {
                    return "fas fa-minus";
                }
                else { return "fas fa-less-than"}
            })
            .classed("searcharrow", true)
            .attr("x", subspacingx - relarrowwidth)
            .attr("y", yspacing - relarrowwidth / 2)
            .attr("width", relarrowwidth)
            .attr("height", relarrowwidth);

        viewel.selectAll(".searchrightarrow")
            .data(me.AdvancedSearch.Edges, function (d) { return d.Name; })
            .enter()
            .append("i")
            .attr("class", function (d) {
                if (d.Direction === "<") { return "fas fa-minus"; }
                else { return "fas fa-arrow-right"; }
            })
            .classed("searcharrow", true)
            .attr("x", subspacingx + rectwidth)
            .attr("y", yspacing - relarrowwidth / 2)
            .attr("width", relarrowwidth)
            .attr("height", relarrowwidth);
    }
};


AdvancedSearchController.prototype.onSearchNodeClicked = function () {
    //console.log("AdvancedSearchController.prototype.onSearchNodeClicked started");
    //console.log(this);
    var datum = AdvancedSearchController.prototype.UpdateItemDatum("searchNodeDetails", this);
    document.getElementById("nodeType").value = datum.Label;
};

AdvancedSearchController.prototype.onSearchEdgeClicked = function () {
    //console.log("AdvancedSearchController.prototype.onSearchEdgeClicked started");
    //console.log(this);
    var datum = AdvancedSearchController.prototype.UpdateItemDatum("searchEdgeDetails", this);
    document.getElementById("edgeType").value = datum.Label;
    document.getElementById("sliderMin").value = datum.Min;
    document.getElementById("sliderMax").value = datum.Max;
};

AdvancedSearchController.prototype.UpdateItemDatum = function (elementid, callingitem) {
    //console.log("AdvancedSearchController.prototype.SearchItemClicked started");
    //console.log(this);
    //console.log(elementid);
    var details = d3.select("#" + elementid);
    var datum = d3.select(callingitem).datum();
    //console.log(datum);
    details.datum(datum);
    return datum;
};

AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked = function () {
    //console.log("AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);
    var node = d3.select("#searchNodeDetails").datum();
    var nodeEl = d3.select("#searchnode_" + node.Name);

    console.log(node);

    if (node.Label !== "") {
        nodeEl.classed(node.Label, false);
    }
    

    node.Name = node.Name;
    node.Label = document.getElementById("nodeType").value;
    console.log(node);

    nodeEl
        .attr("id", "searchnode_" + node.Name)
        .classed(node.Label, true);
};

AdvancedSearchController.prototype.onSearchEdgeSaveBtnClicked = function () {
    console.log("AdvancedSearchController.prototype.onSearchNodeSaveBtnClicked started");
    console.log(this);
    var edge = d3.select("#searchEdgeDetails").datum();
    var edgeEl = d3.select("#searchedge_" + edge.Name);

    console.log(edge);

    if (edge.Label !== "") {
        edgeEl.classed(edge.Label, false);
    }


    edge.Name = edge.Name;
    edge.Label = document.getElementById("edgeType").value;
    edge.Min = document.getElementById("sliderMin").value;
    edge.Max = document.getElementById("sliderMax").value;
    console.log(edge);

    edgeEl
        .attr("id", "searchedge_" + edge.Name)
        .classed(edge.Label, true);
};