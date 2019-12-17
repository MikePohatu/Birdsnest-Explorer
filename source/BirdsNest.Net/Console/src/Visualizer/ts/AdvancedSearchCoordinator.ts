import '../css/advancedsearch.css'; 

import * as $ from 'jquery';
import 'jquery-ui/ui/widgets/autocomplete';

import 'foundation-sites';
import * as d3 from "../js/visualizerD3";

import { webcrap } from "../../shared/webcrap/webcrap";
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
	GetNode,
	ConditionOperators,
	StringCondition,
    GetEdge
} from "./Search";
import ViewTreeNode from "./ViewTreeNode";

//requires d3js
export default class AdvancedSearchCoordinator {
	
	SearchData: Search;
	PathElementID: string;
	ConditionElementID: string;
	Diameter: number = 25;
	XSpacing: number = 170;
	YSpacing: number = 0;
	PlusSize: number = 25;
	PaneHeight: number = 65;
	Margin: number = 5;
	SvgContainerHeight: number = 90;
	ConditionRoot: ViewTreeNode<ICondition> = null;
	NodeDetails: object = null;
	EdgeDetails: object = null;
	SimpleMode: boolean = true;

	ViewTreeNodeConditionsProp = 'Conditions';

	Tooltip: FoundationSites.Tooltip;
	NewCondition: ViewTreeNode<ICondition> = null;
	EditingCondition: ViewTreeNode<ICondition> = null;

	ConditionTypeModal: FoundationSites.Reveal;
	ConditionDetailsModal: FoundationSites.Reveal;
	NodeDetailsModal: FoundationSites.Reveal;
	EdgeDetailsModal: FoundationSites.Reveal;
	SearchAndOrDetailsModal: FoundationSites.Reveal;
	SearchAlertModal: FoundationSites.Reveal;
	ShareModal: FoundationSites.Reveal;

