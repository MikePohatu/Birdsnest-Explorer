/// <reference path="../../../node_modules/foundation-sites/js/typescript/foundation.d.ts" />
/// <reference path="../../script/webcrap.ts" />
/// <reference path="../../../node_modules/@types/d3/index.d.ts" />
/// <reference path="../../../node_modules/@types/jqueryui/index.d.ts" />

//requires d3js
namespace AdvancedSearch {
    class AdvancedSearchCoordinator {
        SearchData: Search;
        PathElementID: string;
        ConditionElementID: string;
        Radius: number;
        XSpacing: number;
        YSpacing: number;
        ConditionRoot: ICondition;
        ConditionD3Root = null;
        NodeDetails: object;
        EdgeDetails: object;

        constructor(pathelementid: string, conditionelementid: string) {
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
            webcrap.data.apiGetJson("/api/graph/node/properties", function (data) {
                //console.log(data);
                me.NodeDetails = data;
            });
            webcrap.data.apiGetJson("/api/graph/edge/properties", function (data) {
                //console.log(data);
                me.EdgeDetails = data;
            });
        }



        RunSearch = function () {
            //console.log("RunSearch started");
            //console.log(this);


            var postdata = JSON.stringify(this.SearchData);
            //console.log("search post:");
            //console.log(postdata);
            this.ShowSearchSpinner();
            webcrap.data.apiPostJson("AdvancedSearch", postdata, function (data) {
                //console.log(data);
                document.getElementById("searchNotification").innerHTML = data;
                $('#searchNotification').foundation();
            });
        }

        ShowSearchSpinner() {
            document.getElementById("searchNotification").innerHTML = "<i class='fas fa-spinner spinner'></i>";
        }

        Clear = function () {
            //console.log("Clear started");
            //console.log(this);

            if (confirm("Are you sure you want to clear this search?") === true) {

                var viewel = d3.select("#" + this.PathElementID);
                viewel.selectAll("*").remove();
                this.SearchData = new Search();
            }

        }

