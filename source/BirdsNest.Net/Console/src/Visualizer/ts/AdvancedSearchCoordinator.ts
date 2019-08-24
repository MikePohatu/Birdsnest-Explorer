import 'jquery-ui/themes/base/core.css';

import 'jquery-ui/themes/base/selectmenu.css';
import 'jquery-ui/themes/base/menu.css';
import 'jquery-ui/themes/base/autocomplete.css';
import 'jquery-ui/themes/base/theme.css';
import '../css/advancedsearch.css'; 

import * as $ from 'jquery';
import 'jquery-ui/ui/widgets/autocomplete';


//import 'jquery-ui/themes/base/selectmenu.css';
//import 'jquery-ui/themes/base/menu.css';
//import 'jquery-ui/themes/base/autocomplete.css';


import 'foundation-sites';
import * as d3 from 'd3';
//var d3 = Object.assign({}, require("d3-selection"), require("d3-transition"), require("d3-hierarchy"));
import { webcrap } from "../../Shared/webcrap/webcrap";
import {
    Search,
    ICondition,
    ConditionBase,
    AndOrCondition,
    SearchNode,
    SearchEdge,
    GetCondition,
    MoveNodeRight,
    MoveNodeLeft,
    AddNode,
    RemoveNode,
    IsConditionValid,
    GetNode
} from "./Search";
import ViewTreeNode from "./ViewTreeNode";

//requires d3js
export default class AdvancedSearchCoordinator {
    SearchData: Search;
    PathElementID: string;
    ConditionElementID: string;
    Diameter: number;
    XSpacing: number;
    YSpacing: number;
    ConditionRoot: ViewTreeNode<ICondition>;
    NodeDetails: object;
    EdgeDetails: object;
    Tooltip: FoundationSites.Tooltip;
    AddingTemp: ViewTreeNode<ICondition>;