	constructor(pathelementid: string, conditionelementid: string) {
		//console.log("AdvancedSearchCoordinator started: " + elementid);
		//console.log(this);
		var me = this;
		this.SearchData = new Search();
		this.PathElementID = pathelementid;
		this.ConditionElementID = conditionelementid;

		var modalOptions: FoundationSites.IRevealOptions = {
			animationIn: "hinge-in-from-top fast",
			animationOut: "hinge-out-from-top fast",
			closeOnClick: true
		};

		this.ConditionTypeModal = new Foundation.Reveal($("#searchConditionTypeSelect"), modalOptions);
		this.ConditionDetailsModal = new Foundation.Reveal($("#searchStringConditionDetails"), modalOptions);
		this.NodeDetailsModal = new Foundation.Reveal($("#searchNodeDetails"), modalOptions);
		this.EdgeDetailsModal = new Foundation.Reveal($("#searchEdgeDetails"), modalOptions);
		this.SearchAndOrDetailsModal = new Foundation.Reveal($("#searchAndOrDetails"), modalOptions);
		this.SearchAlertModal = new Foundation.Reveal($("#searchAlert"), modalOptions);
        this.ShareModal = new Foundation.Reveal($("#shareReveal"), modalOptions);

		this.BindAdvAutoComplete();
		this.BindSimpleAutoComplete();
		webcrap.dom.BindEnterToButton("searchEdgeDetails", "searchEdgeSaveBtn");
		webcrap.dom.BindEnterToButton("searchNodeDetails", "searchNodeSaveBtn");
		webcrap.dom.BindEnterToButton("searchAndOrDetails", "searchAndOrSaveBtn");
		webcrap.dom.BindEnterToButton("simplequery", "simpleSearchBtn", function () {
			$("#simpleSearchTerm").autocomplete("close");
		});

		

		d3.selectAll(".searchSvgContainer").attr("height", this.SvgContainerHeight + "px");

		d3.select("#advSearchView").on("click", function () {
			me.onSearchPaneClicked();
		});

		//advanced/simple mode toggles
		d3.select("#advanceToSimple").on("click", function () {
			me.ToggleSimpleMode();
		});
		d3.select("#simpleToAdvanced").on("click", function () {
			me.ToggleSimpleMode();
		});
		d3.select("#simpleSearchMinimise").on("click", function () {
			me.ShowHide();
		});
		d3.select("#advSearchMinimise").on("click", function () {
			me.ShowHide();
		});
		d3.select("#searchMaximise").on("click", function () {
			me.ShowHide();
		});

		//sliders
        $("#minHopsSlider").foundation();
        $("#maxHopsSlider").foundation();
        $("#minHopsSlider").on("changed.zf.slider", function () {
			me.onMinSliderChanged();
		});
        $("#maxHopsSlider").on("changed.zf.slider", function() {
			me.onMaxSliderChanged();
		});

		//node details dialogs
		d3.select("#searchNodeSaveBtn").on("click", function () {
			me.onSearchNodeSaveBtnClicked();
		});
		d3.select("#searchNodeSaveAndCondBtn").on("click", function () {
			me.onSearchNodeSaveAndCondClicked();
		});
		d3.select("#searchProp").on("change", function () {
			me.onSearchPropChanged();
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
			me.RunAdvSearch();
		});
		d3.select("#simpleSearchBtn").on("click", function () {
			me.RunSimpleSearch();
		});

		d3.select("#pathAddIcon").on("click", function () {
			AddNode(me.SearchData);
			me.UpdateNodes(false);
			me.UpdateEdges(false);
		});
		d3.select("#advSearchClearBtn").on("click", function () {
			me.Clear();
		});
		d3.select("#advSearchShareBtn").on("click", function () {
			me.Share();
		});
		d3.select("#hopsSwitch").on("change", function () {
			me.onHopsSwitchChanged();
		});
		d3.select("#searchConditionDeleteBtn").on("click", function () {
			me.onSearchConditionDeleteClicked();
		});
		d3.select("#searchConditionCancelBtn").on("click", function () {
			me.onSearchConditionCancelClicked();
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
		d3.select("#searchConditionTypeSelectSaveBtn").on("click", function () {
			me.onSearchConditionTypeSaveClicked();
		});

		me.UpdateNodeLabels();
		me.UpdateEdgeLabels();
		me.ResetRootTreeNode();
		me.UpdateConditions();
		webcrap.data.apiGetJson("/api/graph/node/properties", function (data) {
			//console.log(data);
			me.NodeDetails = data;
		});
		webcrap.data.apiGetJson("/api/graph/edge/properties", function (data) {
			//console.log(data);
			me.EdgeDetails = data;
		});
	}

	LoadSearchJson(json) {
		//console.log("LoadSearchJson: ");
		//console.log(json);
		this.SearchData = json;
		if (this.SearchData.Condition) {
			this.ConditionRoot = new ViewTreeNode(this.SearchData.Condition, this.ViewTreeNodeConditionsProp, null);
			this.ConditionRoot.Build();
		}
		this.UpdateSearch(false);
		this.SimpleMode = false;
		this.ShowHide();
	}

	onSearchPaneClicked() {
		this.hideNodeControls();
	}

	onMinSliderChanged() {
		//console.log("onMinSliderChanged");
		var maxel = (document.getElementById("maxSliderVal") as HTMLInputElement);
		var max = maxel.valueAsNumber;
		var min = (document.getElementById("minSliderVal") as HTMLInputElement).valueAsNumber;

        if (max < min) {
            maxel.valueAsNumber = min;
            d3.select('#maxSliderVal').dispatch('change');
        }
	}

	onMaxSliderChanged() {
		//console.log("onMaxSliderChanged");
		var minel = (document.getElementById("minSliderVal") as HTMLInputElement);
		var min = minel.valueAsNumber;
		var max = (document.getElementById("maxSliderVal") as HTMLInputElement).valueAsNumber;

        if (max < min) {
            minel.valueAsNumber = max;
            d3.select('#minSliderVal').dispatch('change');
        }
	}

	ShowHide() {
		
		if (this.SimpleMode) {
			$("#simplequery").slideToggle(400);
		}
		else {
			$("#querybar").slideToggle(400);
		}
		$("#searchMaximise").slideToggle(200);
	}

	ToggleSimpleMode() {
		this.SimpleMode = !this.SimpleMode;
		$("#simplequery").slideToggle(400);
		$("#querybar").slideToggle(400);
	}

	UpdateSearch(animate: boolean) {
		this.UpdateNodes(animate);
		this.UpdateEdges(animate);
		this.UpdateConditions();
	}

	RunSimpleSearch() {
		var searchterm = encodeURI((document.getElementById("simpleSearchTerm") as HTMLInputElement).value);
		//console.log("RunSimpleSearch" + searchterm);
		//console.log("/visualizer/SimpleSearch?searchterm=" + searchterm);
		this.ShowSearchSpinner("simpleSearchNotification");
		webcrap.data.apiGet("/visualizer/SimpleSearch?searchterm=" + searchterm, "html", function (data) {
			//console.log(data);
			document.getElementById("simpleSearchNotification").innerHTML = data;
			$('#simpleSearchNotification').foundation();
		});
	}

	RunAdvSearch () {
		//console.log("RunSearch started");
		//console.log(this);

		if (this.ConditionRoot !== null && IsConditionValid(this.ConditionRoot.Item) === false) {
			alert("You have conditions with incomplete data. This search is cannnot continue");
			//console.log("Invalid condition found. Search cancelled");
			return;
		}
		var postdata = JSON.stringify(this.SearchData);
		//console.log("search post:");
		//console.log(postdata);
		this.ShowSearchSpinner("searchNotification");
		webcrap.data.apiPostJson("/visualizer/AdvancedSearch", postdata, function (data) {
			//console.log(data);
			document.getElementById("searchNotification").innerHTML = data;
			$('#searchNotification').foundation();
		});
	}

	ShowSearchSpinner(elementid: string) {
		document.getElementById(elementid).innerHTML = "<i class='fas fa-spinner spinner'></i>";
	}

	Clear() {
		//console.log("Clear started");
		//console.log(this);

		if (confirm("Are you sure you want to clear this search?") === true) {
			var viewel = d3.select("#" + this.PathElementID);
			viewel.selectAll("*").remove();
			this.SearchData = new Search();
			this.ResetRootTreeNode();
			this.UpdateConditions();
		}
	}

	Share() {
		//console.log("Share started");
		//console.log(this);
        var me = this;
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
            //console.log(urlBase);
            //console.log(encodedData);
            var url = urlBase + "?sharedsearch=" + encodedData;
            document.getElementById('messageLink').innerHTML = "<a href='" + url + "' >" + url + "</a>";

            var postdata = JSON.stringify(this.SearchData);
            webcrap.data.apiPostJson("/visualizer/AdvancedSearchCypher", postdata, function (data) {
                //console.log(data);
                document.getElementById("cypherquery").innerHTML = data;
            });

            this.ShareModal.open();
		}
	}

	UpdateNodes(animate: boolean) {
		//console.log("UpdateNodes started");
		//console.log(this);
		var me = this;
		var currslot = 0;
		var strokewidth = 3;

		var viewel = d3.select("#" + this.PathElementID);
		var newnodeg = viewel.selectAll(".searchnode")
			.data(this.SearchData.Nodes, function (d: SearchNode) { return d.ID; })
			.enter()
			.append("g")
			.attr("id", function (d: SearchNode) { return "searchnode_" + d.ID; })
			.classed("searchnode", true)
			.classed("clickable", true)
			.attr("width", this.Diameter)
			.attr("height", this.Diameter)
			.on("click", function () {
				d3.event.stopPropagation();
				me.onSearchNodeClicked(this);
			});

		newnodeg.append("circle")
			.attr("id", function (d: SearchNode) { return "searchnodebg_" + d.ID; })
			.classed("searchnodecircle", true)
			.attr("r", this.Diameter);

		newnodeg.append("text")
			.text(function (d: SearchNode) { return d.Name; })
			.attr("text-anchor", "middle")
			.attr("dominant-baseline", "central"); 

		viewel.selectAll(".searchnode")
			.data(this.SearchData.Nodes, function (d: SearchNode) { return d.ID; })
			.exit()
			.remove();

		var allnodes = viewel.selectAll(".searchnode")
			.data(this.SearchData.Nodes, function (d: SearchNode) { return d.ID; })
			.attr("id", function (d: SearchNode) { return "searchnode_" + d.ID; })
			.attr("class", function (d: SearchNode) { return d.Label; })
			.classed("searchnode", true)
			.classed("clickable", true);

		//tranform function to be used below
		var transform = function (d) {
			currslot++;
			d.index = currslot;
			d.x = me.XSpacing * currslot - me.XSpacing * 0.5;
			d.y = me.SvgContainerHeight / 2 + me.Margin;
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

		newnodeg.each(function (d: SearchNode) {
			//console.log("newnode: " + d.ID);
			$("#searchnode_" + d.ID).foundation();
		});

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
		var rectheight = this.Diameter *1.5;

		var relarrowwidth = rectheight * 0.7;
		var relarrowy = (rectheight - relarrowwidth)/2;
		var currslot = 0;
		//console.log("subspacingx: " + subspacingx);
		//console.log("subspacingy: " + subspacingy);

		var viewel = d3.select("#" + this.PathElementID);
		var newedgeg = viewel.selectAll(".searchedge")
			.data(this.SearchData.Edges, function (d: SearchEdge) { return d.ID; })
			.enter()
			.append("g")
			.attr("id", function (d: SearchEdge) { return "searchedge_" + d.ID; })
			.classed("searchedge", true)
			.on("click", function () { me.onSearchEdgeClicked(this); });

		newedgeg.append("rect")
			.attr("id", function (d: SearchEdge) { return "searchedgebg_" + d.ID; })
			.classed("searchedgerect", true)
			.attr("width", rectwidth)
			.attr("height", rectheight);

		var newedgetext = newedgeg.append("text")
			.attr("text-anchor", "top")
			.attr("dominant-baseline", "baseline");

		//newedgetext.append("tspan")
		//	.attr("x", "5px")
		//	.attr("dy", "1.1em")
		//	.text(function (d: SearchEdge) { return d.Label; });

		newedgetext.append("tspan")
			.attr("x", "5px")
			.attr("dy", "1.1em")
			.text(function (d: SearchEdge) { return d.Name; });

		newedgetext.append("tspan")
			.attr("x", "5px")
			.attr("dy", "1.1em")
			.text(function (d: SearchEdge) {
				if (d.Max === "-1") {
					return "*";
				}
				else {
					return d.Min + ".." + d.Max;
				}
			});

		newedgeg.append("i")
			.attr("id", function (d: SearchEdge) { return "searchleftarr_" + d.ID; })
			.attr("class", function (d: SearchEdge) {
				if (d.Direction === ">") { return "fas fa-minus"; }
				else { return "fas fa-arrow-left"; }
			})
			.classed("searchleftarrow", true)
			.classed("searcharrow", true)
			.attr("x", 0 - relarrowwidth)
			.attr("y", relarrowy)
			.attr("width", relarrowwidth)
			.attr("height", relarrowwidth);

		newedgeg.append("i")
			.attr("id", function (d: SearchEdge) { return "searchrightarr_" + d.ID; })
			.attr("class", function (d: SearchEdge) {
				if (d.Direction === "<") { return "fas fa-minus"; }
				else { return "fas fa-arrow-right"; }
			})
			.classed("searchrightarrow", true)
			.classed("searcharrow", true)
			.attr("x", rectwidth)
			.attr("y", relarrowy)
			.attr("width", relarrowwidth)
			.attr("height", relarrowwidth);

		//console.log(this.SearchData.Edges);

		viewel.selectAll(".searchedge")
			.data(this.SearchData.Edges, function (d: SearchEdge) { return d.ID; })
			.exit()
			.remove();

		//init foundation for things like reveal
		newedgeg.each(function (d: SearchEdge) {
			//console.log("newedge: " + d.ID);
			$("#searchedge_" + d.ID).foundation();
		});

		var transform = function () {
			//console.log(d);
			currslot++;
			var subspacingx = (currslot + 0.5) * me.XSpacing - rectwidth / 2 - me.XSpacing * 0.5;
			var subspacingy = me.SvgContainerHeight /2 - rectheight / 2 + me.Margin;
			//console.log(currslot + ": " + subspacingx + ": " + subspacingy);
			//console.log("edge transforms");
			//console.log(d);
			return "translate(" + subspacingx + "," + subspacingy + ")";
		};

		if (animate) {
			viewel.selectAll(".searchedge")
				.data(this.SearchData.Edges, function (d: SearchEdge) { return d.ID; })
				.attr("class", function (d: SearchEdge) { return d.Label; })
				.classed("searchedge", true)
				.classed("clickable", true)
				.transition()
				.duration(500)
				.attr("transform", transform);
		}
		else {
			viewel.selectAll(".searchedge")
				.data(this.SearchData.Edges, function (d: SearchEdge) { return d.ID; })
				.attr("class", function (d: SearchEdge) { return d.Label; })
				.classed("searchedge", true)
				.classed("clickable", true)
				.attr("transform", transform);
		}
	}

	showNodeControls(searchnode: SearchNode) {
		//var nodetransform = callingEl.getAttribute("transform");
		var controlIconDimension = 20;
		var controlsx = 0;
		var controlsy = 5;
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
				.classed("nodecontrol clickable", true)
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
			.classed("nodecontrol clickable", true)
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
			.classed("nodecontrol clickable", true)
			.attr("transform", "translate(" + (controlsx + controlIconDimension * 2) + "," + controlsy + ")")
			.on("click", function () {
				d3.event.stopPropagation();
				me.deleteSearchNode(searchnode);
			})
			.append("i")
			.attr("height", controlIconDimension)
			.attr("width", controlIconDimension)
			.attr("class", "far fa-trash-alt");

		if (searchnode.index < this.SearchData.Nodes.length)
			controls.append("g")
				.classed("nodecontrol clickable", true)
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
		this.NodeDetailsModal.open();
	}

	showConditionControls(treenode: ViewTreeNode<ICondition>) {

	}

	onSearchNodeClicked(callingEl: SVGElement) {
		//console.log("onSearchNodeClicked started");
		//console.log(this);
		//update the datum on the callingEl
		var datum = this.UpdateItemDatum("searchNodeDetails", callingEl) as SearchNode;

		//console.log(datum);
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
		//$("#searchEdgeDetails").foundation("open");
		this.EdgeDetailsModal.open();
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

	onSearchNodeSaveAndCondClicked() {
		var node = this.onSearchNodeSaveBtnClicked();
		var cond: ICondition = GetCondition("STRING");

		this.NewCondition = new ViewTreeNode(cond, this.ViewTreeNodeConditionsProp, this.ConditionRoot);
		this.OpenSearchConditionDetails(this.NewCondition);
		(document.getElementById("searchItem") as HTMLSelectElement).value = node.Name;
		d3.select("#searchItem").dispatch('change');
	}

	onSearchPropChanged() {
		console.log("onSearchPropChanged started");
		var itemname = (document.getElementById("searchItem") as HTMLSelectElement).value;
		var property = (document.getElementById("searchProp") as HTMLSelectElement).value;

		var datum: any = GetNode(itemname, this.SearchData);

		console.log("label: " + itemname);
		console.log("property: " + property);
		console.log(datum);
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
		this.NodeDetailsModal.close();
		return node;
	}

	onSearchNodeDelBtnClicked (callingitem) {
		//console.log("onSearchNodeDelBtnClicked started: " + callingitem);
		//console.log(this);

		var nodedatum = d3.select("#searchNodeDetails").datum() as SearchNode;
		//console.log(nodedatum);
		if (this.deleteSearchNode(nodedatum)) {
			this.NodeDetailsModal.close();
		}
	}

	deleteSearchNode(searchnode: SearchNode): boolean {
		//console.log("deleteSearchNode started");
		//console.log(searchnode);
		//console.log(this.SearchData);
		if (confirm("Are you sure you want to delete " + searchnode.Name + "?")) {
			//console.log("deleting: " + searchnode.Name);
			RemoveNode(searchnode, this.SearchData);

			//console.log(this.SearchData);
			this.hideNodeControls();
			this.UpdateNodes(true);
			this.UpdateEdges(true);
			return true;
		}
		return false;
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
		//console.log("onSearchEdgeSaveBtnClicked started");
		//console.log(this);
		var me = this;

		var edge: SearchEdge = d3.select("#searchEdgeDetails").datum() as SearchEdge;
		var edgeEl = d3.select("#searchedge_" + edge.ID);

		//console.log("#searchedge_" + edge.ID);
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
			me.UpdateEdges(false);
		}, 10);

		this.EdgeDetailsModal.close();
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

	BindAdvAutoComplete() {
		var me = this;

		$('#searchVal').autocomplete({
			source: function (request, response) {
				//console.log("advAutoComplete");
				var itemname = (document.getElementById("searchItem") as HTMLSelectElement).value;
				var isnodedautm: boolean = true;
				var datum: any = GetNode(itemname, me.SearchData);
				
				if (datum === null) {
					datum = GetEdge(itemname, me.SearchData);
					isnodedautm = false;
				}

				if (datum !== null) {
					var prop = (document.getElementById("searchProp") as HTMLSelectElement).value;

					var url;

					if (isnodedautm === true) {
						url = "/api/graph/node/values?type=" + datum.Label + "&property=" + prop + "&searchterm=" + request.term;
					}
					else {
						url = "/api/graph/edge/values?type=" + datum.Label + "&property=" + prop + "&searchterm=" + request.term;
					}
					//console.log(url);
					//console.log("autoComplete: " + request.term);
					//console.log(datum);

					webcrap.data.apiGetJson(url, response);
				}
				else {
					console.error("Item could not be found: " + itemname);
				}
			}
		});
	}

	BindSimpleAutoComplete() {
		var me = this;

		$('#simpleSearchTerm').autocomplete({
			source: function (request, response) {
				var url = "/api/graph/node/namevalues?searchterm=" + request.term;
				//console.log(url);
				//console.log("autoComplete: " + request.term);
				//console.log(datum);

				webcrap.data.apiGetJson(url, response);
			}
		});
	}

	UpdateConditions () {
        //console.log("UpdateConditions start:" + this.ConditionElementID);
        //console.log(this.ConditionRoot);
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
		var gridwidth = 20;
		var bracewidth = gridwidth;
		var rectwidth = gridwidth * 6;
		var rectheight = 70;
		var xpadding = 5;
		var xspacing = gridwidth * 3;
		var ypadding = 5;
		var strokewidth = 3;
		
		//console.log('nodes:');
		//console.log(nodes);
		var enter = viewel.selectAll(".searchcondition")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
			.enter()
			.append("g")
			.attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchcondition_" + (d.data as ViewTreeNode<ICondition>).ID; })
			.attr("class", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				if (d.data.Item.Type == "OR" || d.data.Item.Type == "AND") {
					return "conditiongroup";
				}
				else {
					return "condition clickable";
				}
			})
			.classed("searchcondition", true)
			.on("click", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				if (d.data.Item.Type == "OR" || d.data.Item.Type == "AND")  {
					me.onSearchAndOrClicked(this);
				}
				else {
					me.onSearchConditionClicked(this);
				}
			});

		viewel.selectAll(".searchcondition")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
			.exit()
			.remove();

		var enterbg = enter.append("g")
			.classed("conditionbg", true);

		//add the background rect 
		enterbg.append("rect")
			.attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionbg_" + d.data.ID; })
			.classed("searchconditionrect", true)
			.attr("x", strokewidth)
			.attr("y", strokewidth)
			.attr("rx", 5)
			.attr("height", rectheight);


		viewel.selectAll(".searchconditionrect")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
			.attr("width", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				//console.log("width d: ");
				//console.log(d); 
				if (d.descendants().length > 1) { 
					var groupcount: number = d.descendants().length - d.value;
					var descendentcount: number = d.descendants().length;
					var leafcount: number = d.value;

					d.data.RectWidth = bracewidth * d.height * 2 + leafcount * rectwidth + (leafcount - 1) * xspacing + strokewidth * descendentcount * 2 + (me.PlusSize * groupcount); 
				}
				else {
					d.data.RectWidth = rectwidth;
				}
				return d.data.RectWidth;
			});

