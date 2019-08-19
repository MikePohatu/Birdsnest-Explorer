import 'foundation-sites';
import * as d3 from 'd3';
import { webcrap } from "../../Shared/webcrap/webcrap";
import { Search, ICondition, ConditionBase, StringCondition, AndOrCondition, SearchNode, SearchEdge, GetCondition } from "./Search";
import ViewTreeNode from "./ViewTreeNode";
import * as log from 'loglevel';

//requires d3js
export default class AdvancedSearchCoordinator {
    SearchData: Search;
    PathElementID: string;
    ConditionElementID: string;
    Radius: number;
    XSpacing: number;
    YSpacing: number;
    ConditionRoot: ViewTreeNode<ICondition>;
    NodeDetails: object;
    EdgeDetails: object;
    Tooltip: FoundationSites.Tooltip;
    AddingTemp: ViewTreeNode<ICondition>;

    constructor(pathelementid: string, conditionelementid: string) {
        //log.debug("AdvancedSearchCoordinator started: " + elementid);
        //log.debug(this);
        var me = this;
        this.SearchData = new Search();
        this.PathElementID = pathelementid;
        this.ConditionElementID = conditionelementid;
        this.Radius = 30;
        this.XSpacing = 170;
        this.YSpacing = 70;
        this.ConditionRoot = null;
        this.NodeDetails = null;
        this.EdgeDetails = null;

        bindEnterToButton("searchEdgeDetails", "searchEdgeSaveBtn");
        bindEnterToButton("searchNodeDetails", "searchNodeSaveBtn");

        //Bind the enter key for an element to click a button
        function bindEnterToButton(elementid, buttonid) {
            document.getElementById(elementid).addEventListener("keydown", function (event) {
                //log.debug("keydown listener fired: " + elementid);
                // Number 13 is the "Enter" key on the keyboard
                if (event.keyCode === 13) {
                    event.preventDefault();
                    document.getElementById(buttonid).click();
                }
            });
        }

        //select("#nodeType").on("change", this.UpdateNodeProps);
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
            me.onSearchConditionAddClicked(this as HTMLElement, true);
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
        d3.select("#searchAndOrDeleteBtn").on("click", function () {
            me.onSearchAndOrDeleteClicked();
        });
        d3.select("#searchTypeSelectSaveBtn").on("click", function () {
            me.onSearchTypeSaveClicked();
        });

        me.UpdateNodeLabels();
        me.UpdateEdgeLabels();
        webcrap.data.apiGetJson("/api/graph/node/properties", function (data) {
            //log.debug(data);
            me.NodeDetails = data;
        });
        webcrap.data.apiGetJson("/api/graph/edge/properties", function (data) {
            //log.debug(data);
            me.EdgeDetails = data;
        });
    }



    RunSearch () {
        //log.debug("RunSearch started");
        //log.debug(this);


        var postdata = JSON.stringify(this.SearchData);
        //log.debug("search post:");
        //log.debug(postdata);
        this.ShowSearchSpinner();
        webcrap.data.apiPostJson("AdvancedSearch", postdata, function (data) {
            //log.debug(data);
            document.getElementById("searchNotification").innerHTML = data;
            $('#searchNotification').foundation();
        });
    }

    ShowSearchSpinner() {
        document.getElementById("searchNotification").innerHTML = "<i class='fas fa-spinner spinner'></i>";
    }

    Clear() {
        //log.debug("Clear started");
        //log.debug(this);

        if (confirm("Are you sure you want to clear this search?") === true) {

            var viewel = d3.select("#" + this.PathElementID);
            viewel.selectAll("*").remove();
            this.SearchData = new Search();
        }

    }

    UpdateNodes() {
        //log.debug("UpdateNodes started");
        //log.debug(this);
        var me = this;
        var currslot = 0;

        var viewel = d3.select("#" + this.PathElementID);
        var newnodeg = viewel.selectAll(".searchnode")
            .data(this.SearchData.Nodes, function (d: SearchNode) { return d.Name; })
            .enter()
            .append("g")
            .attr("id", function (d: SearchNode) { return "searchnode_" + d.Name; })
            .classed("searchnode", true)
            .attr("width", this.Radius)
            .attr("height", this.Radius)
            .attr("data-open", "searchNodeDetails")
            .on("click", function () {
                me.onSearchNodeClicked(this);
            });

        viewel.selectAll(".searchnode")
            .data(this.SearchData.Nodes, function (d: SearchNode) { return d.Name; })
            .attr("transform", function () {
                currslot++;
                return "translate(" + (me.XSpacing * currslot - me.XSpacing * 0.5) + "," + me.YSpacing + ")";
            })
            .exit()
            .remove();

        newnodeg.append("circle")
            .attr("id", function (d: SearchNode) { return "searchnodebg_" + d.Name; })
            .classed("searchnodecircle", true)
            .attr("r", this.Radius);

        newnodeg.append("text")
            .text(function (d: SearchNode) { return d.Name; })
            .attr("text-anchor", "middle")
            .attr("dominant-baseline", "central");
        //.attr("transform", function (d) { return "translate(0," + (this.Radius + 10) + ")"; });
    }