    constructor(pathelementid: string, conditionelementid: string) {
        //console.log("AdvancedSearchCoordinator started: " + elementid);
        //console.log(this);
        var me = this;
        this.SearchData = new Search();
        this.PathElementID = pathelementid;
        this.ConditionElementID = conditionelementid;
        this.Diameter = 30;
        this.XSpacing = 170;
        this.YSpacing = 70;
        this.ConditionRoot = null;
        this.NodeDetails = null;
        this.EdgeDetails = null;

        this.BindAutoComplete();
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

        var sharedsearchstring = (document.getElementById("sharedSearchString") as HTMLInputElement).value;
        console.log("sharedsearchstring: " + sharedsearchstring);
        if (webcrap.misc.isNullOrWhitespace(sharedsearchstring) === false) {
            try {

                this.SearchData = JSON.parse(webcrap.misc.decodeUrlB64(sharedsearchstring));
                if (this.SearchData.Condition) {
                    this.ConditionRoot = new ViewTreeNode(this.SearchData.Condition, 'Conditions', null);
                    this.ConditionRoot.Build();
                }
                console.log("this.SearchData");
                console.log(this.SearchData);
            } 
            catch {
                console.error("Unable to parse shared search string");
            }
            this.UpdateSearch(false);
        }
        
        d3.select("#searchPane").on("click", function () {
            me.onSearchPaneClicked();
        });
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
            AddNode(me.SearchData);
            me.UpdateNodes(false);
            me.UpdateEdges(false);
        });
        d3.select("#whereAddIcon").on("click", function () {
            me.onSearchConditionAddClicked(this as HTMLElement, true);
        });
        d3.select("#advSearchClearIcon").on("click", function () {
            me.Clear();
        });
        d3.select("#advSearchShareIcon").on("click", function () {
            me.Share();
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
            //console.log(data);
            me.NodeDetails = data;
        });
        webcrap.data.apiGetJson("/api/graph/edge/properties", function (data) {
            //console.log(data);
            me.EdgeDetails = data;
        });
    }

    onSearchPaneClicked() {
        this.hideNodeControls();
    }

    UpdateSearch(animate: boolean) {
        this.UpdateNodes(animate);
        this.UpdateEdges(animate);
        this.UpdateConditions();
    }

    RunSearch () {
        console.log("RunSearch started");
        console.log(this);

        if (this.ConditionRoot !== null && IsConditionValid(this.ConditionRoot.Item) === false) {
            alert("You have conditions with incomplete data. This search is cannnot continue");
            console.log("Invalid condition found. Search cancelled");
            return;
        }
        var postdata = JSON.stringify(this.SearchData);
        console.log("search post:");
        console.log(postdata);
        this.ShowSearchSpinner();
        webcrap.data.apiPostJson("/visualizer/AdvancedSearch", postdata, function (data) {
            //console.log(data);
            document.getElementById("searchNotification").innerHTML = data;
            $('#searchNotification').foundation();
        });
    }

    ShowSearchSpinner() {
        document.getElementById("searchNotification").innerHTML = "<i class='fas fa-spinner spinner'></i>";
    }

    Clear() {
        //console.log("Clear started");
        //console.log(this);

        if (confirm("Are you sure you want to clear this search?") === true) {

            var viewel = d3.select("#" + this.PathElementID);
            viewel.selectAll("*").remove();
            viewel = d3.select("#" + this.ConditionElementID);
            viewel.selectAll("*").remove();
            this.SearchData = new Search();
        }
    }

    Share() {
        console.log("Share started");
        //console.log(this);
        if (this.SearchData.Nodes === null || this.SearchData.Nodes.length === 0) {
            alert("The search is empty. There is nothing to share.");
        }
        else {
            if (this.ConditionRoot !== null && IsConditionValid(this.ConditionRoot.Item) === false) {
                var conf = confirm("You have conditions with incomplete data. This search is not valid. Do you with to continue?");
                if (!conf) { return;}
            }
            var urlBase = [location.protocol, '//', location.host, location.pathname].join('');
            var encodedData = webcrap.misc.encodeUrlB64(JSON.stringify(this.SearchData));
            console.log(urlBase);
            console.log(encodedData);
            var url = urlBase + "?sharedsearch=" + encodedData;
            this.ShowMessage("Copy and paste this url:","<a href='" + url + "' >" + url + "</a>");
        }
    }

    ShowMessage(message: string, link?: string) {
        document.getElementById('message').innerHTML = message;
        document.getElementById('messageLink').innerHTML = link;
        $('#searchMessage').foundation('open');
    }

    UpdateNodes(animate: boolean) {
        //console.log("UpdateNodes started");
        //console.log(this);
        var me = this;
        var currslot = 0;

        var viewel = d3.select("#" + this.PathElementID);
        var newnodeg = viewel.selectAll(".searchnode")
            .data(this.SearchData.Nodes, function (d: SearchNode) { return d.Name; })
            .enter()
            .append("g")
            .classed("searchnode",true)
            //.attr("id", function (d: SearchNode) { return "searchnode_" + d.Name; })
            .attr("width", this.Diameter)
            .attr("height", this.Diameter)
            //.attr("data-open", "searchNodeDetails")
            .on("click", function () {
                d3.event.stopPropagation();
                me.onSearchNodeClicked(this);
            });

        newnodeg.append("circle")
            .attr("id", function (d: SearchNode) { return "searchnodebg_" + d.Name; })
            .classed("searchnodecircle", true)
            .attr("r", this.Diameter);

        newnodeg.append("text")
            .text(function (d: SearchNode) { return d.Name; })
            .attr("text-anchor", "middle")
            .attr("dominant-baseline", "central"); 

        viewel.selectAll(".searchnode")
            .data(this.SearchData.Nodes, function (d: SearchNode) { return d.Name; })
            .exit()
            .remove();

        var allnodes = viewel.selectAll(".searchnode")
            .data(this.SearchData.Nodes, function (d: SearchNode) { return d.Name; })
            .attr("id", function (d: SearchNode) { return "searchnode_" + d.Name; })
            .attr("class", function (d: SearchNode) { return d.Label; })
            .classed("searchnode", true);

        //tranform function to be used below
        var transform = function (d) {
            currslot++;
            d.index = currslot;
            d.x = me.XSpacing * currslot - me.XSpacing * 0.5;
            d.y = me.YSpacing;
            return "translate(" + d.x + "," + d.y + ")";
        };

        if (animate) {
            allnodes.transition()
                .duration(500)
                .attr("transform", transform );
        }
        else {
            allnodes.attr("transform", transform);
        }
        

        allnodes.select("text")
            .text(function (d: SearchNode) { return d.Name; });
        //.attr("transform", function (d) { return "translate(0," + (this.Diameter + 10) + ")"; });
    }

    UpdateEdges (animate: boolean) {
        //console.log("UpdateEdges start");
        //console.log(this);
        //console.log(this.AdvancedSearch.Edges);
        var me = this;

        var rectwidth = this.Diameter * 2;
        var rectheight = this.Diameter * 0.7;

        var relarrowwidth = 20;
        var currslot = 0;
        //console.log("subspacingx: " + subspacingx);
        //console.log("subspacingy: " + subspacingy);

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

        //console.log(this.SearchData.Edges);

        viewel.selectAll(".searchedge")
            .data(this.SearchData.Edges, function (d: SearchEdge) { return d.Name; })
            .exit()
            .remove();

        var transform = function () {
            //console.log(d);
            currslot++;
            var subspacingx = (currslot + 0.5) * me.XSpacing - rectwidth / 2 - me.XSpacing * 0.5;
            var subspacingy = me.YSpacing - rectheight / 2;
            //console.log(currslot + ": " + subspacingx + ": " + subspacingy);
            //console.log("edge transforms");
            //console.log(d);
            return "translate(" + subspacingx + "," + subspacingy + ")";
        };

        if (animate) {
            viewel.selectAll(".searchedge")
                .data(this.SearchData.Edges, function (d: SearchEdge) { return d.Name; })
                .transition()
                .duration(500)
                .attr("transform", transform);
        }
        else {
            viewel.selectAll(".searchedge")
                .data(this.SearchData.Edges, function (d: SearchEdge) { return d.Name; })
                .attr("transform", transform);
        }
    }

    showNodeControls(searchnode: SearchNode) {
        //var nodetransform = callingEl.getAttribute("transform");
        var controlIconDimension = 25;
        var controlsx = 0;
        var controlsy = 0;
        var me = this;

        var controls = d3.select("#" + this.PathElementID)
            .append("g")
            .attr("id", "nodecontrols")
            .attr("transform", "translate(" + (searchnode.x - controlIconDimension * 2) + "," + (searchnode.y - this.Diameter - controlIconDimension - 10) + ")");
        //.attr("transform", nodetransform);

        //console.log("transform", "translate(" + (searchnode.x - controlIconDimension * 2) + "," + (searchnode.y - this.Diameter - controlIconDimension - 10) + ")");
        //class="cell viewcontrol has-tip"

        if (searchnode.index > 1) {
            controls.append("g")
                .classed("viewcontrol", true)
                .classed("nodecontrol", true)
                .attr("transform", "translate(" + controlsx + "," + controlsy + ")")
                .on("click", function () {
                    d3.event.stopPropagation();
                    MoveNodeLeft(searchnode, me.SearchData);
                    me.hideNodeControls();
                    me.UpdateNodes(true);
                    me.showNodeControls(searchnode);
                })
                .append("i")
                .attr("height", controlIconDimension)
                .attr("width", controlIconDimension)
                .attr("class", "fas fa-caret-left");
        }

        controls.append("g")
            .classed("viewcontrol", true)
            .classed("nodecontrol", true)
            .attr("transform", "translate(" + (controlsx + controlIconDimension) + "," + controlsy + ")")
            .on("click", function () {
                d3.event.stopPropagation();
                me.hideNodeControls();
                me.showNodeEdit(searchnode);
            })
            .append("i")
            .attr("height", controlIconDimension)
            .attr("width", controlIconDimension)
            .attr("class", "fas fa-edit");
        controls.append("g")
            .classed("viewcontrol", true)
            .classed("nodecontrol", true)
            .attr("transform", "translate(" + (controlsx + controlIconDimension * 2) + "," + controlsy + ")")
            .on("click", function () {
                d3.event.stopPropagation();
                me.deleteSearchNode(searchnode);
            })
            .append("i")
            .attr("height", controlIconDimension)
            .attr("width", controlIconDimension)
            .attr("class", "far fa-trash-alt");

        if (searchnode.index < this.SearchData.AddedNodes)
            controls.append("g")
                .classed("viewcontrol", true)
                .classed("nodecontrol", true)
                .attr("transform", "translate(" + (controlsx + controlIconDimension * 3) + "," + controlsy + ")")
                .on("click", function () {
                    d3.event.stopPropagation();
                    MoveNodeRight(searchnode, me.SearchData);
                    me.hideNodeControls();
                    me.UpdateNodes(true);
                    me.showNodeControls(searchnode);
                })
                .append("i")
                .attr("height", controlIconDimension)
                .attr("width", controlIconDimension)
                .attr("class", "fas fa-caret-right");

        controls.selectAll(".nodecontrol")
            .append("rect")
            .attr("height", controlIconDimension)
            .attr("width", controlIconDimension);

    }

    hideNodeControls() {
        d3.selectAll("#nodecontrols").remove();
    }

    showNodeEdit(searchnode: SearchNode) {
        (document.getElementById("nodeType") as HTMLSelectElement).value = searchnode.Label;
        (document.getElementById("nodeIdentifier") as HTMLInputElement).value = searchnode.Name;
        $('#searchNodeDetails').foundation('open');
    }

    showConditionControls(treenode: ViewTreeNode<ICondition>) {

    }

    onSearchNodeClicked(callingEl: SVGElement) {
        //console.log("onSearchNodeClicked started");
        //console.log(this);
        //update the datum on the callingEl
        var datum = this.UpdateItemDatum("searchNodeDetails", callingEl) as SearchNode;

        console.log(datum);
        this.hideNodeControls();
        this.showNodeControls(datum);
    }

    onSearchEdgeClicked (callingEl) {
        //console.log("onSearchEdgeClicked started");
        //console.log(this);
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
        //console.log("onHopsSwitchChanged started");
        var hopsswitch: HTMLInputElement = document.getElementById("hopsSwitch") as HTMLInputElement;
        //console.log(hopsswitch.checked);

        if (hopsswitch.checked) {
            d3.selectAll(".hopscontrol")
                .attr("disabled", null);
        }
        else {
            d3.selectAll(".hopscontrol")
                .attr("disabled", "disabled");
        }

    }

    UpdateItemDatum(elementid: string, callingitem: Element) {
        //console.log("SearchItemClicked started");
        //console.log(this);
        //console.log(elementid);
        var el = d3.select("#" + elementid);
        var datum = d3.select(callingitem).datum();
        //console.log(datum);
        el.datum(datum);
        return datum;
    }

    GetItemDatum (elementid) {
        //console.log("GetItemDatum started");
        //console.log(this);
        //console.log(elementid);
        var el = d3.select("#" + elementid);
        var datum = el.datum();
        //console.log(datum);
        return datum;
    }


    GetElementDatum (element) {
        //console.log("GetElementDatum started");
        //console.log(this);
        //console.log(elementid);
        var el = d3.select(element);
        var datum = el.datum();
        //console.log(datum);
        return datum;
    }


    onSearchNodeSaveBtnClicked () {
        //console.log("onSearchNodeSaveBtnClicked started");
        //console.log(this);
        var node: SearchNode = d3.select("#searchNodeDetails").datum() as SearchNode;
        var nodeEl = d3.select("#searchnode_" + node.Name);

        //console.log(node);

        if (node.Label !== "") {
            nodeEl.classed(node.Label, false);
        }

        node.Name = (document.getElementById("nodeIdentifier") as HTMLInputElement).value;
        node.Label = (document.getElementById("nodeType") as HTMLSelectElement).value;
        //console.log(node);
        this.UpdateNodes(false);
    }

    onSearchNodeDelBtnClicked (callingitem) {
        //console.log("onSearchNodeDelBtnClicked started: " + callingitem);
        //console.log(this);

        var nodedatum = d3.select("#searchNodeDetails").datum() as SearchNode;
        //console.log(nodedatum);
        this.deleteSearchNode(nodedatum);
    }

    deleteSearchNode(searchnode: SearchNode) {
        console.log("deleteSearchNode started");
        console.log(searchnode);
        console.log(this.SearchData);
        if (confirm("Are you sure you want to delete " + searchnode.Name + "?")) {
            //console.log("deleting: " + searchnode.Name);
            RemoveNode(searchnode, this.SearchData);

            console.log(this.SearchData);
            this.hideNodeControls();
            this.UpdateNodes(true);
            this.UpdateEdges(true);
        }
        
    }

    ToggleDir () {
        //console.log("ToggleDir");
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
        //console.log("UpdateDirIcon started");
        var rightArr = true;
        if (dir === '<') {
            rightArr = false;
        }

        d3.select("#dirIcon")
            .attr("data-dir", dir)
            .classed("fa-arrow-right", rightArr)
            .classed("fa-arrow-left", !rightArr);
        //console.log("newdir: " + dir);
    }

    onSearchEdgeSaveBtnClicked () {
        //console.log("onSearchNodeSaveBtnClicked started");
        //console.log(this);
        var me = this;

        var edge: SearchEdge = d3.select("#searchEdgeDetails").datum() as SearchEdge;
        var edgeEl = d3.select("#searchedge_" + edge.Name);

        //console.log(edge);
        //console.log(edgeEl);

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
        //setTimeout(function () {
            me.UpdateEdges(false);
        //}, 10);
    }

    UpdateNodeProps () {
        //console.log("updateNodeProps started");
        var type = (document.getElementById("nodeType") as HTMLSelectElement).value;
        var el = (document.getElementById("nodeProp"));

        webcrap.dom.ClearOptions(el);
        let topoption = webcrap.dom.AddOption(el, "", "");
        topoption.setAttribute("disabled", "");
        topoption.setAttribute("hidden", "");
        topoption.setAttribute("selected", "");

        //console.log(this.NodeDetails);
        //console.log(type);

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

    BindAutoComplete() {
        var me = this;

        $('#searchVal').autocomplete({
            source: function (request, response) {
                
                var itemname = (document.getElementById("searchItem") as HTMLSelectElement).value;
                var datum: SearchNode = GetNode(itemname, me.SearchData);
                var prop = (document.getElementById("searchProp") as HTMLSelectElement).value;

                var url = "/api/graph/node/values?type=" + datum.Label + "&property=" + prop + "&searchterm=" + request.term;
                console.log(url);
                console.log("autoComplete: " + request.term);
                console.log(datum);

                webcrap.data.apiGetJson(url, response);
            }
        });
    }

    UpdateConditions () {
        console.log("UpdateConditions start:" + this.ConditionElementID);
        //console.log(this);
        //console.log(this.SearchData.Edges);

        var viewel = d3.select("#" + this.ConditionElementID);

        if (this.ConditionRoot === null) {
            viewel.selectAll(".searchcondition").remove();
            return;
        }

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

        
        console.log('nodes:');
        console.log(nodes);
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
        //    console.log("each");
        //    console.log(d.descendants().length);
        //    console.log(d);
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
                //console.log(d.ancestors());
                d.data.RectHeight = rectheight - d.depth * ypadding * 2;
                return d.data.RectHeight;
            })
            .attr("width", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
                //console.log("width d: ");
                //console.log(d); 
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
                event.stopPropagation();
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
                return "translate(" + x + "," + (y + me.YSpacing / 2) + ")";
            });
    }


    //Might come back to this. HTML condition setup rather than SVG
    //UpdateConditionsElement(element: HTMLElement) {
    //    console.log("UpdateConditionsElement start:" + element);
    //    //console.log(this);
    //    //console.log(this.SearchData.Edges);

        
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
    //    //    console.log("each");
    //    //    console.log(d.descendants().length);
    //    //    console.log(d);
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
    //            //console.log(d.ancestors());
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
    //            //console.log(d);
    //            var x = 0;
    //            var y = 0;
    //            if (d.parent !== null) {
    //                //console.log("parent");
    //                //console.log(d.parent);
    //                x = d.parent.x + xpadding + (xspacing * d.data.Index) + (d.data.Index * rectwidth);
    //                y = ypadding * d.depth;
    //            }
    //            d.x = x;
    //            d.y = y;
    //            return "translate(" + x + "," + (y + me.YSpacing / 2) + ")";
    //        });
    //}

    onSearchConditionClicked (callingelement) {
        //console.log("onSearchConditionClicked started");
        //var datum;
        //console.log(callingelement);
        var me = this;
        var condition: ConditionBase;
        this.ClearAlert();
        var datum = this.UpdateItemDatum("searchConditionDetails", callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        if (datum) { condition = datum.data.Item as ConditionBase; }
        console.log(datum);

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
        console.log("UpdateConditionDetails started");
        //console.log(this.SearchData.Nodes);
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
        //console.log("onSearchConditionItemChanged started");
        //console.log(this);

        var me = this;
        var searchItem: HTMLSelectElement = document.getElementById("searchItem") as HTMLSelectElement;
        var searchProps = document.getElementById("searchProp");
        var selectedName = searchItem.options[searchItem.selectedIndex].value;
        var selectedItem;
        var datum = (this.GetItemDatum("searchConditionDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
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
        //console.log(selectedItem);

        webcrap.dom.ClearOptions(searchProps);
        var props;
        if (selectedItem) {
            if (selectedItem.Label === "" || selectedItem.Label === "*") {
                console.log("node label not selected");
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


        //console.log(datum);
        //var label = datum.
    }

    onSearchConditionAddClicked(callingelement: Element, isroot?: boolean) {
        console.log("onSearchConditionAddClicked started: " + isroot);
        //var datum;
        var tempnode: ViewTreeNode<ICondition>; 
        if (isroot === true) {
            tempnode = new ViewTreeNode(null, 'Conditions', null);
        }
        else {
            var addingparent = (this.GetElementDatum(callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
            console.log(addingparent);
            tempnode = new ViewTreeNode(null, 'Conditions', addingparent);
        }
        console.log('tempnode:');
        console.log(tempnode);
        this.AddingTemp = tempnode;
        $('#searchConditionTypeSelect').foundation('open');
    }

    onSearchTypeSaveClicked() {
        console.log("onSearchTypeSaveClicked started");
        var selectedType: string = (document.getElementById("searchConditionTypeList") as HTMLSelectElement).value;
        var cond: ICondition = GetCondition(selectedType);
        this.AddingTemp.Item = cond;

        if (this.AddingTemp.Parent !== null) {
            this.AddingTemp.Parent.AddChild(this.AddingTemp);
            this.ConditionRoot.Rebuild();
        }
        else {
            this.ConditionRoot = this.AddingTemp;
            this.SearchData.Condition = this.ConditionRoot.Item;
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
        console.log("deleteSearchCondition started: " + elementid);
        //searchConditionDetails

        //datum is the d3 tree datum. Use datum.data to get the ViewTreeNode datum
        var datum = this.GetItemDatum(elementid) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        var treenode = datum.data;

        console.log(datum);
        console.log("RootID: " + this.ConditionRoot.ID);
        if (this.ConditionRoot.ID === treenode.ID) {
            this.ConditionRoot = null;
            this.SearchData.Condition = undefined;
            d3.selectAll(".searchcondition").remove();
        }
        else {
            datum.parent.data.RemoveChild(treenode);
            datum.parent.data.Rebuild();
            this.UpdateConditions();
        }

        console.log(this.ConditionRoot);
        console.log("onSearchConditionDeleteClicked finished");
    }


    onSearchConditionSaveClicked () {
        //console.log("onSearchConditionSaveClicked started");
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
        //console.log("ChangeSelectedValue started");
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
    }

    onSearchAndOrSaveClicked () {
        //console.log("onSearchAndOrSaveClicked started");
        var datum = this.GetItemDatum("searchAndOrDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>;

        datum.data.Item.Type = (document.getElementById("searchAndOr") as HTMLSelectElement).value;
    }

    onSearchAndOrClicked (callingelement) {
        console.log("onSearchAndOrClicked started");
        this.ClearAlert();
        var datum = this.UpdateItemDatum("searchAndOrDetails", callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        console.log(datum);
        var cond: AndOrCondition = datum.data.Item as AndOrCondition;

        (document.getElementById("searchAndOr") as HTMLInputElement).value = cond.Type;
    };

    ClearAlert () {
        var alertEl = $("#alertIcon");
        alertEl.hide();
    }

    SetAlert (message: string) {
        console.log("SetAlert started: " + message);
        console.log(this.Tooltip);
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
