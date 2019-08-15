//requires d3js
function AdvancedSearchCoordinator(pathelementid, conditionelementid) {
    //console.log("AdvancedSearchCoordinator started: " + elementid);
    //console.log(this);
    var me = this;
    this.SearchData = new Search();
    this.PathElementID = pathelementid;
    this.ConditionElementID = conditionelementid;
    this.Radius = 30;
    this.XSpacing = 170;
    this.YSpacing = 70;
    this.ConditionRoot = null;
    this.ConditionD3Root = null;
    this.NodeDetails = null;
    this.EdgeDetails = null;

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
        me.RunSearch();
    });
    d3.select("#pathAddIcon").on("click", function () {
        me.SearchData.AddNode();
        me.UpdateNodes();
        me.UpdateEdges();
    });
    d3.select("#whereAddIcon").on("click", function () {
        me.AddConditionRoot();
    });
    d3.select("#advSearchClearIcon").on("click", function () {
        me.Clear();
    });
    d3.select("#hopsSwitch").on("change", function () {
        me.onHopsSwitchChanged();
    });
    d3.select("#searchConditionDeleteBtn").on("click", function () {
        me.onSearchConditionDeleteClicked();
    });
    d3.select("#searchItem").on("change", function () {
        me.onSearchConditionItemChanged();
    });
    d3.select("#searchConditionSaveBtn").on("click", function () {
        me.onSearchConditionSaveClicked();
    });
    d3.select("#searchAndOrSaveBtn").on("click", function () {
        me.onSearchAndOrSaveClicked();
    });


    me.UpdateNodeLabels();
    me.UpdateEdgeLabels();
    apiGetJson("/api/graph/node/properties", function (data) {
        //console.log(data);
        me.NodeDetails = data;
    });
    apiGetJson("/api/graph/edge/properties", function (data) {
        //console.log(data);
        me.EdgeDetails = data;
    });
}

AdvancedSearchCoordinator.prototype.RunSearch = function () {
    //console.log("AdvancedSearchCoordinator.prototype.RunSearch started");
    //console.log(this);
    

    var postdata = JSON.stringify(this.SearchData);
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

    if (confirm("Are you sure you want to clear this search?") === true) {

        var viewel = d3.select("#" + this.PathElementID);
        viewel.selectAll("*").remove();
        this.SearchData = new Search();
    }
    
};

AdvancedSearchCoordinator.prototype.UpdateNodes = function () {
    //console.log("AdvancedSearchCoordinator.prototype.UpdateNodes started");
    //console.log(this);
    var me = this;
    var currslot = 0;

    var viewel = d3.select("#" + this.PathElementID);
    var newnodeg = viewel.selectAll(".searchnode")
        .data(this.SearchData.Nodes, function (d) { return d.Name; })
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
        .data(this.SearchData.Nodes, function (d) { return d.Name; })
        .attr("transform", function () {
            currslot++;
            return "translate(" + (me.XSpacing * currslot - me.XSpacing * 0.5) + "," + me.YSpacing + ")";
        })
        .exit()
        .remove();

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
    //console.log("AdvancedSearchCoordinator.prototype.UpdateEdges start");
    //console.log(this);
    //console.log(this.AdvancedSearch.Edges);
    var me = this;

    var rectwidth = this.Radius*2;
    var rectheight = this.Radius * 0.7;
    
    var relarrowwidth = 20;
    var currslot = 0;
    //console.log("subspacingx: " + subspacingx);
    //console.log("subspacingy: " + subspacingy);

    var viewel = d3.select("#" + this.PathElementID);
    var newedgeg = viewel.selectAll(".searchedge")
        .data(this.SearchData.Edges, function (d) { return d.Name; })
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchedge_" + d.Name; })
        .attr("class", function (d) { return d.Label; })
        .classed("searchedge", true)
        .on("click", function () { me.onSearchEdgeClicked(this); })
        .attr("data-open", "searchEdgeDetails");

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

    //console.log(this.SearchData.Edges);

    viewel.selectAll(".searchedge")
        .data(this.SearchData.Edges, function (d) { return d.Name; })
        .attr("transform", function (d) {
            //console.log(d);
            currslot++;
            var subspacingx = (currslot + 0.5) * me.XSpacing - rectwidth / 2 - me.XSpacing * 0.5;
            var subspacingy = me.YSpacing - rectheight / 2;
            //console.log(currslot + ": " + subspacingx + ": " + subspacingy);
            //console.log("edge transforms");
            //console.log(d);
            return "translate(" + subspacingx + "," + subspacingy + ")";
        })
        .exit()
        .remove();
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

    this.UpdateDirIcon(datum.Direction);

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

    this.onHopsSwitchChanged();
    //Foundation.reInit($('#hopsSlider'));
    d3.select('#minSliderVal').dispatch('change');
    d3.select('#maxSliderVal').dispatch('change');
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
    var el = d3.select("#" + elementid);
    var datum = d3.select(callingitem).datum();
    //console.log(datum);
    el.datum(datum);
    return datum;
};

