//requires d3js
function AdvancedSearchCoordinator(elementid) {
    //console.log("AdvancedSearchCoordinator started: " + elementid);
    //console.log(this);
    var me = this;
    this.AdvancedSearch = new AdvancedSearch();
    this.ElementID = elementid;
    this.Radius = 30;
    this.XSpacing = 170;
    this.YSpacing = 70;

    bindEnterToButton("searchEdgeDetails", "searchEdgeSaveBtn");
    bindEnterToButton("searchNodeDetails", "searchNodeSaveBtn");

    //Bind the enter key for an element to click a button
    function bindEnterToButton(elementid, buttonid) {
        document.getElementById(elementid).addEventListener("keydown", function (event) {
            //console.log("keydown listener fired: " + elementid);
            // Number 13 is the "Enter" key on the keyboard
            if (event.keyCode === 13) {
                event.preventDefault();
                document.getElementById(buttonid).click();
            }
        });
    }

    //d3.select("#nodeType").on("change", this.UpdateNodeProps);
    d3.select("#searchNodeSaveBtn").on("click", function () {
        me.onSearchNodeSaveBtnClicked();
    });
    d3.select("#searchNodeDeleteBtn").on("click", function () {
        me.onSearchNodeDelBtnClicked(this);
    });
    d3.select("#searchEdgeSaveBtn").on("click", function () {
        me.onSearchEdgeSaveBtnClicked();
    });
    d3.select("#dirIconClicker").on("click", function () {
        me.ToggleDir();
    });
    d3.select("#searchBtn").on("click", function () {
        me.Search();
    });
    d3.select("#addIcon").on("click", function () {
        me.AdvancedSearch.AddNode();
        me.UpdateNodes();
        me.UpdateEdges();
    });
    d3.select("#advSearchClearIcon").on("click", function () {
        me.Clear();
    });
    d3.select("#hopsSwitch").on("change", function () {
        me.onHopsSwitchChanged();
    });
    

    me.UpdateNodeLabels();
}

AdvancedSearchCoordinator.prototype.Search = function () {
    //console.log("AdvancedSearchCoordinator.prototype.Search started");
    //console.log(this);
    

    var postdata = JSON.stringify(this.AdvancedSearch);
    //console.log("search post:");
    //console.log(postdata);
    showSearchSpinner();
    apiPostJson("AdvancedSearch", postdata, function (data) {
        //console.log(data);
        document.getElementById("searchNotification").innerHTML = data;
        $('#searchNotification').foundation();
    });
};

AdvancedSearchCoordinator.prototype.Clear = function () {
    //console.log("AdvancedSearchCoordinator.prototype.Clear started");
    //console.log(this);
    
    var viewel = d3.select("#" + this.ElementID);
    viewel.selectAll("*").remove();
    this.AdvancedSearch = new AdvancedSearch();
};

AdvancedSearchCoordinator.prototype.UpdateNodes = function () {
    //console.log("AdvancedSearchCoordinator.prototype.UpdateNodes started");
    //console.log(this);
    var me = this;
    var currslot = 0;

    var viewel = d3.select("#" + this.ElementID);
    var newnodeg = viewel.selectAll(".searchnode")
        .data(this.AdvancedSearch.Nodes, function (d) { return d.Name; })
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchnode_" + d.Name; })
        .classed("searchnode",true)
        .attr("width", this.Radius)
        .attr("height", this.Radius)
        .attr("data-open", "searchNodeDetails")
        .on("click", function () {
            me.onSearchNodeClicked(this);
        });

    viewel.selectAll(".searchnode")
        .data(this.AdvancedSearch.Nodes, function (d) { return d.Name; })
        .attr("transform", function () {
            currslot++;
            return "translate(" + (me.XSpacing * currslot - me.XSpacing * 0.5) + "," + me.YSpacing + ")";
        });

    newnodeg.append("circle")
        .attr("id", function (d) { return "searchnodebg_" + d.Name; })
        .classed("searchnodecircle",true)
        .attr("r",this.Radius);

    newnodeg.append("text")
        .text(function (d) { return d.Name; })
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "central");
        //.attr("transform", function (d) { return "translate(0," + (this.Radius + 10) + ")"; });
};