		var condtext = enter.filter(".searchcondition.condition")
			.append("text")
			.attr("y", "5px")
			.attr("text-anchor", "top")
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
			.text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				var s: string = (d.data.Item as ConditionBase).Name + "." + (d.data.Item as ConditionBase).Property;
				if (s.length < 20) { return s; }
				return s.substring(0, 18) + "...";
			});

		viewel.selectAll(".condition .searchconditionopval")
			.text(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				var s: string = (d.data.Item as ConditionBase).Operator + " " + (d.data.Item as ConditionBase).Value;
				if (s.length < 20) { return s;}
				return s.substring(0, 18)+ "...";
			});

		//setup the + button for group nodes e.g. AND/OR
		var condgroupenters = enter.filter(".conditiongroup");

		var groupaddbtns = condgroupenters.append("g")
			.attr("id", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return "searchconditionplus_" + d.data.ID; })
			.classed("searchconditionplus", true)
			.classed("searchcontrol", true)
			.classed("clickable",true)
			.on("click", function () {
				event.stopPropagation();
				me.onSearchConditionAddClicked(this as Element);
			});

		groupaddbtns.append("i")
			.attr("class", "fas fa-plus")
			.attr("width", me.PlusSize)
			.attr("height", me.PlusSize);


		groupaddbtns.append("rect")
			.attr("width", me.PlusSize)
			.attr("height", me.PlusSize);

		//transform translate all + button
		viewel.selectAll(".searchconditionplus")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
			.attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				var y = (rectheight / 2 - me.PlusSize / 2 + strokewidth);
				if (d.data.Children === null) {
					return "translate(" + (d.data.RectWidth / 2 - me.PlusSize / 2 + xpadding) + "," + y + ")";
				}
				else {
					return "translate(" + (d.data.RectWidth - me.PlusSize - bracewidth / 2) + "," + y + ")";
				}
				
			});

		//init foundation for things like reveal
		enter.each(function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
			$("searchcondition_" + (d.data as ViewTreeNode<ICondition>).ID).foundation();
		});

		//add the braces to the bg of the condition groups
		var condgroupentersbg = condgroupenters.selectAll(".conditionbg");

		condgroupentersbg.append("g")
			.classed("conditionbrace leftbrace", true)
			.append("text")
			.text("{")
			.attr("alignment-baseline", "hanging")
			.attr("font-size", rectheight + "px");

		condgroupentersbg.append("g")
			.classed("conditionbrace rightbrace", true)
			.append("text")
			.text("}")
			.attr("alignment-baseline", "hanging")
			.attr("font-size", rectheight + "px");

		var condgroups = viewel.selectAll(".conditiongroup")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; });

		condgroups.selectAll(".leftbrace")
			.attr("transform", function () {
				return "translate(0 " + ypadding + ")";
			});

		condgroups.selectAll(".rightbrace")
			.attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				//console.log(d);
				return "translate(" + (d.data.RectWidth - bracewidth + xpadding) + " " + ypadding + ")";
			});

		//remove the AND/OR labels and readd them
		condgroups.selectAll(".and-or-label").remove();

		condgroups.each(function (d: d3.HierarchyPointNode<ViewTreeNode<AndOrCondition>>, i: number) {
			//console.log(this);
			var treenode: ViewTreeNode<AndOrCondition> = d.data;
			//console.log(treenode);
			if (treenode.Children !== null && treenode.Children.length > 1) {
				var j;
				var labelx = bracewidth + treenode.Children[0].RectWidth;

				for (j = 1; j < treenode.Children.length; j++) {

					d3.select(this).append("text")
						.text(treenode.Item.Type)
						.classed("and-or-label", true)
						.classed("clickable", true)
						.attr("y", rectheight / 2)
						.attr("x", labelx + xspacing / 2 - xpadding);

					labelx = labelx + xspacing + xpadding + treenode.Children[j].RectWidth;
				}
			}
		});


		//finally move everything into place
		//console.log("translating conditions");
		viewel.selectAll(".searchcondition")
			.data(nodes, function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) { return d.data.ID; })
			.attr("transform", function (d: d3.HierarchyPointNode<ViewTreeNode<ICondition>>) {
				//console.log(d);
				var x = 0;
				var y = me.SvgContainerHeight / 2 - rectheight / 2 + me.Margin;
				if (d.parent !== null) {
					if (d.data.Index >= 1) {
						//console.log("Sub child");
						var prev: d3.HierarchyPointNode<ViewTreeNode<ICondition>> = d.parent.children[d.data.Index - 1];
						//console.log(prev);
						x = prev.x + prev.data.RectWidth + xspacing + xpadding;
					}
					else {
						//console.log("First child");
						x = d.parent.x + bracewidth + xpadding;
						
					}
				}
				d.x = x;
				d.y = y;
				return "translate(" + x + "," + (y + me.YSpacing / 2) + ")";
			});
	}



	onSearchConditionClicked(callingelement: HTMLElement) {
        //console.log("onSearchConditionClicked");
        var datum = this.GetElementDatum(callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;

		//console.log(datum);
		if (datum) {
			this.OpenSearchConditionDetails(datum.data);
		}
	}

	onSearchConditionCancelClicked() {
		this.NewCondition = null;
		this.EditingCondition = null;
		this.ConditionDetailsModal.close();
	}

	OpenSearchConditionDetails(treenode: ViewTreeNode<ICondition>) {
        //console.log("OpenSearchConditionDetails started");
		//console.log(treenode);
		var me = this;
	
		this.ClearAlert();
		this.EditingCondition = treenode;
		var condition: ConditionBase = treenode.Item as ConditionBase;
		//var datum = this.UpdateItemDatum("searchStringConditionDetails", callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
		//if (datum) { condition = datum.data.Item as ConditionBase; }
		//console.log(datum);

		(document.getElementById("searchProp") as HTMLSelectElement).disabled = false;
		(document.getElementById("searchVal") as HTMLInputElement).disabled = false;
		(document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = false;

		this.UpdateConditionDetails(condition, function () {
			me.ChangeSelectedValue(document.getElementById("searchItem"), condition.Name, function () {
				me.ChangeSelectedValue(document.getElementById("searchProp"), condition.Property, null);
				me.ChangeSelectedValue(document.getElementById("searchOperator"), condition.Operator, null);
			});
		});

		(document.getElementById("searchVal") as HTMLInputElement).value = condition.Value;
		if (condition.Type === "STRING") {
			(document.getElementById("searchCaseOptions") as HTMLDivElement).hidden = false;
			(document.getElementById("searchCase") as HTMLInputElement).checked = (condition as StringCondition).CaseSensitive;
			(document.getElementById("searchNot") as HTMLInputElement).checked = (condition as StringCondition).Not;
		}
		else {
			(document.getElementById("searchCaseOptions") as HTMLDivElement).hidden = true;
		}

        if (this.EditingCondition === this.NewCondition) {
			d3.select("#searchConditionDeleteBtn").classed("hidden", true);
			d3.select("#searchConditionCancelBtn").classed("hidden", false);
			//(document.getElementById("searchConditionDeleteBtn") as HTMLButtonElement).hidden = true;
			//(document.getElementById("searchConditionCancelBtn") as HTMLButtonElement).hidden = false;
		}
		else {
			d3.select("#searchConditionDeleteBtn").classed("hidden", false);
			d3.select("#searchConditionCancelBtn").classed("hidden", true);
		}

		this.ConditionDetailsModal.open();
	}

	UpdateConditionDetails(condition: ICondition, callback) {
		//console.log("UpdateConditionDetails started");
		//console.log(this.SearchData.Nodes);

		//*** setup the searchItem list
		var searchItem = document.getElementById("searchItem");
		webcrap.dom.ClearOptions(searchItem);

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
            option = webcrap.dom.AddOption(searchItem, item.Name, item.Name);
            //if ((item.Max === "1" && item.Min === "1") === false) { option.setAttribute("disabled", ""); }
            //if (webcrap.misc.isNullOrWhitespace(item.Label)) { option.setAttribute("disabled", ""); }
		});

		//*** setup the searchOperator list
		var searchOperator = document.getElementById("searchOperator");
		webcrap.dom.ClearOptions(searchOperator);

		//var cond: ICondition = datum.data.Item;
		//console.log(ConditionOperators[condition.Type]);
		ConditionOperators[condition.Type].forEach(function (operator) {
			webcrap.dom.AddOption(searchOperator, operator, operator);
		});


		if (typeof callback === "function") {
			callback();
			//setTimeout(callback, 5);
		}
	}

	onSearchConditionItemChanged () {
		//console.log("onSearchConditionItemChanged started");
		//console.log(this);

		//var me = this;
		var searchItem: HTMLSelectElement = document.getElementById("searchItem") as HTMLSelectElement;
		var searchProps = document.getElementById("searchProp");
		var selectedName = searchItem.options[searchItem.selectedIndex].value;
		var selectedItem;
		//var datum = (this.GetItemDatum("searchStringConditionDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
		
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
				//console.log("node label not selected");
				(document.getElementById("searchProp") as HTMLSelectElement).disabled = true;
				(document.getElementById("searchVal") as HTMLInputElement).disabled = true;
				(document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = true;
				this.SetAlert("The item you have selected (" + selectedItem.Name + ") does not have a type. You must set the type on the item before you can create a condition");
            }
            else if (typeSelected === "edge" && !(selectedItem.Min === "1" && selectedItem.Max === "1")) {
                (document.getElementById("searchProp") as HTMLSelectElement).disabled = true;
                (document.getElementById("searchVal") as HTMLInputElement).disabled = true;
                (document.getElementById("searchConditionSaveBtn") as HTMLInputElement).disabled = true;
                this.SetAlert("The item you have selected (" + selectedItem.Name + ") contains a multi-hop path. This is currently not supported. Set min and max to 1 on this item");
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
	}

	onSearchConditionAddClicked(callingelement: Element) {
		//console.log("onSearchConditionAddClicked started: " + isroot);
		//var datum;
		var tempnode: ViewTreeNode<ICondition>; 
		var addingparent = (this.GetElementDatum(callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>).data;
		
		//console.log(addingparent);
		tempnode = new ViewTreeNode(null, this.ViewTreeNodeConditionsProp, addingparent);
		//console.log('tempnode:');
		//console.log(tempnode);
		this.NewCondition = tempnode;
		this.ConditionTypeModal.open();

		//change the select list back to the default (first) value
		var typelistel = (document.getElementById("searchConditionTypeList") as HTMLSelectElement);
		var firstval = (typelistel[0] as HTMLOptionElement).value;
		typelistel.value = firstval;
		d3.select(firstval).dispatch("change");

	}

	onSearchConditionTypeSaveClicked() {
		//console.log("onSearchConditionTypeSaveClicked started");
		var selectedType: string = (document.getElementById("searchConditionTypeList") as HTMLSelectElement).value;
		
		var cond: ICondition = GetCondition(selectedType);
		this.NewCondition.Item = cond;
		if (selectedType === "AND" || selectedType === "OR") {
			this.ApplyNewCondition();
			this.UpdateConditions();
		}
		else {
			this.OpenSearchConditionDetails(this.NewCondition);
		}

		this.ConditionTypeModal.close();
		
	}

	ApplyNewCondition() {
		this.InsertTreeNode(this.NewCondition);
		this.NewCondition = null;
	}

	//Properly add a new treenode in the conditions tree e.g. from this.AddingTemp
	InsertTreeNode(treenode: ViewTreeNode<ICondition>) {
		if (treenode.Parent !== null) {
			treenode.Parent.AddChild(treenode);
			this.ConditionRoot.Rebuild();
		}
		else {
			this.ConditionRoot = treenode;
			this.SearchData.Condition = treenode.Item;
			this.ConditionRoot.Build();
		}
	}

	ResetRootTreeNode() {
		//console.log("ResetRootTreeNode started");
		this.ConditionRoot = null;
		this.SearchData.Condition = undefined;

		var cond: ICondition = GetCondition("AND");
		var treenode = new ViewTreeNode<ICondition>(cond, this.ViewTreeNodeConditionsProp, null);
		this.InsertTreeNode(treenode);
	}

	onSearchConditionDeleteClicked() {
		if (confirm('Are you sure you want to delete this condition?')) {
			this.DeleteSearchCondition();
			this.ConditionDetailsModal.close();
		}
	}

	onSearchAndOrDeleteClicked() {
		if (confirm('Are you sure you want to delete this group and all conditions inside?')) {
			this.DeleteSearchCondition();
			this.SearchAndOrDetailsModal.close();
		}
		
	}

	DeleteSearchCondition () {
		//console.log("deleteSearchCondition started");
		//searchStringConditionDetails

		//datum is the d3 tree datum. Use datum.data to get the ViewTreeNode datum
		//var datum = this.GetItemDatum(elementid) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
		var treenode = this.EditingCondition;

        //console.log(treenode);
		//console.log("RootID: " + this.ConditionRoot.ID);
		if (this.ConditionRoot.ID === treenode.ID) {
			//delete the conditions and re-add an AND condition 
			this.ResetRootTreeNode();
			//d3.selectAll(".searchcondition").remove();
		}
		else {
			treenode.Parent.RemoveChild(treenode);
			treenode.Parent.Rebuild();
			//this.UpdateConditions();
		}
		this.EditingCondition = null;
		this.UpdateConditions();
		//console.log(this.ConditionRoot);
		//console.log("onSearchConditionDeleteClicked finished");
	}


	onSearchConditionSaveClicked () {
		//console.log("onSearchConditionSaveClicked started");
		//var datum = this.GetItemDatum("searchStringConditionDetails") as d3.HierarchyNode<ViewTreeNode<ICondition>>;
		//var condition = datum.data.Item as ConditionBase;
		var newname = (document.getElementById("searchItem") as HTMLSelectElement).value;
		var newprop = (document.getElementById("searchProp") as HTMLSelectElement).value;
		var newval = (document.getElementById("searchVal") as HTMLInputElement).value;
		var newop = (document.getElementById("searchOperator") as HTMLInputElement).value;

		if (webcrap.misc.isNullOrWhitespace(newname) || webcrap.misc.isNullOrWhitespace(newprop) || webcrap.misc.isNullOrWhitespace(newval)) {
			this.SetAlert("Name, property, or value is empty. Please set a value");
		}
		else {
			var condition = this.EditingCondition.Item as ConditionBase;
			if (condition.Type === "STRING") {
				//console.log("saving case state");
				(condition as StringCondition).CaseSensitive = (document.getElementById("searchCase") as HTMLInputElement).checked;
			}

			condition.Not = (document.getElementById("searchNot") as HTMLInputElement).checked;
			condition.Name = newname;
			condition.Property = newprop;
			condition.Value = newval;
			condition.Operator = newop;

			//console.log(condition);

			if (this.NewCondition !== null) {
				this.ApplyNewCondition();
			}
			this.ConditionDetailsModal.close();
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
		this.SearchAndOrDetailsModal.close();
		this.UpdateConditions();
	}

	onSearchAndOrClicked (callingelement) {
		//console.log("onSearchAndOrClicked started");
        this.ClearAlert();

        var datum = this.GetElementDatum(callingelement) as d3.HierarchyNode<ViewTreeNode<ICondition>>;
        this.EditingCondition = datum.data;
		//console.log(datum);
		var cond: AndOrCondition = datum.data.Item as AndOrCondition;

		(document.getElementById("searchAndOr") as HTMLInputElement).value = cond.Type;
		this.SearchAndOrDetailsModal.open();
	};

	ClearAlert () {
		var alertEl = $("#alertIcon");
		alertEl.hide();
	}

	SetAlert (message: string) {
		//console.log("SetAlert started: " + message);
		//console.log(this.Tooltip);
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