AdvancedSearchCoordinator.prototype.GetItemDatum = function (elementid) {
    //console.log("AdvancedSearchCoordinator.prototype.GetItemDatum started");
    //console.log(this);
    //console.log(elementid);
    var el = d3.select("#" + elementid);
    var datum = el.datum();
    //console.log(datum);
    return datum;
};


AdvancedSearchCoordinator.prototype.GetElementDatum = function (element) {
    //console.log("AdvancedSearchCoordinator.prototype.GetElementDatum started");
    //console.log(this);
    //console.log(elementid);
    var el = d3.select(element);
    var datum = el.datum();
    //console.log(datum);
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
    //console.log("AdvancedSearchCoordinator.prototype.onSearchNodeDelBtnClicked started: " + callingitem);
    //console.log(this);

    var nodedatum = d3.select("#searchNodeDetails").datum();
    //console.log(nodedatum);
    this.SearchData.RemoveNode(nodedatum);

    this.UpdateNodes();
    this.UpdateEdges();
};

AdvancedSearchCoordinator.prototype.ToggleDir = function() {
    //console.log("AdvancedSearchCoordinator.prototype.ToggleDir");
    var iconEl = document.getElementById("dirIcon");
    var dir = iconEl.getAttribute("data-dir");
    if (dir === '>') {
        this.UpdateDirIcon("<");
    }
    else {
        this.UpdateDirIcon(">");
    }
};

AdvancedSearchCoordinator.prototype.UpdateDirIcon = function (dir) {
    //console.log("AdvancedSearchCoordinator.prototype.UpdateDirIcon started");
    var rightArr = true;
    if (dir === '<') {
        rightArr = false;
    }

    d3.select("#dirIcon")
        .attr("data-dir", dir)
        .classed("fa-arrow-right", rightArr)
        .classed("fa-arrow-left", !rightArr);
    //console.log("newdir: " + dir);
};

AdvancedSearchCoordinator.prototype.onSearchEdgeSaveBtnClicked = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchNodeSaveBtnClicked started");
    //console.log(this);
    var me = this;
    
    var edge = d3.select("#searchEdgeDetails").datum();
    var edgeEl = d3.select("#searchedge_" + edge.Name);

    //console.log(edge);
    //console.log(edgeEl);

    edge.Name = document.getElementById("edgeIdentifier").value;
    edge.Label = document.getElementById("edgeType").value;
    edge.Direction = document.getElementById('dirIcon').getAttribute("data-dir");    

    var hopsswitch = document.getElementById("hopsSwitch");
    if (hopsswitch.checked) {
        edge.Min = document.getElementById("minSliderVal").value;
        edge.Max = document.getElementById("maxSliderVal").value;
    }
    else {
        edge.Min = -1;
        edge.Max = -1;
    }

    //remove the edge so it can be readded with new details    
    edgeEl.remove();
    setTimeout(function () {
        me.UpdateEdges();
    },10);
};