AdvancedSearchCoordinator.prototype.UpdateEdges = function () {
    console.log("AdvancedSearchCoordinator.prototype.UpdateEdges start");
    //console.log(this);
    //console.log(typeof slotnum);
    var me = this;

    var rectwidth = this.Radius*2;
    var rectheight = this.Radius * 0.7;
    
    var relarrowwidth = 20;
    var currslot = 0;
    //console.log("subspacingx: " + subspacingx);
    //console.log("subspacingy: " + subspacingy);

    var viewel = d3.select("#" + this.ElementID);
    var newedgeg = viewel.selectAll(".searchedge")
        .data(this.AdvancedSearch.Edges)
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchedge_" + d.Name; })
        .attr("class", function (d) { return d.Label; })
        .classed("searchedge", true)
        .on("click", function () { this.onSearchEdgeClicked(this); })
        .attr("data-open", "searchEdgeDetails");

    viewel.selectAll(".searchedge")
        .data(this.AdvancedSearch.Edges)
        .attr("transform", function () {
            currslot++;
            var subspacingx = (currslot + 0.5) * me.XSpacing - rectwidth / 2 - me.XSpacing * 0.5;
            var subspacingy = me.YSpacing - rectheight / 2;
            console.log(currslot + ": " + subspacingx + ": " + subspacingy);
            return "translate(" + subspacingx + "," + subspacingy + ")";
        });

    newedgeg.append("rect")
        .attr("id", function (d) { return "searchedgebg_" + d.Name; })
        .classed("searchedgerect", true)
        .attr("width", rectwidth)
        .attr("height", rectheight);

    newedgeg.append("text")
        .text(function (d) { return d.Name; })
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "middle")
        .attr("x", rectwidth / 2)
        .attr("y", rectheight / 2);

    newedgeg.append("i")
        .attr("id", function (d) { return "searchleftarr_" + d.Name; })
        .attr("class", function (d) {
            if (d.Direction === ">") { return "fas fa-minus"; }
            else { return "fas fa-arrow-left"; }
        })
        .classed("searchleftarrow", true)
        .classed("searcharrow", true)
        .attr("x", 0 - relarrowwidth)
        .attr("y", 0)
        .attr("width", relarrowwidth)
        .attr("height", relarrowwidth);

    newedgeg.append("i")
        .attr("id", function (d) { return "searchrightarr_" + d.Name; })
        .attr("class", function (d) {
            if (d.Direction === "<") { return "fas fa-minus"; }
            else { return "fas fa-arrow-right"; }
        })
        .classed("searchrightarrow", true)
        .classed("searcharrow", true)
        .attr("x", rectwidth)
        .attr("y", 0)
        .attr("width", relarrowwidth)
        .attr("height", relarrowwidth);
};


AdvancedSearchCoordinator.prototype.onSearchNodeClicked = function (callingEl) {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchNodeClicked started");
    //console.log(this);
    var datum = this.UpdateItemDatum("searchNodeDetails", callingEl);
    document.getElementById("nodeType").value = datum.Label;
    document.getElementById("nodeIdentifier").value = datum.Name;
};

AdvancedSearchCoordinator.prototype.onSearchEdgeClicked = function (callingEl) {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchEdgeClicked started");
    //console.log(this);
    var datum = this.UpdateItemDatum("searchEdgeDetails", callingEl);
    document.getElementById("edgeType").value = datum.Label;
    document.getElementById("edgeIdentifier").value = datum.Name;

    var hopsswitch = document.getElementById("hopsSwitch");
    var minsliderval = document.getElementById("minSliderVal");
    var maxsliderval = document.getElementById("maxSliderVal");

    this.UpdateDirIcon(datum.Direction, document.getElementById("dirIcon"));

    if (datum.Min < 0 || datum.Max < 0) {
        hopsswitch.checked = false;
        minsliderval.value = 1;
        maxsliderval.value = 1;
    }
    else {
        hopsswitch.checked = true;
        minsliderval.value = datum.Min;
        maxsliderval.value = datum.Max;
    }

    hopsswitch.dispatchEvent(new Event('change'));
    minsliderval.dispatchEvent(new Event('change'));
    maxsliderval.dispatchEvent(new Event('change'));
    //document.getElementById("sliderFill").dispatchEvent(new Event('input'));
};

AdvancedSearchCoordinator.prototype.onHopsSwitchChanged = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onHopsSwitchChanged started");
    var hopsswitch = document.getElementById("hopsSwitch");
    //console.log(hopsswitch.checked);

    if (hopsswitch.checked) {
        d3.selectAll(".hopscontrol")
            .attr("disabled", null);
    }
    else {
        d3.selectAll(".hopscontrol")
            .attr("disabled", "disabled");
    }
    
};

AdvancedSearchCoordinator.prototype.UpdateItemDatum = function (elementid, callingitem) {
    //console.log("AdvancedSearchCoordinator.prototype.SearchItemClicked started");
    //console.log(this);
    //console.log(elementid);
    var details = d3.select("#" + elementid);
    var datum = d3.select(callingitem).datum();
    //console.log(datum);
    details.datum(datum);
    return datum;
};

AdvancedSearchCoordinator.prototype.onSearchNodeSaveBtnClicked = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);
    var node = d3.select("#searchNodeDetails").datum();
    var nodeEl = d3.select("#searchnode_" + node.Name);

    //console.log(node);

    if (node.Label !== "") {
        nodeEl.classed(node.Label, false);
    }

    node.Name = document.getElementById("nodeIdentifier").value;
    node.Label = document.getElementById("nodeType").value;
    //console.log(node);

    nodeEl
        .attr("id", "searchnode_" + node.Name)
        .classed(node.Label, true);
    nodeEl.select("text")
        .text(node.Name);
};