        UpdateNodes = function () {
            //console.log("UpdateNodes started");
            //console.log(this);
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

        UpdateEdges = function () {
            //console.log("UpdateEdges start");
            //console.log(this);
            //console.log(this.AdvancedSearch.Edges);
            var me = this;

            var rectwidth = this.Radius * 2;
            var rectheight = this.Radius * 0.7;

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
                .attr("transform", function () {
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
        }


        onSearchNodeClicked = function (callingEl) {
            //console.log("onSearchNodeClicked started");
            //console.log(this);
            var datum = this.UpdateItemDatum("searchNodeDetails", callingEl);
            (document.getElementById("nodeType") as HTMLSelectElement).value = datum.Label;
            (document.getElementById("nodeIdentifier") as HTMLInputElement).value = datum.Name;
        }

        onSearchEdgeClicked = function (callingEl) {
            //console.log("onSearchEdgeClicked started");
            //console.log(this);
            var datum = this.UpdateItemDatum("searchEdgeDetails", callingEl);
            (document.getElementById("edgeType") as HTMLSelectElement).value = datum.Label;
            (document.getElementById("edgeIdentifier") as HTMLInputElement).value = datum.Name;

            var hopsswitch: HTMLInputElement = document.getElementById("hopsSwitch") as HTMLInputElement;
            var minsliderval: HTMLInputElement = document.getElementById("minSliderVal") as HTMLInputElement;
            var maxsliderval: HTMLInputElement = document.getElementById("maxSliderVal") as HTMLInputElement;

            this.UpdateDirIcon(datum.Direction);

            if (datum.Min < 0 || datum.Max < 0) {
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

        onHopsSwitchChanged = function () {
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

        UpdateItemDatum = function (elementid, callingitem) {
            //console.log("SearchItemClicked started");
            //console.log(this);
            //console.log(elementid);
            var el = d3.select("#" + elementid);
            var datum = d3.select(callingitem).datum();
            //console.log(datum);
            el.datum(datum);
            return datum;
        }

        GetItemDatum = function (elementid) {
            //console.log("GetItemDatum started");
            //console.log(this);
            //console.log(elementid);
            var el = d3.select("#" + elementid);
            var datum = el.datum();
            //console.log(datum);
            return datum;
        }


        GetElementDatum = function (element) {
            //console.log("GetElementDatum started");
            //console.log(this);
            //console.log(elementid);
            var el = d3.select(element);
            var datum = el.datum();
            //console.log(datum);
            return datum;
        }


        onSearchNodeSaveBtnClicked = function () {
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

            nodeEl
                .attr("id", "searchnode_" + node.Name)
                .classed(node.Label, true);
            nodeEl.select("text")
                .text(node.Name);
        }

        onSearchNodeDelBtnClicked = function (callingitem) {
            //console.log("onSearchNodeDelBtnClicked started: " + callingitem);
            //console.log(this);

            var nodedatum = d3.select("#searchNodeDetails").datum();
            //console.log(nodedatum);
            this.SearchData.RemoveNode(nodedatum);

            this.UpdateNodes();
            this.UpdateEdges();
        }

        ToggleDir = function () {
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

        UpdateDirIcon = function (dir) {
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

        onSearchEdgeSaveBtnClicked = function () {
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
            setTimeout(function () {
                me.UpdateEdges();
            }, 10);
        }

        UpdateNodeProps = function () {
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

        UpdateNodeLabels = function () {
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

        UpdateEdgeLabels = function () {
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

        BindAutoComplete = function () {
            $("#searchVal").autocomplete({
                source: function (request, response) {
                    //console.log("autoComplete: "+ request.term);
                    var type = (document.getElementById("searchType") as HTMLSelectElement).value;
                    var prop = (document.getElementById("searchProp") as HTMLSelectElement).value;

                    var url = "/api/graph/node/values?type=" + type + "&property=" + prop + "&searchterm=" + request.term;
                    webcrap.data.apiGetJson(url, response);
                }
            });
        }

        AddConditionRoot = function () {
            //console.log("AddConditionRoot started");
            //d3.select("#whereAddIcon").classed("hidden", true);

            var cond1 = new StringCondition();
            var cond2 = new StringCondition();
            var cond3 = new AndOrCondition();
            var cond4 = new StringCondition();
            var cond5 = new StringCondition();

            var andcond = new AndOrCondition();

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
        }

        onAddCondition = function () {

        }

        NewCondition = function () {
            return new StringCondition();
        }

        UpdateConditions = function () {
            //console.log("UpdateConditions start:" + this.ConditionElementID);
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
                .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.ID; })
                .enter()
                .append("g")
                .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return "searchcondition_" + (d.data as ViewTreeNode).ID; })
                .attr("class", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
                    if (d.data.Item instanceof AndOrCondition) {
                        return "conditiongroup";
                    }
                    else {
                        return "condition";
                    }
                })
                .classed("searchcondition", true)
                .attr("data-open", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
                    if (d.data.Item.Type === "AND" || d.data.Item.Type === "OR") {
                        return "searchAndOrDetails";
                    }
                    return "searchConditionDetails";
                })
                .on("click", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
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
            viewel.selectAll(".searchcondition")
                .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.ID; })
                .exit()
                .remove();

            enter.append("rect")
                .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return "searchconditionbg_" + d.data.ID; })
                .classed("searchconditionrect", true)
                .attr("height", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
                    //console.log(d.ancestors());
                    d.data.RectHeight = rectheight - d.depth * ypadding * 2;
                    return d.data.RectHeight;
                })
                .attr("width", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
                    if (d.value > 1) {
                        d.data.RectWidth = d.value * rectwidth + (d.value - 1) * xspacing + (d.descendants().length - 1) * 2 + strokewidth * d.value + editwidth + xpadding * 2;
                    }
                    else {
                        d.data.RectWidth = rectwidth;
                    }
                    return d.data.RectWidth;
                })
                .attr("x", strokewidth)
                .attr("y", strokewidth)
                .attr("rx", 5);

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
                .text(function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.Item.Type; });