AdvancedSearchCoordinator.prototype.UpdateNodeProps = function () {
    //console.log("updateNodeProps started");
    var type = document.getElementById("nodeType").value;
    var el = document.getElementById("nodeProp");

    domcrap.ClearOptions(el);
    let topoption = domcrap.AddOption(el, "", "");
    topoption.setAttribute("disabled", "");
    topoption.setAttribute("hidden", "");
    topoption.setAttribute("selected", "");

    //console.log(this.NodeDetails);
    //console.log(type);

    if (type || type === "*") {
        var typedeets = this.NodeDetails[type];
        if (typedeets) {
            typedeets.forEach(function (prop) {
                domcrap.AddOption(el, prop, prop);
            });
        }
        else {
            domcrap.AddOption(el, "name", "name");
        }
    }
};

AdvancedSearchCoordinator.prototype.UpdateNodeLabels = function () {
    var el = document.getElementById("nodeType");
    var me = this;
    domcrap.ClearOptions(el);
    let topoption = domcrap.AddOption(el, "*", "");
    topoption.setAttribute("selected", "");

    apiGetJson("/api/graph/node/labels", function (data) {
        data.forEach(function (label) {
            domcrap.AddOption(el, label, label);
        });
    });
};

AdvancedSearchCoordinator.prototype.UpdateEdgeLabels = function () {
    var el = document.getElementById("edgeType");
    var me = this;
    domcrap.ClearOptions(el);
    let topoption = domcrap.AddOption(el, "*", "");
    topoption.setAttribute("selected", "");

    apiGetJson("/api/graph/edges/labels", function (data) {
        for (var i = 0; i < data.length; ++i) {
            domcrap.AddOption(el, data[i], data[i]);
        }
    });
};

//function addLabelOptions(selectbox, labelList) {
//    for (var i = 0; i < labelList.length; ++i) {
//        domcrap.AddOption(selectbox, labelList[i], labelList[i]);
//    }
//}

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



AdvancedSearchCoordinator.prototype.AddConditionRoot = function () {
    //console.log("AdvancedSearchCoordinator.prototype.AddConditionRoot started");
    //d3.select("#whereAddIcon").classed("hidden", true);

    var cond1 = new StringCondition();
    var cond2 = new StringCondition();
    var cond3 = new AndCondition();
    var cond4 = new StringCondition();
    var cond5 = new StringCondition();

    var andcond = new AndCondition();

    this.SearchData.Condition = andcond;
    this.ConditionRoot = new ViewTreeNode(andcond, "Conditions");
    andcond.Conditions.push(cond1);
    andcond.Conditions.push(cond2);
    //andcond.Conditions.push(cond4);
    andcond.Conditions.push(cond3);
    cond3.Conditions.push(cond4);
    cond3.Conditions.push(cond5);
    this.ConditionRoot.Build();

    //this.ConditionRoot.AddChildItem(cond1);
    //this.ConditionRoot.AddChildItem(cond2);

    this.ConditionD3Root = d3.hierarchy(this.ConditionRoot, function (d) { return d.Children; });
    //console.log(this.ConditionD3Root);
    this.UpdateConditions();
};

AdvancedSearchCoordinator.prototype.onAddCondition = function () {

};

AdvancedSearchCoordinator.prototype.NewCondition = function () {
    return new StringCondition();
};