    UpdateEdges () {
        //log.debug("UpdateEdges start");
        //log.debug(this);
        //log.debug(this.AdvancedSearch.Edges);
        var me = this;

        var rectwidth = this.Radius * 2;
        var rectheight = this.Radius * 0.7;

        var relarrowwidth = 20;
        var currslot = 0;
        //log.debug("subspacingx: " + subspacingx);
        //log.debug("subspacingy: " + subspacingy);

        var viewel = d3.select("#" + this.PathElementID);
        var newedgeg = viewel.selectAll(".searchedge")
            .data(this.SearchData.Edges, function (d: SearchEdge) { return d.Name; })
            .enter()
            .append("g")
            .attr("id", function (d: SearchEdge) { return "searchedge_" + d.Name; })
            .attr("class", function (d: SearchEdge) { return d.Label; })
            .classed("searchedge", true)
            .on("click", function () { me.onSearchEdgeClicked(this); })
            .attr("data-open", "searchEdgeDetails");

        newedgeg.append("rect")
            .attr("id", function (d: SearchEdge) { return "searchedgebg_" + d.Name; })
            .classed("searchedgerect", true)
            .attr("width", rectwidth)
            .attr("height", rectheight);

        newedgeg.append("text")
            .text(function (d: SearchEdge) { return d.Name; })
            .attr("text-anchor", "middle")
            .attr("dominant-baseline", "middle")
            .attr("x", rectwidth / 2)
            .attr("y", rectheight / 2);

        newedgeg.append("i")
            .attr("id", function (d: SearchEdge) { return "searchleftarr_" + d.Name; })
            .attr("class", function (d: SearchEdge) {
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
            .attr("id", function (d: SearchEdge) { return "searchrightarr_" + d.Name; })
            .attr("class", function (d: SearchEdge) {
                if (d.Direction === "<") { return "fas fa-minus"; }
                else { return "fas fa-arrow-right"; }
            })
            .classed("searchrightarrow", true)
            .classed("searcharrow", true)
            .attr("x", rectwidth)
            .attr("y", 0)
            .attr("width", relarrowwidth)
            .attr("height", relarrowwidth);

        //log.debug(this.SearchData.Edges);

        viewel.selectAll(".searchedge")
            .data(this.SearchData.Edges, function (d: SearchEdge) { return d.Name; })
            .attr("transform", function () {
                //log.debug(d);
                currslot++;
                var subspacingx = (currslot + 0.5) * me.XSpacing - rectwidth / 2 - me.XSpacing * 0.5;
                var subspacingy = me.YSpacing - rectheight / 2;
                //log.debug(currslot + ": " + subspacingx + ": " + subspacingy);
                //log.debug("edge transforms");
                //log.debug(d);
                return "translate(" + subspacingx + "," + subspacingy + ")";
            })
            .exit()
            .remove();
    }


    onSearchNodeClicked (callingEl) {
        //log.debug("onSearchNodeClicked started");
        //log.debug(this);
        var datum = this.UpdateItemDatum("searchNodeDetails", callingEl) as SearchNode;
        (document.getElementById("nodeType") as HTMLSelectElement).value = datum.Label;
        (document.getElementById("nodeIdentifier") as HTMLInputElement).value = datum.Name;
    }

    onSearchEdgeClicked (callingEl) {
        //log.debug("onSearchEdgeClicked started");
        //log.debug(this);
        var datum = this.UpdateItemDatum("searchEdgeDetails", callingEl) as SearchEdge;
        (document.getElementById("edgeType") as HTMLSelectElement).value = datum.Label;
        (document.getElementById("edgeIdentifier") as HTMLInputElement).value = datum.Name;

        var hopsswitch: HTMLInputElement = document.getElementById("hopsSwitch") as HTMLInputElement;
        var minsliderval: HTMLInputElement = document.getElementById("minSliderVal") as HTMLInputElement;
        var maxsliderval: HTMLInputElement = document.getElementById("maxSliderVal") as HTMLInputElement;

        this.UpdateDirIcon(datum.Direction);

        var min: number = parseInt(datum.Min);
        var max: number = parseInt(datum.Max);
        if (min < 0 || max < 0) {
            hopsswitch.checked = false;
            minsliderval.value = "1";
            maxsliderval.value = "1";
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
    }

    onHopsSwitchChanged () {
        //log.debug("onHopsSwitchChanged started");
        var hopsswitch: HTMLInputElement = document.getElementById("hopsSwitch") as HTMLInputElement;
        //log.debug(hopsswitch.checked);

        if (hopsswitch.checked) {
            d3.selectAll(".hopscontrol")
                .attr("disabled", null);
        }
        else {
            d3.selectAll(".hopscontrol")
                .attr("disabled", "disabled");
        }

    }

    UpdateItemDatum(elementid, callingitem) {
        //log.debug("SearchItemClicked started");
        //log.debug(this);
        //log.debug(elementid);
        var el = d3.select("#" + elementid);
        var datum = d3.select(callingitem).datum();
        //log.debug(datum);
        el.datum(datum);
        return datum;
    }

    GetItemDatum (elementid) {
        //log.debug("GetItemDatum started");
        //log.debug(this);
        //log.debug(elementid);
        var el = d3.select("#" + elementid);
        var datum = el.datum();
        //log.debug(datum);
        return datum;
    }


    GetElementDatum (element) {
        //log.debug("GetElementDatum started");
        //log.debug(this);
        //log.debug(elementid);
        var el = d3.select(element);
        var datum = el.datum();
        //log.debug(datum);
        return datum;
    }


    onSearchNodeSaveBtnClicked () {
        //log.debug("onSearchNodeSaveBtnClicked started");
        //log.debug(this);
        var node: SearchNode = d3.select("#searchNodeDetails").datum() as SearchNode;
        var nodeEl = d3.select("#searchnode_" + node.Name);

        //log.debug(node);

        if (node.Label !== "") {
            nodeEl.classed(node.Label, false);
        }

        node.Name = (document.getElementById("nodeIdentifier") as HTMLInputElement).value;
        node.Label = (document.getElementById("nodeType") as HTMLSelectElement).value;
        //log.debug(node);

        nodeEl
            .attr("id", "searchnode_" + node.Name)
            .classed(node.Label, true);
        nodeEl.select("text")
            .text(node.Name);
    }

    onSearchNodeDelBtnClicked (callingitem) {
        //log.debug("onSearchNodeDelBtnClicked started: " + callingitem);
        //log.debug(this);

        var nodedatum = d3.select("#searchNodeDetails").datum();
        //log.debug(nodedatum);
        this.SearchData.RemoveNode(nodedatum);

        this.UpdateNodes();
        this.UpdateEdges();
    }

    ToggleDir () {
        //log.debug("ToggleDir");
        var iconEl = document.getElementById("dirIcon");
        var dir = iconEl.getAttribute("data-dir");
        if (dir === '>') {
            this.UpdateDirIcon("<");
        }
        else {
            this.UpdateDirIcon(">");
        }
    }

    UpdateDirIcon (dir) {
        //log.debug("UpdateDirIcon started");
        var rightArr = true;
        if (dir === '<') {
            rightArr = false;
        }

        d3.select("#dirIcon")
            .attr("data-dir", dir)
            .classed("fa-arrow-right", rightArr)
            .classed("fa-arrow-left", !rightArr);
        //log.debug("newdir: " + dir);
    }

    onSearchEdgeSaveBtnClicked () {
        //log.debug("onSearchNodeSaveBtnClicked started");
        //log.debug(this);
        var me = this;

        var edge: SearchEdge = d3.select("#searchEdgeDetails").datum() as SearchEdge;
        var edgeEl = d3.select("#searchedge_" + edge.Name);

        //log.debug(edge);
        //log.debug(edgeEl);

        edge.Name = (document.getElementById("edgeIdentifier") as HTMLInputElement).value;
        edge.Label = (document.getElementById("edgeType") as HTMLSelectElement).value;
        edge.Direction = (document.getElementById('dirIcon').getAttribute("data-dir"));

        var hopsswitch: HTMLInputElement = document.getElementById("hopsSwitch") as HTMLInputElement;
        if (hopsswitch.checked) {
            edge.Min = (document.getElementById("minSliderVal") as HTMLInputElement).value;
            edge.Max = (document.getElementById("maxSliderVal") as HTMLInputElement).value;
        }
        else {
            edge.Min = "-1";
            edge.Max = "-1";
        }

        //remove the edge so it can be readded with new details    
        edgeEl.remove();
        setTimeout(function () {
            me.UpdateEdges();
        }, 10);
    }

    UpdateNodeProps () {
        //log.debug("updateNodeProps started");
        var type = (document.getElementById("nodeType") as HTMLSelectElement).value;
        var el = (document.getElementById("nodeProp"));

        webcrap.dom.ClearOptions(el);
        let topoption = webcrap.dom.AddOption(el, "", "");
        topoption.setAttribute("disabled", "");
        topoption.setAttribute("hidden", "");
        topoption.setAttribute("selected", "");

        //log.debug(this.NodeDetails);
        //log.debug(type);

        if (type || type === "*") {
            var typedeets = this.NodeDetails[type];
            if (typedeets) {
                typedeets.forEach(function (prop) {
                    webcrap.dom.AddOption(el, prop, prop);
                });
            }
            else {
                webcrap.dom.AddOption(el, "name", "name");
            }
        }
    }

    UpdateNodeLabels () {
        var el = document.getElementById("nodeType");
        var me = this;
        webcrap.dom.ClearOptions(el);
        let topoption = webcrap.dom.AddOption(el, "*", "");
        topoption.setAttribute("selected", "");

        webcrap.data.apiGetJson("/api/graph/node/labels", function (data) {
            data.forEach(function (label) {
                webcrap.dom.AddOption(el, label, label);
            });
        });
    }

    UpdateEdgeLabels () {
        var el = document.getElementById("edgeType");
        var me = this;
        webcrap.dom.ClearOptions(el);
        let topoption = webcrap.dom.AddOption(el, "*", "");
        topoption.setAttribute("selected", "");

        webcrap.data.apiGetJson("/api/graph/edges/labels", function (data) {
            for (var i = 0; i < data.length; ++i) {
                webcrap.dom.AddOption(el, data[i], data[i]);
            }
        });
    }

    //function addLabelOptions(selectbox, labelList) {
    //    for (var i = 0; i < labelList.length; ++i) {
    //        webcrap.dom.AddOption(selectbox, labelList[i], labelList[i]);
    //    }
    //}

    BindAutoComplete () {
        $("#searchVal").autocomplete({
            source: function (request, response) {
                //log.debug("autoComplete: "+ request.term);
                var type = (document.getElementById("searchType") as HTMLSelectElement).value;
                var prop = (document.getElementById("searchProp") as HTMLSelectElement).value;

                var url = "/api/graph/node/values?type=" + type + "&property=" + prop + "&searchterm=" + request.term;
                webcrap.data.apiGetJson(url, response);
            }
        });
    }

    AddConditionRoot () {
        //log.debug("AddConditionRoot started");
        //d3.select("#whereAddIcon").classed("hidden", true);

        var cond1 = new StringCondition();
        var cond2 = new StringCondition();
        var cond3 = new AndOrCondition();
        var cond4 = new StringCondition();
        var cond5 = new StringCondition();

        var andcond = new AndOrCondition();

        this.SearchData.Condition = andcond;
        this.ConditionRoot = new ViewTreeNode(andcond, "Conditions", null);
        andcond.Conditions.push(cond1);
        andcond.Conditions.push(cond2);
        //andcond.Conditions.push(cond4);
        andcond.Conditions.push(cond3);
        cond3.Conditions.push(cond4);
        cond3.Conditions.push(cond5);
        this.ConditionRoot.Build();

        //this.ConditionRoot.AddChildItem(cond1);
        //this.ConditionRoot.AddChildItem(cond2);

        //this.ConditionD3Root = d3.hierarchy(this.ConditionRoot, function (d: ViewTreeNode) { return d.Children; });
        //log.debug(this.ConditionD3Root);
        this.UpdateConditions();
    }

    NewCondition () {
        return new StringCondition();
    }

    UpdateConditions () {
        log.debug("UpdateConditions start:" + this.ConditionElementID);
        //log.debug(this);
        //log.debug(this.SearchData.Edges);


        var d3root = d3.hierarchy(this.ConditionRoot, function (d: ViewTreeNode<ICondition>) { return d.Children; });
        if (d3root === null) { return; }
        var nodes = d3root.count().descendants();
        
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
        log.debug('nodes:');
        log.debug(nodes);
        var enter = viewel.selectAll(".searchcondition")
            .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
            .enter()
            .append("g")
            .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchcondition_" + (d.data as ViewTreeNode<ICondition>).ID; })
            .attr("class", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                if (d.data.Item instanceof AndOrCondition) {
                    return "conditiongroup";
                }
                else {
                    return "condition";
                }
            })
            .classed("searchcondition", true)
            .attr("data-open", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                if (d.data.Item.Type === "AND" || d.data.Item.Type === "OR") {
                    return "searchAndOrDetails";
                }
                return "searchConditionDetails";
            })
            .on("click", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                if (d.data.Item instanceof AndOrCondition) {
                    me.onSearchAndOrClicked(this);
                }
                else {
                    me.onSearchConditionClicked(this);
                }
            });
        //.each(function (d) {
        //    log.debug("each");
        //    log.debug(d.descendants().length);
        //    log.debug(d);
        //});
        viewel.selectAll(".searchcondition")
            .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
            .exit()
            .remove();

        enter.append("rect")
            .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionbg_" + d.data.ID; })
            .classed("searchconditionrect", true)
            .attr("x", strokewidth)
            .attr("y", strokewidth)
            .attr("rx", 5);

        viewel.selectAll(".searchconditionrect")
            .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
            .attr("height", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                //log.debug(d.ancestors());
                d.data.RectHeight = rectheight - d.depth * ypadding * 2;
                return d.data.RectHeight;
            })
            .attr("width", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                //log.debug("width d: ");
                //log.debug(d); 
                if (d.descendants().length > 1) { 
                    var groupcount: number = d.descendants().length - d.value;
                    var descendentcount: number = d.descendants().length;
                    var leafcount: number = d.descendants().length - groupcount;

                    d.data.RectWidth = leafcount * rectwidth + (d.value - 1) * xspacing + strokewidth * descendentcount * 2 + (editwidth * groupcount) + xpadding * 2;
                }
                else {
                    d.data.RectWidth = rectwidth;
                }
                return d.data.RectWidth;
            });

        var condtext = enter.filter(".searchcondition.condition")
            .append("text")
            .attr("y", "5px")
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
            .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.Item.Type; });

        viewel.selectAll(".condition .searchconditioneval")
            .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return (d.data.Item as ConditionBase).Name + "." + (d.data.Item as ConditionBase).Property; });

        viewel.selectAll(".condition .searchconditionopval")
            .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return (d.data.Item as ConditionBase).Operator + " " + (d.data.Item as ConditionBase).Value; });

        //setup the + button for group nodes e.g. AND/OR
        var groupaddbtns = enter.filter(".conditiongroup").append("g")
            .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionplus_" + d.data.ID; })
            .classed("searchconditionplus", true)
            .classed("searchcontrol", true)
            .on("click", function () {
                me.onSearchConditionAddClicked(this as Element, false);
            });

        //transform translate + button
        viewel.selectAll(".searchconditionplus")
            .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
            .attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                return "translate(" + (d.data.RectWidth - pluswidth - xpadding) + "," + (d.data.RectHeight - ypadding - pluswidth) + ")";
            });

        groupaddbtns.append("i")
            .attr("class", "fas fa-plus")
            .attr("width", pluswidth)
            .attr("height", pluswidth);


        groupaddbtns.append("rect")
            .attr("width", pluswidth)
            .attr("height", pluswidth);

        viewel.selectAll(".searchcondition")
            .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
            .attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                //log.debug(d);
                var x = 0;
                var y = 0;
                if (d.parent !== null) {
                    //log.debug("parent");
                    //log.debug(d.parent);
                    x = d.parent.x + xpadding + (xspacing * d.data.Index) + (d.data.Index * rectwidth);
                    y = ypadding * d.depth;
                }
                d.x = x;
                d.y = y;
                return "translate(" + x + "," + (y + me.YSpacing / 2) + ")";
            });
    }


    //Might come back to this. HTML condition setup rather than SVG
    //UpdateConditionsElement(element: HTMLElement) {
    //    log.debug("UpdateConditionsElement start:" + element);
    //    //log.debug(this);
    //    //log.debug(this.SearchData.Edges);

        
    //    //var links = this.tree.links(nodes);
    //    var me = this;
    //    var tree = this.GetElementDatum(element) as ViewTreeNode<ICondition>;
    //    var viewel = d3.select(element);
    //    viewel.selectAll("*").remove();

    //    var newEl = viewel.append("div")
    //        .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchcondition_" + (d.data as ViewTreeNode<ICondition>).ID; })
    //        .attr("class", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
    //            if (d.data.Item instanceof AndOrCondition) {
    //                return "conditiongroup";
    //            }
    //            else {
    //                return "condition";
    //            }
    //        })
    //        .classed("searchcondition", true)
    //        .attr("data-open", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
    //            if (d.data.Item.Type === "AND" || d.data.Item.Type === "OR") {
    //                return "searchAndOrDetails";
    //            }
    //            return "searchConditionDetails";
    //        })
    //        .on("click", function (d: ViewTreeNode<ICondition>) {
    //            if (d.Item instanceof AndOrCondition) {
    //                me.onSearchAndOrClicked(this);
    //            }
    //            else {
    //                me.onSearchConditionClicked(this);
    //            }
    //        });
    //    //.each(function (d) {
    //    //    log.debug("each");
    //    //    log.debug(d.descendants().length);
    //    //    log.debug(d);
    //    //});
    //    viewel.selectAll(".searchcondition")
    //        .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
    //        .exit()
    //        .remove();

    //    enter.append("rect")
    //        .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionbg_" + d.data.ID; })
    //        .classed("searchconditionrect", true)
    //        .attr("x", strokewidth)
    //        .attr("y", strokewidth)
    //        .attr("rx", 5);

    //    viewel.selectAll(".searchconditionrect")
    //        .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
    //        .attr("height", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
    //            //log.debug(d.ancestors());
    //            d.data.RectHeight = rectheight - d.depth * ypadding * 2;
    //            return d.data.RectHeight;
    //        })
    //        .attr("width", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
    //            if (d.value > 1) {
    //                d.data.RectWidth = d.value * rectwidth + (d.value - 1) * xspacing + (d.descendants().length - 1) * 2 + strokewidth * d.value + editwidth + xpadding * 2;
    //            }
    //            else {
    //                d.data.RectWidth = rectwidth;
    //            }
    //            return d.data.RectWidth;
    //        });

    //    var condtext = enter.filter(".searchcondition.condition")
    //        .append("text")
    //        .attr("y", "5px")
    //        .attr("text-anchor", "left")
    //        .attr("dominant-baseline", "baseline");

    //    condtext.append("tspan")
    //        .attr("x", "10px")
    //        .attr("dy", "1.2em")
    //        .classed("searchconditiontype", true);

    //    condtext.append("tspan")
    //        .attr("x", "10px")
    //        .attr("dy", "1.2em")
    //        .classed("searchconditioneval", true);

    //    condtext.append("tspan")
    //        .attr("x", "10px")
    //        .attr("dy", "1.2em")
    //        .classed("searchconditionopval", true);

    //    viewel.selectAll(".condition .searchconditiontype")
    //        .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.Item.Type; });

    //    viewel.selectAll(".condition .searchconditioneval")
    //        .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return (d.data.Item as ConditionBase).Name + "." + (d.data.Item as ConditionBase).Property; });

    //    viewel.selectAll(".condition .searchconditionopval")
    //        .text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return (d.data.Item as ConditionBase).Operator + " " + (d.data.Item as ConditionBase).Value; });

    //    //setup the + button for group nodes e.g. AND/OR
    //    var groupaddbtns = viewel.selectAll(".conditiongroup")
    //        .append("g")
    //        .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionplus_" + d.data.ID; })
    //        .classed("searchconditionplus", true)
    //        .classed("searchcontrol", true)
    //        .on("click", function () {
    //            me.onSearchConditionAddClicked(this, false);
    //        });

    //    groupaddbtns.append("i")
    //        .attr("class", "fas fa-plus")
    //        .attr("width", pluswidth)
    //        .attr("height", pluswidth)
    //        .attr("x", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.RectWidth - pluswidth - xpadding; })
    //        .attr("y", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.RectHeight - ypadding - pluswidth; });


    //    groupaddbtns.append("rect")
    //        .attr("width", pluswidth)
    //        .attr("height", pluswidth)
    //        .attr("x", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.RectWidth - pluswidth - xpadding; })
    //        .attr("y", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.RectHeight - ypadding - pluswidth; });

    //    viewel.selectAll(".searchcondition")
    //        .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
    //        .attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
    //            //log.debug(d);
    //            var x = 0;
    //            var y = 0;
    //            if (d.parent !== null) {
    //                //log.debug("parent");
    //                //log.debug(d.parent);
    //                x = d.parent.x + xpadding + (xspacing * d.data.Index) + (d.data.Index * rectwidth);
    //                y = ypadding * d.depth;
    //            }
    //            d.x = x;
    //            d.y = y;
    //            return "translate(" + x + "," + (y + me.YSpacing / 2) + ")";
    //        });
    //}

    onSearchConditionClicked (callingelement) {
        log.debug("onSearchConditionClicked started");
        //var datum;
        //log.debug(callingelement);
        var me = this;
        var condition: ConditionBase;
        this.ClearAlert();
        var datum = this.UpdateItemDatum("searchConditionDetails", callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        if (datum) { condition = datum.data.Item as ConditionBase; }
        log.debug(datum);

        (document.getElementById("searchProp") as HTMLSelectElement).disabled = false;
        (document.getElementById("searchVal") as HTMLInputElement).disabled = false;
        (document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = false;

        this.UpdateConditionDetails(function () {
            me.ChangeSelectedValue(document.getElementById("searchItem"), condition.Name, function () {
                me.ChangeSelectedValue(document.getElementById("searchProp"), condition.Property, null);
            });
        });

        (document.getElementById("searchVal") as HTMLInputElement).value = condition.Value;
    }

    UpdateConditionDetails (callback) {
        log.debug("UpdateConditionDetails started");
        //log.debug(this.SearchData.Nodes);
        var me = this;
        var datum = this.GetItemDatum("searchConditionDetails");

        var searchItem = document.getElementById("searchItem");
        //var searchProps = document.getElementById("searchProp");
        webcrap.dom.ClearOptions(searchItem);
        //webcrap.dom.ClearOptions(searchProps);

        //set empty top option
        var option = webcrap.dom.AddOption(searchItem, "", "");
        option.setAttribute("disabled", "");
        option.setAttribute("hidden", "");
        option.setAttribute("selected", "");

        option = webcrap.dom.AddOption(searchItem, "Nodes", "Nodes");
        option.setAttribute("disabled", "");
        this.SearchData.Nodes.forEach(function (item) {
            webcrap.dom.AddOption(searchItem, item.Name, item.Name);
        });

        option = webcrap.dom.AddOption(searchItem, "Relationships", "Relationships");
        option.setAttribute("disabled", "");
        this.SearchData.Edges.forEach(function (item) {
            webcrap.dom.AddOption(searchItem, item.Name, item.Name);
        });

        if (typeof callback === "function") {
            callback();
            //setTimeout(callback, 5);
        }
    }

    onSearchConditionItemChanged () {
        //log.debug("onSearchConditionItemChanged started");
        //log.debug(this);

        var me = this;
        var searchItem: HTMLSelectElement = document.getElementById("searchItem") as HTMLSelectElement;
        var searchProps = document.getElementById("searchProp");
        var selectedName = searchItem.options[searchItem.selectedIndex].value;
        var selectedItem;
        var datum = (this.GetItemDatum("searchConditionDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
        //log.debug(datum);
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

        //log.debug(this.NodeDetails);
        //log.debug(this.EdgeDetails);
        //log.debug(selectedItem);

        webcrap.dom.ClearOptions(searchProps);
        var props;
        if (selectedItem) {
            if (selectedItem.Label === "" || selectedItem.Label === "*") {
                log.debug("node label not selected");
                (document.getElementById("searchProp") as HTMLSelectElement).disabled = true;
                (document.getElementById("searchVal") as HTMLInputElement).disabled = true;
                (document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = true;
                this.SetAlert("The item you have selected (" + selectedItem.Name + ") does not have a type. You must set the type on the item before you can create a condition");
            }
            else {
                this.ClearAlert();
                (document.getElementById("searchProp") as HTMLSelectElement).disabled = false;
                (document.getElementById("searchVal") as HTMLInputElement).disabled = false;
                (document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = false;
                if (typeSelected === "node") {
                    props = this.NodeDetails[selectedItem.Label];
                }
                else if (typeSelected === "edge") {
                    props = this.EdgeDetails[selectedItem.Label];
                }

                if (selectedItem.Label) {
                    if (props) {
                        props.forEach(function (item) {
                            webcrap.dom.AddOption(searchProps, item, item);
                        });
                    }
                }
            }
        }


        //log.debug(datum);
        //var label = datum.
    }

    onSearchConditionAddClicked(callingelement: Element, isroot?: boolean) {
        log.trace("onSearchConditionAddClicked started: " + isroot);
        //var datum;
        var tempnode: ViewTreeNode<ICondition>; 
        if (isroot === true) {
            tempnode = new ViewTreeNode(null, 'Conditions', null);
        }
        else {
            var addingparent = (this.GetElementDatum(callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
            log.debug(addingparent);
            tempnode = new ViewTreeNode(null, 'Conditions', addingparent);
        }
        log.debug('tempnode:');
        log.debug(tempnode);
        this.AddingTemp = tempnode;
        $('#searchConditionTypeSelect').foundation('open');
    }

    onSearchTypeSaveClicked() {
        log.trace("onSearchTypeSaveClicked started");
        var selectedType: string = (document.getElementById("searchConditionTypeList") as HTMLSelectElement).value;
        var cond: ICondition = GetCondition(selectedType);
        this.AddingTemp.Item = cond;

        if (this.AddingTemp.Parent !== null) {
            this.AddingTemp.Parent.AddChild(this.AddingTemp);
            this.ConditionRoot.Rebuild();
        }
        else {
            this.ConditionRoot = this.AddingTemp;
            this.ConditionRoot.Build();
        }
        $('#searchConditionTypeSelect').foundation('close');
        
        this.UpdateConditions();
    }

    onSearchConditionDeleteClicked() {
        this.DeleteSearchCondition('searchConditionDetails');
    }

    onSearchAndOrDeleteClicked() {
        this.DeleteSearchCondition('searchAndOrDetails');
    }

    DeleteSearchCondition (elementid: string) {
        log.trace("deleteSearchCondition started: " + elementid);
        //searchConditionDetails

        //datum is the d3 tree datum. Use datum.data to get the ViewTreeNode datum
        var datum = this.GetItemDatum(elementid) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        var treenode = datum.data;

        log.debug(datum);
        log.debug("RootID: " + this.ConditionRoot.ID);
        if (this.ConditionRoot.ID === treenode.ID) {
            this.ConditionRoot = null;
            d3.selectAll(".searchcondition").remove();
        }
        else {
            datum.parent.data.RemoveChild(treenode);
            datum.parent.data.Rebuild();
            this.UpdateConditions();
        }

        log.debug(this.ConditionRoot);
        log.trace("onSearchConditionDeleteClicked finished");
    }


    onSearchConditionSaveClicked () {
        //log.debug("onSearchConditionSaveClicked started");
        var datum = this.GetItemDatum("searchConditionDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        var condition = datum.data.Item as ConditionBase;

        var newname = (document.getElementById("searchItem") as HTMLSelectElement).value;
        var newprop = (document.getElementById("searchProp") as HTMLSelectElement).value;
        var newval = (document.getElementById("searchVal") as HTMLInputElement).value;

        if (webcrap.misc.isNullOrWhitespace(newname) || webcrap.misc.isNullOrWhitespace(newprop) || webcrap.misc.isNullOrWhitespace(newval)) {
            this.SetAlert("Name, property, or value is empty. Please set a value");
        }
        else {
            condition.Name = newname;
            condition.Property = newprop;
            condition.Value = newval;
            $('#searchConditionDetails').foundation('close');
            this.UpdateConditions();
        }

    }

    ChangeSelectedValue (selectEl, value, callback) {
        //log.debug("ChangeSelectedValue started");
        //log.debug(selectEl);
        //log.debug(value);
        var i;
        for (i = 0; i < selectEl.options.length; i++) {
            //log.debug(selectEl[i].value);
            if (selectEl[i].value === value) {
                selectEl.selectedIndex = i;
                //log.debug("found " + value + " at index: " + i);
                d3.select(selectEl).dispatch('change');
                break;
            }
        }

        if (typeof callback === "function") { callback(); }
    }

    onSearchAndOrSaveClicked () {
        //log.debug("onSearchAndOrSaveClicked started");
        var datum = this.GetItemDatum("searchAndOrDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>;

        datum.data.Item.Type = (document.getElementById("searchAndOr") as HTMLSelectElement).value;
    }

    onSearchAndOrClicked (callingelement) {
        log.debug("onSearchAndOrClicked started");
        this.ClearAlert();
        var datum = this.UpdateItemDatum("searchAndOrDetails", callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        log.debug(datum);
        var cond: AndOrCondition = datum.data.Item as AndOrCondition;

        (document.getElementById("searchAndOr") as HTMLInputElement).value = cond.Type;
    };

    ClearAlert () {
        var alertEl = $("#alertIcon");
        alertEl.hide();
    }

    SetAlert (message: string) {
        log.debug("SetAlert started: " + message);
        log.debug(this.Tooltip);
        var alertEl = $("#alertIcon");
        if (this.Tooltip) {
            this.Tooltip.destroy();
            this.Tooltip = null;
        }
        this.Tooltip = new Foundation.Tooltip(alertEl, <FoundationSites.ITooltipOptions>{
            allowHtml: true,
            tipText: message
        });
        alertEl.show();
    }
}