AdvancedSearchCoordinator.prototype.onSearchNodeDelBtnClicked = function (callingitem) {
    console.log("AdvancedSearchCoordinator.prototype.onSearchNodeDelBtnClicked started: " + callingitem);
    console.log(this);

    var nodedatum = d3.select("#searchNodeDetails").datum();
    console.log(nodedatum);
    this.AdvancedSearch.RemoveNode(nodedatum);

    var viewel = d3.select("#" + this.ElementID);
    viewel.selectAll(".searchnode")
        .data(this.AdvancedSearch.Nodes, function (d) { return d.Name; })
        .exit()
        .remove();
    viewel.selectAll(".searchedge")
        .data(this.AdvancedSearch.Edges, function (d) { return d.Name; })
        .exit()
        .remove();

    this.UpdateNodes();
    this.UpdateEdges();
};

AdvancedSearchCoordinator.prototype.ToggleDir = function() {
    //console.log("AdvancedSearchCoordinator.prototype.ToggleDir");
    var icon = document.getElementById("dirIcon");
    var dir = icon.dataset.dir;
    if (dir === '>') {
        AdvancedSearchCoordinator.prototype.UpdateDirIcon("<", icon);
    }
    else if (dir === '<') {
        AdvancedSearchCoordinator.prototype.UpdateDirIcon(">", icon);
    }
};

AdvancedSearchCoordinator.prototype.UpdateDirIcon = function (dir, iconEl) {
    //console.log("AdvancedSearchCoordinator.prototype.UpdateDirIcon started");
    iconEl.dataset.dir = dir;
    if (dir === '<') {
        iconEl.classList.remove("fa-arrow-right");
        iconEl.classList.add("fa-arrow-left");
    }
    else {
        iconEl.classList.remove("fa-arrow-left");
        iconEl.classList.add("fa-arrow-right");
    }
    //console.log("newdir: " + dir);
};

AdvancedSearchCoordinator.prototype.onSearchEdgeSaveBtnClicked = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);

    
    var edge = d3.select("#searchEdgeDetails").datum();
    var edgeEl = d3.select("#searchedge_" + edge.Name);

    //console.log(edge);

    //if (edge.Label !== "") {
    //    edgeEl.classed(edge.Label, false);
    //}

    edge.Name = document.getElementById("edgeIdentifier").value;
    edge.Label = document.getElementById("edgeType").value;
    var newdir = document.getElementById('dirIcon').dataset.dir;
    if (newdir !== edge.Direction) {
        edge.Direction = document.getElementById('dirIcon').dataset.dir;
    }
    

    var hopsswitch = document.getElementById("hopsSwitch");
    if (hopsswitch.checked) {
        edge.Min = document.getElementById("minSliderVal").value;
        edge.Max = document.getElementById("maxSliderVal").value;
    }
    else {
        edge.Min = -1;
        edge.Max = -1;
    }

    var edgeslot = parseInt(edgeEl.attr("data-slotnum"),10);
    //console.log("slot");
    //console.log(edgeslot);
    edgeEl.remove();
    setTimeout(function () {
        this.UpdateEdges();
    },5);
};



AdvancedSearchCoordinator.prototype.UpdateNodeProps = function () {
    //console.log("updateNodeProps started");
    var type = document.getElementById("nodeType").value;
    var el = document.getElementById("nodeProp");

    clearOptions(el);
    let topoption = addOption(el, "", "");
    topoption.setAttribute("disabled", "");
    topoption.setAttribute("hidden", "");
    topoption.setAttribute("selected", "");

    //console.log(nodeDetails);
    //console.log(type);

    if (type || type === "*") {
        var typedeets = nodeDetails[type];
        if (typedeets) {
            typedeets.forEach(function (prop) {
                addOption(el, prop, prop);
            });
        }
        else {
            addOption(el, "name", "name");
        }
    }
};

AdvancedSearchCoordinator.prototype.UpdateNodeLabels = function () {
    var el = document.getElementById("nodeType");
    clearOptions(el);
    let topoption = addOption(el, "*", "");
    topoption.setAttribute("selected", "");

    apiGetJson("/api/graph/node/labels", function (data) {
        data.forEach(function (label) {
            addOption(el, label, label);
        });
    });
};

AdvancedSearchCoordinator.prototype.BindAutoComplete = function () {
    $("#" + elementPrefix + "Val").autocomplete({
        source: function (request, response) {
            //console.log("autoComplete: "+ request.term);
            var type = document.getElementById(elementPrefix + "Type").value;
            var prop = document.getElementById(elementPrefix + "Prop").value;

            var url = "/api/graph/node/values?type=" + type + "&property=" + prop + "&searchterm=" + request.term;
            apiGetJson(url, response);
        }
    });
};