AdvancedSearchCoordinator.prototype.UpdateConditions = function () {
    //console.log("AdvancedSearchCoordinator.prototype.UpdateConditions start:" + this.ConditionElementID);
    //console.log(this);
    //console.log(this.SearchData.Edges);
    if (this.ConditionD3Root === null) { return; }

    var nodes = this.ConditionD3Root.descendants();
    this.ConditionD3Root.count();
    //var links = this.tree.links(nodes);
    var me = this;

    var rectwidth = 130;
    var rectheight = 100;
    var xpadding = 5;
    var xspacing = 70;
    var ypadding = 5;
    var strokewidth = 3;
    var pluswidth = 25;
    var editwidth = 25;

    var viewel = d3.select("#" + this.ConditionElementID);
    //console.log(nodes);
    var enter = viewel.selectAll(".searchcondition")
        .data(nodes, function (d) { return d.data.ID; })
        .enter()
        .append("g")
        .attr("id", function (d) { return "searchcondition_" + d.data.ID; })
        .attr("class", function (d) {
            if (d.data.Item.Conditions) {
                return "conditiongroup";
            }
            else {
                return "condition";
            }
        })
        .classed("searchcondition", true)
        .attr("data-open", function (d) {
            if (d.data.Item.Type === "AND" || d.data.Item.Type === "OR") {
                return "searchAndOrDetails";
            }
            return "searchConditionDetails";
        })
        .on("click", function (d) {
            if (d.data.Item.Type === "AND" || d.data.Item.Type === "OR") {
                me.onSearchAndOrClicked(this);
            }
            else {
                me.onSearchConditionClicked(this);
            }
        });
        //.each(function (d) {
        //    console.log("each");
        //    console.log(d.descendants().length);
        //    console.log(d);
        //});

    enter.append("rect")
        .attr("id", function (d) { return "searchconditionbg_" + d.data.ID; })
        .classed("searchconditionrect", true)
        .attr("height", function (d) {
            //console.log(d.ancestors());
            d.rectheight = rectheight - d.depth * ypadding * 2;
            return d.rectheight;
        })
        .attr("width", function (d) {
            if (d.value > 1) {
                d.rectwidth = d.value * rectwidth + (d.value - 1) * xspacing + (d.descendants().length - 1) * 2 + strokewidth * d.value + editwidth + xpadding *2;
            }
            else {
                d.rectwidth = rectwidth;
            }
            return d.rectwidth;
        })
        .attr("x", strokewidth)
        .attr("y", strokewidth)
        .attr("rx", 5);

    //BUG HERE
    //this will append even to items that already have a text node.  
    //select is the same level as above so doesn't really work
    var condtext = viewel.selectAll(".searchcondition.condition")
        .data(nodes, function (d) { return d.data.ID; })
        .append("text")
        .attr("y","5px")
        .attr("text-anchor", "left")
        .attr("dominant-baseline", "baseline");

    condtext.append("tspan")
        .attr("x", "10px")
        .attr("dy", "1.2em")
        .classed("searchconditiontype", true);

    condtext.append("tspan")
        .attr("x", "10px")
        .attr("dy", "1.2em")
        .classed("searchconditioneval", true);

    condtext.append("tspan")
        .attr("x", "10px")
        .attr("dy", "1.2em")
        .classed("searchconditionopval", true);

    viewel.selectAll(".condition .searchconditiontype")
        .text(function (d) { return d.data.Item.Type; });

    viewel.selectAll(".condition .searchconditioneval")
        .text(function (d) { return d.data.Item.Name + "." + d.data.Item.Property; });

    viewel.selectAll(".condition .searchconditionopval")
        .text(function (d) { return d.data.Item.Operator + " " + d.data.Item.Value; });

    //setup the + button for group nodes e.g. AND/OR
    var groupaddbtns = viewel.selectAll(".conditiongroup")
        .append("g")
        .attr("id", function (d) { return "searchconditionplus_" + d.data.ID; })
        .classed("searchconditionplus", true)
        .classed("searchcontrol", true)
        .on("click", function () {
            me.onSearchConditionAddClicked(this);
        });

    groupaddbtns.append("i")
        .attr("class", "fas fa-plus")
        .attr("width", pluswidth)
        .attr("height", pluswidth)
        .attr("x", function (d) { return d.rectwidth - pluswidth - xpadding; })
        .attr("y", function (d) { return d.rectheight - ypadding - pluswidth; });


    groupaddbtns.append("rect")
        .attr("width", pluswidth)
        .attr("height", pluswidth)
        .attr("x", function (d) { return d.rectwidth - pluswidth - xpadding; })
        .attr("y", function (d) { return d.rectheight - ypadding - pluswidth; });

    viewel.selectAll(".searchcondition")
        .data(nodes, function (d) { return d.data.ID; })
        .attr("transform", function (d) {
            //console.log(d);
            var x = 0;
            var y = 0;
            if (d.parent !== null) {
                //console.log("parent");
                //console.log(d.parent);
                x = d.parent.x + xpadding + (xspacing * d.data.Index) + (d.data.Index * rectwidth);
                y = ypadding * d.depth;
            }
            d.x = x;
            d.y = y;
            return "translate(" + x + "," + (y + me.YSpacing / 2 ) + ")";
        })
        .exit()
        .remove();
};