            viewel.selectAll(".condition .searchconditioneval")
                .text(function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.Item.Name + "." + (d.data.Item as ConditionBase).Property; });

            viewel.selectAll(".condition .searchconditionopval")
                .text(function (d: d3.HierarchyPointNode<ViewTreeNode>) { return (d.data.Item as ConditionBase).Operator + " " + (d.data.Item as ConditionBase).Value; });

            //setup the + button for group nodes e.g. AND/OR
            var groupaddbtns = viewel.selectAll(".conditiongroup")
                .append("g")
                .attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return "searchconditionplus_" + d.data.ID; })
                .classed("searchconditionplus", true)
                .classed("searchcontrol", true)
                .on("click", function () {
                    me.onSearchConditionAddClicked(this);
                });

            groupaddbtns.append("i")
                .attr("class", "fas fa-plus")
                .attr("width", pluswidth)
                .attr("height", pluswidth)
                .attr("x", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.RectWidth - pluswidth - xpadding; })
                .attr("y", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.RectHeight - ypadding - pluswidth; });


            groupaddbtns.append("rect")
                .attr("width", pluswidth)
                .attr("height", pluswidth)
                .attr("x", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.RectWidth - pluswidth - xpadding; })
                .attr("y", function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.RectHeight - ypadding - pluswidth; });

            viewel.selectAll(".searchcondition")
                .data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode>) { return d.data.ID; })
                .attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode>) {
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

        onSearchConditionClicked = function (callingelement) {
            //console.log("onSearchConditionClicked started");
            //var datum;
            //d3.select(callingelement)
            //    .each(function (d) { datum = d.data; });
            //console.log(callingelement);
            var me = this;
            this.ClearAlert();
            var datum = this.UpdateItemDatum("searchConditionDetails", callingelement);
            if (datum) { datum = datum.data; }
            //console.log(datum);

            (document.getElementById("searchProp") as HTMLSelectElement).disabled = false;
            (document.getElementById("searchVal") as HTMLInputElement).disabled = false;
            (document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = false;

            this.UpdateConditionDetails(function () {
                me.ChangeSelectedValue(document.getElementById("searchItem"), datum.Item.Name, function () {
                    me.ChangeSelectedValue(document.getElementById("searchProp"), datum.Item.Property);
                });
            });

            (document.getElementById("searchVal") as HTMLInputElement).value = datum.Item.Value;
        }

        UpdateConditionDetails = function (callback) {
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

        onSearchConditionItemChanged = function () {
            //console.log("onSearchConditionItemChanged started");
            //console.log(this);

            var me = this;
            var searchItem: HTMLSelectElement = document.getElementById("searchItem") as HTMLSelectElement;
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

        onSearchConditionAddClicked = function (callingelement) {
            console.log("onSearchConditionAddClicked started");
            //var datum;

        }

        onSearchConditionDeleteClicked = function () {
            console.log("onSearchConditionDeleteClicked started");
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
            console.log("onSearchConditionDeleteClicked finished");
        }


        onSearchConditionSaveClicked = function () {
            //console.log("onSearchConditionSaveClicked started");
            var datum = this.GetItemDatum("searchConditionDetails").data;

            var newname = (document.getElementById("searchItem") as HTMLSelectElement).value;
            var newprop = (document.getElementById("searchProp") as HTMLSelectElement).value;
            var newval = (document.getElementById("searchVal") as HTMLInputElement).value;

            if (webcrap.isNullOrWhitespace(newname) || webcrap.isNullOrWhitespace(newprop) || webcrap.isNullOrWhitespace(newval)) {
                this.SetAlert("Name, property, or value is empty. Please set a value");
            }
            else {
                datum.Item.Name = newname;
                datum.Item.Property = newprop;
                datum.Item.Value = newval;
                $('#searchConditionDetails').foundation('close');
                this.UpdateConditions();
            }

        }

        ChangeSelectedValue = function (selectEl, value, callback) {
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

        onSearchAndOrSaveClicked = function () {
            //console.log("onSearchAndOrSaveClicked started");
            var datum = this.GetItemDatum("searchAndOrDetails").data;

            datum.Item.Type = (document.getElementById("searchAndOr") as HTMLSelectElement).value;
        }

        onSearchAndOrClicked = function (callingelement) {
            //console.log("onSearchAndOrClicked started");
            var me = this;
            this.ClearAlert();
            var datum = this.UpdateItemDatum("searchAndOrDetails", callingelement);
            if (datum) { datum = datum.data; }
            //console.log(datum);

            (document.getElementById("searchAndOr") as HTMLInputElement).value = datum.Item.Type;
        };

        ClearAlert = function () {
            var alertEl = $("#alertIcon");
            alertEl.hide();
        }

        SetAlert = function (message) {
            var alertEl = $("#alertIcon");
            new Foundation.Tooltip(alertEl, <FoundationSites.ITooltipOptions>{
                allowHtml: true,
                tipText: message
            });
            alertEl.show();
        }
    }



    export class Search {
        Condition;
        Nodes = [];
        Edges = [];
        AddedNodes = 0;


        AddNode() {
            //console.log("AddNode start:");
            //console.log(this);
            var me = this;
            me.AddedNodes++;
            var newNode = new SearchNode;
            this.Nodes.push(newNode);
            newNode.Name = "node" + me.AddedNodes;

            if (me.Nodes.length > 1) {
                var newEdge = new SearchEdge;
                this.Edges.push(newEdge);
                newEdge.Name = "hop" + (me.AddedNodes - 1);
            }

            return newNode;
            //console.log("AddNode end:");
            //console.log(me);
        }


        //remove the node and return the index of the node that was removed
        RemoveNode(node) {
            var me = this;
            var foundindex = -1;
            var i;
            for (i = 0; i < me.Nodes.length; i++) {
                if (foundindex !== -1) { //if found already, start shifting nodes back on in the array
                    me.Nodes[i - 1] = me.Nodes[i];
                    if (i < me.Edges.length) { me.Edges[i - 1] = me.Edges[i]; }
                }
                else {
                    if (me.Nodes[i] === node) {
                        foundindex = i;
                        if (i === 0) {
                            me.Nodes.shift; //remove the first node
                            if (me.Edges.length > 0) { me.Edges.shift; } //if there is an edge, remove that too

                            //we're done
                            return foundindex;
                        }
                    }
                }
            }

            if (foundindex !== -1) {
                // pop off the end if the node wasn't first i.e hasn't been removed with shift
                me.Nodes.pop;
                me.Edges.pop;
            }

            return foundindex;
        }

        MoveNodeRight(node) {
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
        }

        MoveNodeLeft(node) {
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
        }
    }


    export class SearchNode {
        Name: string = "";
        Label: string = "";
    }

    export class SearchEdge {
        Name: string = "";
        Label: string = "";
        Direction: string = ">";
        Min: string = "1";
        Max: string = "1";
    }

    export interface ICondition {
        Type: string;
        Name: string;
    }

    export class AndOrCondition implements ICondition {
        Name: string = "";
        Conditions: ICondition[] = [];

        private _type: string = "AND";
        get Type(): string {
            return this._type;
        }
        set bar(value: string) {
            if (value === "AND" || value === "OR") {
                this._type = value;
            }
            else {
                console.log("Cannot set type of AND/OR condition to " + value)
            }
        }

    }

    export class ConditionBase implements ICondition {
        Type: string = "";
        Name: string = "";
        Property: string = "";
        Value: any;
        Operator: string;
    }

    export class MathCondition extends ConditionBase {
        Value: number = 0;

        constructor() {
            super();
            this.Type = "MATH";
            this.Name = "";
            this.Property = "";
            this.Operator = "=";
        }
    }

    export class StringCondition extends ConditionBase {
        Value: string = "";

        constructor() {
            super();
            this.Type = "STRING";
            this.Name = "";
            this.Property = "";
            this.Operator = "=";
        }
    }
}

    
 