AdvancedSearchCoordinator.prototype.onSearchConditionClicked = function (callingelement) {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchConditionClicked started");
    //var datum;
    //d3.select(callingelement)
    //    .each(function (d) { datum = d.data; });
    //console.log(callingelement);
    var me = this;
    this.ClearAlert();
    var datum = this.UpdateItemDatum("searchConditionDetails", callingelement);
    if (datum) { datum = datum.data; }
    //console.log(datum);

    document.getElementById("searchProp").disabled = false;
    document.getElementById("searchVal").disabled = false;
    document.getElementById("searchConditionSaveBtn").disabled = false;

    this.UpdateConditionDetails(function () {
        me.ChangeSelectedValue(document.getElementById("searchItem"), datum.Item.Name, function () {
            me.ChangeSelectedValue(document.getElementById("searchProp"), datum.Item.Property);
        });
    });

    document.getElementById("searchVal").value = datum.Item.Value;
};

AdvancedSearchCoordinator.prototype.UpdateConditionDetails = function (callback) {
    console.log("AdvancedSearchCoordinator.prototype.UpdateConditionDetails started");
    //console.log(this.SearchData.Nodes);
    var me = this;
    var datum = this.GetItemDatum("searchConditionDetails");

    var searchItem = document.getElementById("searchItem");
    //var searchProps = document.getElementById("searchProp");
    domcrap.ClearOptions(searchItem);
    //domcrap.ClearOptions(searchProps);

    //set empty top option
    var option = domcrap.AddOption(searchItem, "", "");
    option.setAttribute("disabled", "");
    option.setAttribute("hidden", "");
    option.setAttribute("selected", "");

    option = domcrap.AddOption(searchItem, "Nodes", "Nodes");
    option.setAttribute("disabled", "");
    this.SearchData.Nodes.forEach(function (item) {
        domcrap.AddOption(searchItem, item.Name, item.Name);
    });

    option = domcrap.AddOption(searchItem, "Relationships", "Relationships");
    option.setAttribute("disabled", "");
    this.SearchData.Edges.forEach(function (item) {
        domcrap.AddOption(searchItem, item.Name, item.Name);
    });

    if (typeof callback === "function") {
        callback();
        //setTimeout(callback, 5);
    }
};

AdvancedSearchCoordinator.prototype.onSearchConditionItemChanged = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchConditionItemChanged started");
    //console.log(this);

    var me = this;
    var searchItem = document.getElementById("searchItem");
    var searchProps = document.getElementById("searchProp");
    var selectedName = searchItem.options[searchItem.selectedIndex].value;
    var selectedItem;
    var datum = this.GetItemDatum("searchConditionDetails").data;
    //console.log(datum);
    var typeSelected = "";

    var i;
    for (i = 0; i < this.SearchData.Nodes.length; i++) {
        if (this.SearchData.Nodes[i].Name === selectedName) {
            selectedItem = this.SearchData.Nodes[i];
            typeSelected = "node";
            break;
        }
    }

    if (typeSelected !== "node") {
        for (i = 0; i < this.SearchData.Edges.length; i++) {
            if (this.SearchData.Edges[i].Name === selectedName) {
                selectedItem = this.SearchData.Edges[i];
                typeSelected = "edge";
                break;
            }
        }
    }

    //console.log(this.NodeDetails);
    //console.log(this.EdgeDetails);
    console.log(selectedItem);

    domcrap.ClearOptions(searchProps);
    var props;
    if (selectedItem) {
        if (selectedItem.Label === "" || selectedItem.Label === "*") {
            console.log("node label not selected");
            document.getElementById("searchProp").disabled = true;
            document.getElementById("searchVal").disabled = true;
            document.getElementById("searchConditionSaveBtn").disabled = true;
            this.SetAlert("The item you have selected (" + selectedItem.Name + ") does not have a type. You must set the type on the item before you can create a condition");
        }
        else {
            this.ClearAlert();
            document.getElementById("searchProp").disabled = false;
            document.getElementById("searchVal").disabled = false;
            document.getElementById("searchConditionSaveBtn").disabled = false;
            if (typeSelected === "node") {
                props = this.NodeDetails[selectedItem.Label];
            }
            else if (typeSelected === "edge") {
                props = this.EdgeDetails[selectedItem.Label];
            }

            if (selectedItem.Label) {
                if (props) {
                    props.forEach(function (item) {
                        option = domcrap.AddOption(searchProps, item, item);
                    });
                }
            }
        }
    }
    

    console.log(datum);
    //var label = datum.
};

AdvancedSearchCoordinator.prototype.onSearchConditionAddClicked = function (callingelement) {
    console.log("AdvancedSearchCoordinator.prototype.onSearchConditionAddClicked started");
    //var datum;
    
};

AdvancedSearchCoordinator.prototype.onSearchConditionDeleteClicked = function () {
    console.log("AdvancedSearchCoordinator.prototype.onSearchConditionDeleteClicked started");
    //searchConditionDetails

    //datum is the d3 tree datum. Use datum.data to get the ViewTreeNode datum
    var datum = this.GetItemDatum("searchConditionDetails");
    console.log(datum);
    if (this.ConditionRoot.ID === datum.data.ID) {
        this.ConditionRoot = null;
        this.ConditionD3Root = null;
        d3.selectAll(".searchcondition").remove();
    }
    else {
        datum.parent.data.RemoveChild(datum.data);
        datum.parent.data.Rebuild();
        this.UpdateConditions();
    }

    console.log(this.ConditionRoot);
};


AdvancedSearchCoordinator.prototype.onSearchConditionSaveClicked = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchConditionSaveClicked started");
    var datum = this.GetItemDatum("searchConditionDetails").data;

    var newname = document.getElementById("searchItem").value;
    var newprop = document.getElementById("searchProp").value;
    var newval = document.getElementById("searchVal").value;

    if (crap.isNullOrWhitespace(newname) || crap.isNullOrWhitespace(newprop) || crap.isNullOrWhitespace(newval)) {
        this.SetAlert("Name, property, or value is empty. Please set a value");
    }
    else {
        datum.Item.Name = newname;
        datum.Item.Property = newprop;
        datum.Item.Value = newval;
        $('#searchConditionDetails').foundation('close');
        this.UpdateConditions();
    }
    
};

AdvancedSearchCoordinator.prototype.ChangeSelectedValue = function (selectEl, value, callback) {
    //console.log("AdvancedSearchCoordinator.prototype.ChangeSelectedValue started");
    //console.log(selectEl);
    //console.log(value);
    var i;
    for (i = 0; i < selectEl.options.length; i++) {
        //console.log(selectEl[i].value);
        if (selectEl[i].value === value) {
            selectEl.selectedIndex = i;
            //console.log("found " + value + " at index: " + i);
            d3.select(selectEl).dispatch('change');
            break;
        }
    }

    if (typeof callback === "function") { callback(); }
};

AdvancedSearchCoordinator.prototype.onSearchAndOrSaveClicked = function () {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchAndOrSaveClicked started");
    var datum = this.GetItemDatum("searchAndOrDetails").data;

    datum.Item.Type = document.getElementById("searchAndOr").value;
};

AdvancedSearchCoordinator.prototype.onSearchAndOrClicked = function (callingelement) {
    //console.log("AdvancedSearchCoordinator.prototype.onSearchAndOrClicked started");
    var me = this;
    this.ClearAlert();
    var datum = this.UpdateItemDatum("searchAndOrDetails", callingelement);
    if (datum) { datum = datum.data; }
    //console.log(datum);

    document.getElementById("searchAndOr").value = datum.Item.Type;
};

AdvancedSearchCoordinator.prototype.ClearAlert = function () {
    var alertEl = $("#alertIcon");
    alertEl.hide();
};

AdvancedSearchCoordinator.prototype.SetAlert = function (message) {
    var alertEl = $("#alertIcon");
    new Foundation.Tooltip(alertEl, {
        allowHtml: true,
        tipText: message
    });
    alertEl.show();
};
