import '../css/main.css';
import '../css/visualization.css';


import DataQueue from '../ts/DataQueue';
import Slope from '../ts/Slope';
import SimulationController from '../ts/SimulationController';
import Mappings from '../ts/Mappings';
import DatumStore from '../ts/DatumStore';

import $ from 'jquery';

import * as d3 from "./visualizerD3";
import { webcrap } from '../../shared/webcrap/webcrap';

var drawingPane;
var zoomLayer;
var drawingsvg;
var graphbglayer;
var nodeslayer;
var edgeslayer;

var queue = new DataQueue(addSearchResults);
var simController = new SimulationController();
simController.ProgressBarTag = "#progress";
simController.TreeEdgeTag = ".treeedge";
simController.NodeTag = ".nodes";
simController.onPercentUpdated = updateProgressBar;
simController.onFinishSimulation = onLayoutFinished;


//for recording the labels in teh graph and whether they are enabled e.g. AD_User,false
var graphnodelabels = new Object();
var graphedgelabels = new Object();

//var meshsimulation;
var meshnodes = new DatumStore();
var meshedges = new DatumStore();

//var treesimulation;
var treenodes = new DatumStore();
var treeedges = new DatumStore();

//var connectsimulation;
var connectnodes = new DatumStore();
var connectedges = new DatumStore();

//var graphsimulation;
var graphnodes = new DatumStore();
var graphedges = new DatumStore();

var areaBox;
var force;

var playMode = false;
var shiftKey = false;

var defaultsize = 40;
var edgelabelwidth = 70;

var minScaling = 1;
var maxScaling = 3;

var perfmode = false; //perfmode indicates high numbers of nodes, and minimises animation
var maxAnimateNodes = 300;

var zoomer = d3.zoom()
    .scaleExtent([0.05, 5])
    .on("zoom", onZoom);


//IconMappings object to asign the correct font awesome icon to the correct nodes
var iconmappings;
webcrap.data.apiGetJson("/visualizer/icons", function (data) {
    //console.log(data);
    iconmappings = new Mappings(data, "fas fa-question");
    //console.log(iconmappings);
});

//SubTypeMappings object to asign the correct font awesome icon to the correct nodes
var subtypeproperties;
webcrap.data.apiGetJson("/visualizer/subtypeproperties", function (data) {
    //console.log(data);
    subtypeproperties = new Mappings(data, "");
    //console.log(subtypeproperties);
});

export function drawGraph(selectid, loaddata) {
    //console.log('drawGraph started');
    //updateNodeLabels("target");
    drawingPane = d3.select("#" + selectid);

    drawingsvg = drawingPane.append("svg")
        .attr("id", "drawingsvg");
    resetDrawingEvents();

    //setup the zooming layer
    zoomLayer = drawingsvg.append("g")
        .attr("id", "zoomlayer");

    graphbglayer = zoomLayer.append("g")
        .attr("id", "graphbglayer");

    edgeslayer = zoomLayer.append("g")
        .attr("id", "edgeslayer");

    nodeslayer = zoomLayer.append("g")
        .attr("id", "nodeslayer");

    //*center dot for testing
    //nodeslayer.append("circle")
    //    .classed("nodecircle", true)
    //    .attr("id","centerdot")
    //    .attr("r", 5)
    //    .attr("cx", 0)
    //    .attr("cy", 0)
    //    .style("fill","black");

    window.addEventListener('resize', debounce(updatePaneSize, 500, false), false);
    updatePaneSize();
    if (loaddata !== undefined && loaddata.length > 0) {
        //console.log(loaddata);
        getNodes(loaddata);
        //setTimeout(function () {
        //    menuShowHide();
        //}, 100);
    }

    //look for a stored resultset. if found, load it and clear it
    //console.log('loading stored result set')
    var results = JSON.parse(sessionStorage.getItem("birdsnest_resultset"));
    sessionStorage.removeItem("birdsnest_resultset");
    //console.log(results);
    if (results !== null) {
        queue.QueueResults(results);
        //setTimeout(function () {
        //    menuShowHide();
        //}, 100);
    }
}

//https://stackoverflow.com/questions/641857/javascript-window-resize-event
//function to stop excessive calls on resize
const debounce = function (func, wait, immediate) {
    var timeout;
    return function () {
        const context = this, args = arguments;
        const later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};

function resetDrawingEvents() {
    drawingsvg
        .on('click', pageClicked)
        .on('mousedown', null)
        .on('touchstart', null)
        .on('touchend', null)
        .on('mouseup', null)
        .call(zoomer);
}

export function resetView() {
    //console.log("resetView");

    if (confirm("Are you sure you want to clear the display?") === true) {
        meshnodes = new DatumStore();
        meshedges = new DatumStore();
        treenodes = new DatumStore();
        treeedges = new DatumStore();
        graphnodes = new DatumStore();
        graphedges = new DatumStore();
        connectedges = new DatumStore();
        graphnodelabels = new Object();
        graphedgelabels = new Object();

        unselectAllNodes();
        graphbglayer.selectAll("*").remove();
        edgeslayer.selectAll("*").remove();
        nodeslayer.selectAll("*").remove();
        resetScale();
        cleanAndUpdateLabelEyes();
    }
}


export function updateProgressBar(percent) {
    d3.select("#progress").style("width", percent + "%");
}

d3.selectAll("#restartLayoutBtn").attr('onclick', 'vis.restartLayout()');
export function restartLayout() {
    //console.log('restartLayout');
    d3.selectAll("#restartIcon").classed("spinner", true);
    d3.selectAll("#restartLayoutBtn")
        .attr('onclick', 'vis.pauseLayout()')
        .attr('title', 'Updating layout');

    simController.RestartSimulation();
}

d3.selectAll("#pausePlayBtn").attr('onclick', "vis.playLayout()");
export function playLayout() {
    playMode = true;

    d3.selectAll("#pausePlayIcon")
        .classed("fa-play", false)
        .classed("fa-pause", true);
    d3.selectAll("#pausePlayBtn")
        .attr('onclick', "vis.pauseLayout()");
}

export function pauseLayout() {
    playMode = false;
    simController.StopSimulations();

    d3.selectAll("#pausePlayIcon")
        .classed("fa-pause", false)
        .classed("fa-play", true);
    d3.selectAll("#pausePlayBtn")
        .attr('onclick', 'vis.playLayout()');
    d3.selectAll("#restartIcon").classed("spinner", false);
    d3.selectAll("#restartLayoutBtn").attr('onclick', 'vis.restartLayout()');
}

function cleanAndUpdateLabelEyes() {
    cleanLabels(graphnodelabels);
    cleanLabels(graphedgelabels);
    function cleanLabels(obj) {
        Object.keys(obj).forEach(function (d) {
            //console.log("checking label: " + d);
            if (d3.select("." + d).size() === 0) {
                //console.log("removing eye: " + d);
                delete obj[d];
            }
        });
    }
    updateLabelEyes();
}

function updateLabelEyes() {
    buildEyeTable(graphnodelabels, "eyeNodeLabelList", "nodes");
    buildEyeTable(graphedgelabels, "eyeEdgeLabelList", "edges");
    function buildEyeTable(obj, id, classtype) {
        let arrList = Object.keys(obj).sort();
        let rootEl = d3.select("#" + id).html("");
        rootEl.attr("classtype", classtype);

        let htlabel = rootEl.append("li")
            .append("div")
            .classed("cell small-12", true);

        htlabel.append("a")
            .attr("href", "javascript:vis.onEyeShowAllClicked(\"" + id + "\");")
            .html("Show all");
        htlabel.append("span")
            .html(" | ");
        htlabel.append("a")
            .attr("href", "javascript:vis.onEyeHideAllClicked(\"" + id + "\");")
            .html("Hide all");
        htlabel.append("span")
            .html(" | ");
        htlabel.append("a")
            .attr("href", "javascript:vis.onEyeInvert(\"" + id + "\");")
            .html("Invert");

        arrList.forEach(function (d) {
            htlabel = rootEl.append("li")
                .classed("eyeListItem", true)
                .attr("label", d)
                .append("a")
                .attr("href", "javascript:vis.onEyeLabelClicked(\"" + d + "\");");

            let htlabeltable = htlabel.append("div").classed("grid-x align-middle", true);

            htlabeltable.append("div")
                .attr("id", "eyeIcon_" + d)
                .classed("cell far fa-eye small-2", true);

            htlabeltable.append("div")
                .attr("id", "eyeLabel_" + d)
                .classed("cell small-10", true)
                .html(d);
        });
    }
}

export function onReportClicked(sametab) {
    //console.log("onReportClicked started");

    if (typeof Storage !== "undefined") {
        try {
            var exportnodes = graphnodes.GetArray();
            if (exportnodes.length !== 0) {
                var tempArr = [];
                exportnodes.forEach(function (d) {
                    tempArr.push("id=" + d.db_id);
                });

                tempArr.push("property=name");
                tempArr.push("property=id");

                var querystring = tempArr.join("&");
                //console.log(querystring);
                if (sametab) {
                    window.location.href = "/reports/nodesquery?" + querystring;
                }
                else {
                    window.open("/reports/nodesquery?" + querystring, '_blank');
                }

            }
            else {
                console.log("Nothing to export");
            }

        }
        catch (err) {
            updateStatus("There was an error opening report view. Your result set may be too large");
        }

    } else {
        console.log("No Web Storage support..");
        // Sorry! No Web Storage support..
    }
}

export function onEyeShowAllClicked(element) {
    //console.log("onEyeShowAllClicked: " + element);
    d3.selectAll("#" + element + " .eyeListItem")
        .each(function (d) {
            //console.log(this);
            //console.log(d);
            let label = d3.select(this).attr("label");
            eyeShowHideLabel(label, true);
        });
}

export function onEyeHideAllClicked(element) {
    //console.log("onEyeHideAllClicked: " + element);
    d3.selectAll("#" + element + " .eyeListItem")
        .each(function () {
            //console.log(this);
            //console.log(d);
            let label = d3.select(this).attr("label");
            eyeShowHideLabel(label, false);
        });
}

export function onEyeInvert(element) {
    let el = d3.selectAll("#" + element);
    let type = el.attr("classtype");
    let enabled = d3.selectAll(".enabled." + type);
    let disabled = d3.selectAll(".disabled." + type);
    disabled.classed("disabled", false).classed("enabled", true);
    enabled.classed("enabled", false).classed("disabled", true);

}

export function onEyeNodeDetailClicked(dbid) {
    //console.log("onEyeNodeDetailClicked: " + dbid);
    let show = !d3.selectAll("#node_" + dbid).classed("enabled");

    d3.select("#eyeIcon_" + dbid)
        .classed("fa-eye", show)
        .classed("fa-eye-slash", !show);

    d3.select("#node_" + dbid)
        .classed("enabled", show)
        .classed("disabled", !show);
}

//<a href="javascript:getNode(@node.DbId);">(+)</a><br />
export function onEyeLabelClicked(label) {
    //console.log("onEyeLabelClicked: " + label);
    let currenabled;
    if (graphnodelabels[label] !== undefined) {
        currenabled = graphnodelabels[label];
        //graphnodelabels[label] = !currenabled;
    }
    else {
        currenabled = graphedgelabels[label];
        //graphedgelabels[label] = !currenabled;
    }
    eyeShowHideLabel(label, !currenabled);
}

function eyeShowHideLabel(label, show) {
    if (graphnodelabels[label] !== undefined) {
        graphnodelabels[label] = show;
    }
    else {
        graphedgelabels[label] = show;
    }

    d3.selectAll("." + label)
        .classed("disabled", !show)
        .classed("enabled", show);

    d3.selectAll("#eyeIcon_" + label)
        .classed("fa-eye", show)
        .classed("fa-eye-slash", !show);
}

function onTargetAddClicked(nodeid) {
    //updateSearchDetails(nodeid, 'target');
}

function onSourceAddClicked(nodeid) {
    //updateSearchDetails(nodeid, 'source');
}

//export function updateSearchDetails(nodeid, elprefix) {
//    //console.log("onSourceAddClicked: " + nodeid);
//    var node = graphnodes.GetDatum(nodeid);
//    //console.log(node);
//    d3.select("#" + elprefix + "Type").property('value', node.labels[0]);
//    updateNodeProps();
//    var propname = 'name';
//    var propval = node.name;

//    if (node.properties.hasOwnProperty('id')) {
//        propname = 'id';
//        propval = node.properties.id;
//    } else if (node.properties.hasOwnProperty('path')) {
//        propname = 'path';
//        propval = node.properties.path;
//    }

//    d3.select("#" + elprefix + "Prop").property('value', propname);
//    d3.select("#" + elprefix + "Val").property('value', propval);
//}

function onLayoutFinished() {
    //console.log("onLayoutFinished");
    //simRunning = false;
    if (perfmode === false) { updateAllNodeLocations(true); }
    else { updateAllNodeLocations(false); }
    d3.selectAll("#restartIcon").classed("spinner", false);
    d3.select("#progress").style("width", "100%");
    d3.selectAll("#restartLayoutBtn")
        .attr('onclick', 'vis.restartLayout()')
        .attr('title', 'Refresh layout');
}



/*
*****************************
setup edges and nodes
*****************************
*/

function addSearchResults(results, callback) {
    //console.log(pendingResults.nodes.length);
    //console.log("addSearchResults start: " );
    //console.log(results);

    if (results.Nodes.length > maxAnimateNodes) {
        if (confirm("You are adding " + results.Nodes.length + " nodes to the view. This is a " +
            "large number of nodes. Layout animation will be disabled for performance reasons. " +
            " Are you sure?") !== true) {
            return;
        }
    }

    d3.selectAll("#restartIcon").classed("spinner", true);
    d3.selectAll("#restartLayoutBtn")
        .attr('title', 'Updating data');

    let newcount = addResultSet(results);
    if (newcount > 0) {
        //let nodeids = getAllNodeIds();
        let nodeids = graphnodes.GetIDs();
        //console.log("nodeids");
        //console.log(nodeids);
        getEdgesForNodes(nodeids, function (data) {
            //console.log("getEdgesForNodes complete");
            //console.log(data);

            addResultSet(data);
            addSvgNodes(graphnodes.GetArray());
            addSvgEdges(graphedges.GetArray());
            restartLayout();
            if (callback !== undefined) {
                //console.log(callback);
                callback();
            }
        });
    }
    else {
        d3.selectAll("#restartIcon").classed("spinner", false);
        if (callback !== undefined) {
            //console.log(callback);
            callback();
        }
    }

    //console.log("addSearchResults end");

}

function addResultSet(json) {
    //console.log("addResultSet start: " + json);
    let edges = json.Edges;
    let nodes = json.Nodes;

    //console.log(nodes);
    //populate necessary additional data for the view
    let newitemcount = loadNodeData(nodes);
    checkPerfMode();

    edges.forEach(function (d) {
        if (graphedges.DatumExists(d) === false) {
            if (graphedgelabels[d.label] === undefined) {
                graphedgelabels[d.label] = true;
            }//record the label is in the graph
            graphedges.Add(d);
            let src = graphnodes.GetDatum(d.source);
            let tar = graphnodes.GetDatum(d.target);
            //console.log(d);
            //console.log(src);

            if (src.properties.layout === "tree" && tar.properties.layout === "tree") { treeedges.Add(d); }
            else if (src.properties.layout === "mesh" && tar.properties.layout === "mesh") { meshedges.Add(d); }
            else {
                if (connectnodes.DatumExists(d.source) === false) { connectnodes.Add(graphnodes.GetDatum(d.source)); }
                if (connectnodes.DatumExists(d.target) === false) { connectnodes.Add(graphnodes.GetDatum(d.target)); }
                connectedges.Add(d);
            }

            newitemcount++;
        }
    });

    if (newitemcount > 0) {
        simController.SetNodes(graphnodes.GetArray(), meshnodes.GetArray(), treenodes.GetArray(), connectnodes.GetArray());
        simController.SetEdges(meshedges.GetArray(), treeedges.GetArray(), connectedges.GetArray());
    }
    updateLabelEyes();

    return newitemcount;
    //console.log("addResultSet end: " );
}

export function onAddToView() {
    let results = $('#searchResultData').val();
    //console.log(results);
    addSearchResults(JSON.parse(results));
	document.getElementById("searchNotification").innerHTML = '';
	document.getElementById("simpleSearchNotification").innerHTML = '';
}



var currMaxScope = 20;
var currMinScope = 1;
function resetScale() {
    currMaxScope = 20;
    graphnodes.GetArray().forEach(function (d) {
        if (d.properties.scope > currMaxScope) {
            currMaxScope = d.properties.scope;
        }
    });

    let scalingRange = new Slope(currMinScope, minScaling, currMaxScope, maxScaling);

    graphnodes.GetArray().forEach(function (d) {
        //console.log(d);
        d.scaling = scalingRange.getYFromX(d.scope);
        d.radius = defaultsize * d.scaling / 2;
        d.cx = d.x + d.radius;
        d.cy = d.y + d.radius;
        d.size = defaultsize * d.scaling;
    });

    updateNodeSizes();
}

function loadNodeData(newnodedata) {
    //console.log('loadNodeData');
    //console.log(newnodedata);
    let rangeUpdated = false;
    let newcount = 0;
    let newtograph = [];

    //evaluate the nodes to figure out the max and min size so we can work out the scaling
    newnodedata.forEach(function (d) {
        if (graphnodes.DatumExists(d) === false) {
            newtograph.push(d);
            d.x = 0;
            d.y = 0;
            d.labels.forEach(function (label) {
                if (graphnodelabels[label] === undefined) {
                    graphnodelabels[label] = true;
                }//record the label is in the graph
            });

            if (d.properties.scope > currMaxScope) {
                rangeUpdated = true;
                currMaxScope = d.properties.scope;
            }

            d.icon = iconmappings.getMappingFromArray(d.labels) + " nodeicon";
            let labelclasses = "";
            d.labels.forEach(function (label) {
                //console.log(label);
                labelclasses += " " + label;
                var subtype = subtypeproperties.getMappingFromValue(label);
                //console.log(subtype);
                if (subtype !== "") { labelclasses += " " + label + "-" + d.properties[subtype]; }
            });
            //console.log(labelclasses);
            d.classes = labelclasses;
        }
    });

    let scalingRange = new Slope(currMinScope, minScaling, currMaxScope, maxScaling);

    if (rangeUpdated) {
        //update the scaling range and update all existing nodes
        graphnodes.GetArray().forEach(function (d) {
            //console.log(d);
            d.scaling = scalingRange.getYFromX(d.scope);
            d.radius = defaultsize * d.scaling / 2;
            d.cx = d.x + d.radius;
            d.cy = d.y + d.radius;
            d.size = defaultsize * d.scaling;
        });

        updateNodeSizes();
    }

    //load the data and pre-calculate/set the values for each new node 	
    newtograph.forEach(function (d) {
        //console.log("New node: " + d.db_id);
        d.scaling = scalingRange.getYFromX(d.scope);

        d.radius = defaultsize * d.scaling / 2;
        d.cx = d.x + d.radius;
        d.cy = d.y + d.radius;
        //console.log(d.x); 
        //console.log(d.cx); 
        d.size = defaultsize * d.scaling;
        d.selected = false;
        //populateDetails(d);
        graphnodes.Add(d);
        if (d.properties.layout === "mesh") { meshnodes.Add(d); }
        else if (d.properties.layout === "tree") { treenodes.Add(d); }
        newcount++;
    });
    //console.log("graphnodes");
    //console.log(graphnodes);
    //console.log("meshnodes");
    //console.log(meshnodes);
    //console.log("treenodes");
    //console.log(treenodes);

    return newcount;
}

function addSvgNodes(nodes) {
    //console.log(": addSvgNodes");
    //add the bg
    setTimeout(function () {
        graphbglayer.selectAll('.nodebg')
            .data(nodes, function (d) { return d.db_id; })
            .enter()
            .append("circle")
            .attr("r", function (d) { return d.radius + 10; })
            .attr("cx", function (d) { return d.radius; })
            .attr("cy", function (d) { return d.radius; })
            .attr("id", function (d) { return "nodebg_" + d.db_id; })
            .classed("graphbg", true)
            .classed("nodebg", true);
    }, 1);

    let enternodes = nodeslayer.selectAll(".nodes")
        .data(nodes, function (d) { return d.db_id; })
        .enter();

    let enternodesg = enternodes.append("g")
        .attr("id", function (d) { return "node_" + d.db_id; })
        .attr("class", function (d) { return d.classes; })
        .attr("cursor", "pointer")
        .classed("nodes", true)
        .classed("enabled", true)
        .classed("selected", function (d) { return d.selected; })
        .on("click", onNodeClicked)
        .on("mouseover", onNodeMouseOver)
        .on("mouseout", onNodeMouseOut)
        .on("dblclick", onNodeDblClicked)
        .call(
            d3.drag()
                .on('start', onNodeDragStart)
                .on('drag', onNodeDragged)
                .on('end', onNodeDragFinished));

    setTimeout(function () {
        //node layout
        enternodesg.append("circle")
            .attr("id", function (d) { return "node_" + d.db_id + "_icon"; })
            .classed("nodecircle", true)
            .attr("r", function (d) { return d.radius; })
            .attr("cx", function (d) { return d.radius; })
            .attr("cy", function (d) { return d.radius; });
    }, 1);

    setTimeout(function () {
        enternodesg.append("i")
            .attr("height", function (d) { return d.size * 0.6; })
            .attr("width", function (d) { return d.size * 0.6; })
            .attr("x", function (d) { return d.size * 0.2; })
            .attr("y", function (d) { return d.size * 0.2; })
            .attr("class", function (d) { return d.icon; });
    }, 1);

    setTimeout(function () {
        enternodesg.append("text")
            .text(function (d) { return d.name; })
            .attr("text-anchor", "middle")
            .attr("dominant-baseline", "central")
            .attr("transform", function (d) { return "translate(" + (d.size / 2) + "," + (d.size + 10) + ")"; });
    }, 1);
}

function updateNodeSizes() {
    //console.log("updateNodeSizes");
    //add the bg
    graphbglayer.selectAll('.nodebg')
        .data(graphnodes.GetArray(), function (d) { return d.db_id; })
        .attr("r", function (d) { return d.radius + 10; })
        .attr("cx", function (d) { return d.radius; })
        .attr("cy", function (d) { return d.radius; });

    //build the nodes
    let allnodes = nodeslayer.selectAll(".nodes")
        .data(graphnodes.GetArray(), function (d) { return d.db_id; });

    //node layout
    allnodes.selectAll("circle")
        .attr("r", function (d) { return d.radius; })
        .attr("cx", function (d) { return d.radius; })
        .attr("cy", function (d) { return d.radius; });

    //remove all the icons so they can be regenerated the correct size
    nodeslayer.selectAll(".nodeicon").remove();
    allnodes.append("i")
        .attr("height", function (d) { return d.size * 0.6; })
        .attr("width", function (d) { return d.size * 0.6; })
        .attr("x", function (d) { return d.size * 0.2; })
        .attr("y", function (d) { return d.size * 0.2; })
        .attr("class", function (d) { return d.icon; });

    allnodes.selectAll("text")
        .attr("transform", function (d) { return "translate(" + (d.size / 2) + "," + (d.size + 10) + ")"; });

    let pins = allnodes.selectAll(".pin");
    pins.selectAll("circle")
        .attr("r", function (d) { return 8 * d.scaling; })
        .attr("cx", function (d) { return 5 * d.scaling; })
        .attr("cy", function (d) { return 5 * d.scaling; });

    //remove all the icons so they can be regenerated the correct size
    nodeslayer.selectAll(".pinicon").remove();
    pins.append("i")
        .classed("fas fa-thumbtack", true)
        .attr("x", 0)
        .attr("y", 1)
        .attr("height", function (d) { return 10 * d.scaling; })
        .attr("width", function (d) { return 10 * d.scaling; });
}

function addSvgEdges(edges) {
    //setup the edges
    //console.log("addSvgEdges");
    //   console.log(edges);

    //add the bg
    setTimeout(function () {
        graphbglayer.selectAll('.edgebg')
            .data(edges, function (d) { return d.db_id; })
            .enter()
            .append("path")
            .attr("id", function (d) { return "edgebg_" + d.db_id; })
            .classed("graphbg", true)
            .classed("edgebg", true);
    }, 1);

    let enteredges = edgeslayer.selectAll(".edges")
        .data(edges, function (d) { return d.db_id; })
        .enter();

    let enteredgesg = enteredges.append("g")
        .attr("id", function (d) { return "edge_" + d.db_id; })
        .attr("class", function (d) {
            var combined;
            if (d.source.properties.hasOwnProperty('layout') && d.target.properties.hasOwnProperty('layout')) {
                if (d.source.properties.layout === "tree" && d.target.properties.layout === "tree") {
                    combined = d.label + " treeedge";
                }
                else if (d.source.properties.layout === "mesh" && d.target.properties.layout === "mesh") {
                    combined = d.label + " meshedge";
                }
                else { combined = d.label + " connectingedge"; }
            }
            else { combined = d.label + " connectingedge"; }
            //console.log(combined);
            return combined;
        })
        .classed("edges", true)
        .classed("enabled", true);

    setTimeout(function () {
        enteredgesg.append("path")
            .classed("wrapper", true)
            .attr("fill", "none");
    }, 0);

    setTimeout(function () {
        enteredgesg.append("path")
            .classed("arrows", true);
    }, 0);

    setTimeout(function () {
        let edgelabels = enteredgesg.append("g")
            .classed("edgelabel", true);

        edgelabels.append("text")
            .text(function (d) { return d.label; })
            .attr("dominant-baseline", "text-bottom")
            .attr("text-anchor", "middle")
            .attr("transform", "translate(0,-5)");
    }, 1);
}

document.getElementById('removeBtn').addEventListener('click', removeNodes, false);
function removeNodes() {
    //console.log("removeNodes: " );
    var nodeList = [];
    var nodeids = [];

    d3.selectAll(".selected")
        .each(function (d) {
            nodeList.push(d);
        });

    if (nodeList.length === 0) { return; }

    if (confirm("This will remove " + nodeList.length + " nodes. Are you sure?") !== true) {
        return;
    }

    stopSelect(); //disable select function if used
    nodeList.forEach(function (d) {
        updateNodeSelection(d, false, false);
        nodeids.push(d.db_id);
        graphnodes.Remove(d);
    });

    getDirectEdgesForNodeList(nodeids, function (data) {
        //console.log(data);
        //console.log(nodeids);
        data.Edges.forEach(function (d) { graphedges.Remove(d); });
        checkPerfMode();

        graphbglayer.selectAll('.edgebg')
            .data(graphedges.GetArray(), function (d) { return d.db_id; })
            .exit().remove();

        graphbglayer.selectAll('.nodebg')
            .data(graphnodes.GetArray(), function (d) { return d.db_id; })
            .exit().remove();

        edgeslayer.selectAll(".edges")
            .data(graphedges.GetArray(), function (d) { return d.db_id; })
            .exit().remove();

        nodeslayer.selectAll(".nodes")
            .data(graphnodes.GetArray(), function (d) { return d.db_id; })
            .exit().remove();

        resetScale();
        //updateAllNodeLocations();
        cleanAndUpdateLabelEyes();
    });
}

//function getAllNodeIds() {
//    var nodeids = [];
//    zoomLayer.selectAll(".nodes")
//        .each(function (d) {
//            nodeids.push(d.db_id);
//        }
//    );

//    return nodeids;
//}


//check the number of nodes in the view, and enable perfmode if required
function checkPerfMode() {
    if (graphnodes.GetArray().length > maxAnimateNodes) { perfmode = true; }
    else { perfmode = false; }
}

function findFromDbId(arraydata, id) {
    for (var i = 0; i < arraydata.length; i++) {
        if (arraydata[i].db_id === id) {
            return arraydata[i];
        }
    }
    return null;
}



function pinNode(d) {
    //console.log("pinNode");
    d.fx = d.x;
    d.fy = d.y;
    if (!d.pinned) {
        let ping = d3.selectAll("#node_" + d.db_id)
            .append("g")
            .classed("pin", true)
            .on("click", unpinNode);
        ping.append("circle")
            .attr("r", 8 * d.scaling)
            .attr("cx", 5 * d.scaling)
            .attr("cy", 5 * d.scaling);
        ping.append("i")
            .classed("fas fa-thumbtack pinicon", true)
            .attr("x", 0)
            .attr("y", 1)
            .attr("height", 10 * d.scaling)
            .attr("width", 10 * d.scaling);
        d.pinned = true;
    }
}

function unpinNode(d) {
    //console.log(": unpinNode");
    if (d3.event.defaultPrevented) return; // dragged
    d3.event.stopPropagation();
    delete d.fx;
    delete d.fy;
    d3.selectAll("#node_" + d.db_id).select(".pin").remove();
    d.pinned = false;
}

function pageClicked() {
    //console.log("pageClicked");
    if (d3.event.defaultPrevented) { return; } // dragged
    unselectAllNodes();
}

d3.select("#selectBtn").on('click', startSelect);
function startSelect() {
    //console.log("startSelect");
    d3.select("#selectBtn")
        .on('click', stopSelect)
        .classed('viewcontrolActive', true);
    d3.select("#cropBtn")
        .on('click', startCrop)
        .classed('viewcontrolActive', false);
    drawingsvg
        .on('click', onDrawAreaBoxClick, true)
        .on('mousedown', onSelectMouseDown, true)
        .on('touchstart', onSelectMouseDown, true)
        .on(".zoom", null);
}

function stopSelect() {
    //console.log("stopSelect");
    if (areaBox !== undefined) { areaBox.remove(); }

    d3.select("#selectBtn")
        .on('click', startSelect)
        .classed('viewcontrolActive', false);

    //delay the re-register so any mouseup doesn't trigger a pageClick when it gets re-registered
    setTimeout(function () {
        resetDrawingEvents();
    }, 50);
}

function onDrawAreaBoxClick() {
    d3.event.stopPropagation();
}

function onSelectMouseDown() {
    //console.log("onSelectMouseDown");
    d3.event.stopPropagation();
    if (areaBox !== undefined) { areaBox.remove(); }

    areaBox = drawingsvg.append("rect")
        .attr("id", "areaBox")
        .attr("class", "cropBox");

    var oriMouseX = d3.mouse(this)[0];
    var oriMouseY = d3.mouse(this)[1];

    drawingsvg
        .on("mousemove", function () {
            drawAreaBox(areaBox, [oriMouseX, oriMouseY], d3.mouse(this));
        })
        .on("mouseup", function () {
            //console.log("onSelectMouseDown mouseup");
            let newMouseX = d3.mouse(this)[0];
            let newMouseY = d3.mouse(this)[1];

            let areaBoxEl = areaBox.node().getBoundingClientRect();

            if (newMouseX !== oriMouseX && newMouseY !== oriMouseY) {
                d3.selectAll(".nodes.enabled")
                    .each(function (d) {
                        let elem = d3.select("#node_" + d.db_id + "_icon").node().getBoundingClientRect();

                        if (areaBoxEl.top < elem.top && areaBoxEl.bottom > elem.bottom && areaBoxEl.left < elem.left && areaBoxEl.right > elem.right) {
                            updateNodeSelection(d, true, false);
                        }
                        else {
                            if (d3.event.ctrlKey === false) {
                                updateNodeSelection(d, false, false);
                            }
                        }
                    });
            }

            drawingsvg
                .on("mousemove", null)
                .on("mouseup", null);
            areaBox.remove();
            stopSelect();
        });
}

d3.select("#cropBtn").on('click', startCrop);
function startCrop() {
    //console.log("startSelect");
    d3.select("#selectBtn")
        .on('click', startSelect)
        .classed('viewcontrolActive', false);
    d3.select("#cropBtn")
        .on('click', stopCrop)
        .classed('viewcontrolActive', true);
    drawingsvg
        .on('click', onDrawAreaBoxClick, true)
        .on('mousedown', onCropMouseDown, true)
        .on('touchstart', onCropMouseDown, true);
    drawingsvg.on(".zoom", null);
}

function stopCrop() {
    //console.log("stopSelect");
    if (areaBox !== undefined) { areaBox.remove(); }
    d3.select("#cropBtn")
        .on('click', startCrop)
        .classed('viewcontrolActive', false);

    //delay the re-register so any mouseup doesn't trigger a pageClick when it gets re-registered
    setTimeout(function () {
        resetDrawingEvents();
    }, 50);
}

function onCropMouseDown() {
    d3.event.stopPropagation();
    if (areaBox !== undefined) { areaBox.remove(); }

    areaBox = drawingsvg.append("rect")
        .attr("id", "areaBox")
        .attr("class", "cropBox");

    var oriMouseX = d3.mouse(this)[0];
    var oriMouseY = d3.mouse(this)[1];

    drawingsvg
        .on("mousemove", function () {
            drawAreaBox(areaBox, [oriMouseX, oriMouseY], d3.mouse(this));
        })
        .on("mouseup", function () {
            drawingsvg
                .on("mousemove", null)
                .on("mouseup", null);

            let newMouseX = d3.mouse(this)[0];
            let newMouseY = d3.mouse(this)[1];

            if (newMouseX !== oriMouseX && newMouseY !== oriMouseY) {
                let box = drawingPane.node().getBoundingClientRect();
                let areaBoxEl = areaBox.node().getBBox();
                let currentk = d3.zoomTransform(drawingsvg.node()).k;
                let k = Math.min(box.width / areaBoxEl.width, box.height / areaBoxEl.height);
                let areaBoxElCenterX = areaBoxEl.x + areaBoxEl.width / 2;
                let areaBoxElCenterY = areaBoxEl.y + areaBoxEl.height / 2;
                let movex = (box.width / 2 - areaBoxElCenterX) / currentk;
                let movey = (box.height / 2 - areaBoxElCenterY) / currentk;

                drawingsvg
                    .transition()
                    .duration(500)
                    .ease(d3.easeCubicInOut)
                    .call(zoomer.translateBy, movex, movey)
                    .on("end", function () {
                        drawingsvg
                            .transition()
                            .duration(500)
                            .ease(d3.easeCubicInOut)
                            .call(zoomer.scaleBy, k);
                    });
            }
            areaBox.remove();
            stopCrop();
        });
}

function drawAreaBox(areaBoxEl, oriCoords, newCoords) {
    areaBoxEl.attr("x", Math.min(oriCoords[0], newCoords[0]))
        .attr("y", Math.min(newCoords[1], oriCoords[1]))
        .attr("width", Math.abs(newCoords[0] - oriCoords[0]))
        .attr("height", Math.abs(newCoords[1] - oriCoords[1]));
}


function onZoom() {
    //console.log("onZoom");
    zoomLayer.attr("transform", d3.event.transform);
}

d3.selectAll("#centerBtn").attr('onclick', "vis.updatePaneSize()");
export function updatePaneSize() {
    //console.log("updatePaneSize");
    var box = drawingPane.node().getBoundingClientRect();
    var svgbox = drawingsvg.node().getBBox();
    var k = d3.zoomTransform(drawingsvg.node()).k;
    var movex = (box.width / 2 - svgbox.x - svgbox.width / 2) / k;
    var movey = (box.height / 2 - svgbox.y - svgbox.height / 2) / k;

    drawingsvg
        .transition()
        .duration(1000)
        .ease(d3.easeCubicInOut)
        .call(zoomer.translateBy, movex, movey);
}

function onNodeDblClicked(d) {
    //console.log(": onNodeDblClicked");
    //if (d3.event.defaultPrevented) { return; } // dragged
    //d3.event.stopPropagation();
    //getRelated(d.db_id);
}

function onNodeClicked(d) {
    //console.log("onNodeClicked");
    if (d3.event.defaultPrevented) { return; } // dragged
    d3.event.stopPropagation();

    if (d3.event.ctrlKey) {
        //if ctrl key is down, just toggle the node		
        updateNodeSelection(d, !d.selected, !d.selected);
    }
    else {
        //if the ctrl key isn't down, unselect everything and select the node
        unselectAllOtherNodes(d);
        updateNodeSelection(d, true, true);
    }
}

function updateNodeSelection(d, isselected, showdetails) {
    //console.log("updateNodeSelection : " + d.name + " : " + d.selected + ":" + isselected);     
    let node = nodeslayer.select("#node_" + d.db_id)
        .classed("selected", isselected);

    let enabled = node.classed("enabled");
    //console.log("enabled: " + enabled);
    d3.select("#eyeIcon_" + d.db_id)
        .classed("fa-eye", enabled)
        .classed("fa-eye-slash", !enabled);

    if (d.selected !== isselected) {
        d.selected = isselected;
        if (isselected) {
            if (showdetails === true) {
                if (!d.hasOwnProperty('detailsHTML')) {
                    //console.log("populate");
                    populateDetails(d, function () {
                        //console.log("populate callback");
                        nodeShowDetailsSelected(d);
                    });
                }
                else { nodeShowDetailsSelected(d); }
            }
        }
        else { nodeHideDetailsSelected(d); }
    }
}

/*
Build the details card for the node
*/
function populateDetails(d, callback) {
    //console.log("populateDetails");
    webcrap.data.apiGet("/visualizer/details/" + d.db_id, "html", function (data) {
        //console.log("populateDetails callback");
        d.detailsHTML = data;
        callback();
    });
}

function unselectAllOtherNodes(keptdatum) {
    //console.log("unselectAllOtherNodes");
    d3.selectAll(".selected")
        .classed("selected", function (d) {
            if (keptdatum.db_id !== d.db_id) {
                nodeHideDetailsSelected(d);
                d.selected = false;
                return false;
            }
        });
}

function unselectAllNodes() {
    //console.log("unselectAllNodes");
    d3.selectAll(".detailcard").remove();
    d3.selectAll(".selected")
        .classed("selected", function (d) {
            d.selected = false;
            return false;
        });
}

function nodeShowDetailsSelected(d) {
    //console.log("nodeShowDetailsSelected");
    //console.log(d.detailsHTML);
    d3.select("#detailcardwrapper")
        .append("div")
        .attr("id", "details_" + d.db_id)
        .attr("class", "detailcard pane")
        .html(d.detailsHTML);
    $('#details_' + d.db_id).foundation();
}

function nodeHideDetailsSelected(d) {
    //console.log("nodeHideDetailsSelected");
    d3.selectAll("#details_" + d.db_id).remove();
}

function onNodeMouseOver(d) {
    //console.log("onNodeMouseOver: " + d.name);
    d3.selectAll("#details_" + d.db_id)
        .classed("currentActiveDetailCard", true);
}

function onNodeMouseOut(d) {
    //console.log("onNodeMouseOut: " + d.name);
    d3.selectAll("#details_" + d.db_id)
        .classed("currentActiveDetailCard", false);
}

function onNodeDragged(d) {
    //console.log("onNodeDragged");
    if (d3.event.dx === 0 && 0 === d3.event.dy) { return; }
    d3.event.sourceEvent.stopPropagation();
    d.dragged = true;
    let nodes;
    //if (playMode === true) { pauseLayout(); }
    //if the node is selected the move it and all other selected nodes
    if (d.selected) {
        nodes = d3.selectAll(".selected")
            .each(function (seld) {
                seld.x += d3.event.dx;
                seld.y += d3.event.dy;
                seld.startx = seld.x;
                seld.starty = seld.y;
                pinNode(d);
            });
    }
    else {
        nodes = d3.select("#node_" + d.db_id);
        d.x += d3.event.dx;
        d.y += d3.event.dy;
        d.startx = d.x;
        d.starty = d.y;
        pinNode(d);
    }

    updateNodeLocations(nodes, false);
}

function onNodeDragStart(d) {
    //console.log("onNodeDragStart");
    //d3.event.sourceEvent.stopPropagation();

    simController.StopSimulations();
}

function onNodeDragFinished(d) {
    //console.log("onNodeDragFinished");
    d3.event.sourceEvent.stopPropagation();

    if (playMode === true && d.dragged === true) {
        restartLayout();
    }
    d.dragged = false;
}

//update location for all nodes
function updateAllNodeLocations(animate) {
    let nodes = nodeslayer.selectAll(".nodes");
    updateNodeLocations(nodes, animate);
}

function updateNodeLocations(nodes, animate) {
    //console.log("updateLocations");
    //console.log(nodes);
    let duration = 750;

    if (animate === true) {
        let nodecount = nodes.size() - 1;
        nodes = nodes
            .transition("nodes_update")
            .tween("link_update", function (d, i) {
                let interx = d3.interpolateNumber(d.startx, d.x);
                let intery = d3.interpolateNumber(d.starty, d.y);
                let bg = graphbglayer.select("#nodebg_" + d.db_id);
                return function (t) {
                    //console.log(nodecount);
                    if (d.dragged === true) { return; }
                    d.cx = interx(t) + d.radius;
                    d.cy = intery(t) + d.radius;
                    //update the edge locations when you get to the end of the nodes list
                    //don't run on the first 'tick'
                    if (i === nodecount && t !== 0) {
                        updateLocationsEdges();
                    }
                    bg.attr("transform", function () { return "translate(" + interx(t) + "," + intery(t) + ")"; });
                };
            })
            .ease(d3.easeCubic)
            .duration(duration);
    }
    else {
        nodes
            .each(function (d) {
                d.cx = d.x + d.radius;
                d.cy = d.y + d.radius;
                graphbglayer.select("#nodebg_" + d.db_id)
                    .attr("transform", function (d) { return "translate(" + d.x + "," + d.y + ")"; });
            });
        updateLocationsEdges();
    }
    nodes.attr("transform", function (d) { return "translate(" + d.x + "," + d.y + ")"; });
}

function updateLocationsEdges() {
    //console.log("updateLocationsEdges");
    let alledges = edgeslayer.selectAll(".edges").data(graphedges.GetArray());
    let edgebgwidth = 13;
    alledges.each(function (d) {
        //console.log(d);
        let diagLine = new Slope(d.source.cx, d.source.cy, d.target.cx, d.target.cy);

        //move and rotate the edge line to the right spot
        let edge = d3.select(this).attr("transform", function () {
            return "rotate(" + diagLine.deg + " " + diagLine.x1 + " " + diagLine.y1 + ") " +
                "translate(" + diagLine.x1 + " " + diagLine.y1 + ")";
        });

        //do the bg as well
        graphbglayer.select("#edgebg_" + d.db_id)
            .attr("transform", function () {
                return "rotate(" + diagLine.deg + " " + diagLine.x1 + " " + diagLine.y1 + ") " +
                    "translate(" + diagLine.x1 + " " + diagLine.y1 + ")";
            })
            .attr("d", function () {
                return "M 0 -" + edgebgwidth + " " +
                    "L " + diagLine.length + " -" + edgebgwidth + " " +
                    "L " + diagLine.length + " " + edgebgwidth + " " +
                    "L 0 " + edgebgwidth + " " + " Z";
            });

        // Reevaluate the path coordinates so it looks right
        edge.selectAll(".wrapper")
            .attr("d", function (d) {
                return "M 0 -3 " +
                    "L " + diagLine.length + " -3 " +
                    "L " + diagLine.length + " 3 " +
                    "L 0 3 Z";
            });

        edge.selectAll(".arrows")
            .attr("d", function (d) {
                let lineend = Math.max(diagLine.length - d.target.radius - 8, 0);
                let linepoint = Math.max(lineend + 5, lineend);
                let linestart = Math.min(d.source.radius + 5, lineend);

                let path = "M " + linestart + " -0.5 " +
                    "L " + lineend + " -0.5 " +
                    "L " + lineend + " -5 " +
                    "L " + linepoint + " 0 " +
                    "L " + lineend + " 5 " +
                    "L " + lineend + " 0.5 " +
                    "L " + linestart + " 0.5 Z ";

                return path;
            });

        edge.selectAll(".edgelabel")
            .attr("transform-origin", "30,0")
            .attr("transform", function (d) {
                //let translation;
                if (diagLine.x2 > diagLine.x1) {
                    return "translate(" + diagLine.mid + ",0)";
                }
                else {
                    //let axis = diagLine.getCoordsFromLength(diagLine.mid);
                    return "translate(" + diagLine.mid + ",0) rotate(180)";
                }
            });
    });
}








/* 
****************************
Search functionality
****************************
*/


var pendingResults;


function search() {
    showSearchSpinner();
    //console.log('search');
    var sourcetype = document.getElementById("sourceType").value;
    var sourceprop = document.getElementById("sourceProp").value;
    var sourceval = document.getElementById("sourceVal").value;

    var relationship = document.getElementById("edgeType").value;
    var relmin = document.getElementById("sliderMin").value;
    var relmax = document.getElementById("sliderMax").value;

    var tartype = document.getElementById("targetType").value;
    var tarprop = document.getElementById("targetProp").value;
    var tarval = document.getElementById("targetVal").value;

    var dir = $('#dirIcon').attr('data-dir');

    let urlquery = "?" +
        "sourcetype=" + sourcetype +
        "&sourceprop=" + sourceprop +
        "&sourceval=" + sourceval +

        "&relationship=" + relationship +
        "&relmin=" + relmin +
        "&relmax=" + relmax +
        "&dir=" + dir +

        "&targettype=" + tartype +
        "&targetprop=" + tarprop +
        "&targetval=" + tarval;

    /*console.log("sourcetype: " + sourcetype);
    console.log("sourceprop: " + sourceprop);
    console.log("sourceval: " + sourceval);
    console.log("relationship: " + relationship);
    console.log("tartype: " + tartype);
    console.log("tarprop: " + tarprop);
    console.log("tarval: " + tarval);
    console.log(url);*/
    webcrap.data.apiGet("/visualizer/search" + urlquery, "html", function (data) {
        document.getElementById("searchNotification").innerHTML = data;
        $('#searchNotification').foundation();
    });
}


export function getNode(nodeid) {
    //console.log(nodeid);
    webcrap.data.apiGetJson("/api/graph/node/" + nodeid, function (data) {
        //console.log(data);
        queue.QueueResults(data);
    });
}

// GET api/graph/nodes?id=1&id=2
function getNodes(nodeids) {
    var tempArr = [];
    nodeids.forEach(function (id) {
        tempArr.push("id=" + id);
    });

    var querystring = tempArr.join("&");
    //console.log(querystring);
    webcrap.data.apiGetJson("/api/graph/nodes?" + querystring, function (data) {
        //console.log(data);
        queue.QueueResults(data);
    });
}

function getRelated(nodeid) {
    //console.log("getRelated"+ nodeid);
    webcrap.data.apiGetJson("/api/graph/nodes/" + nodeid, function (data) {
        //pendingResults = data;
        queue.QueueResults(data);
    });

}

function getEdgesForNodes(nodelist, callback) {
    var postdata = JSON.stringify(nodelist);
    //console.log(": json stringyfied");
    //console.log(nodeids);

    webcrap.data.apiPostJson('/api/graph/edges', postdata, callback);
}


function getDirectEdgesForNodeList(nodelist, callback) {
    var postdata = JSON.stringify(nodelist);
    //console.log(": json stringyfied");
    //console.log(nodeids);

    webcrap.data.apiPostJson('/api/graph/edges/direct', postdata, callback);
}

// Keystroke event handlers
var typetimer = null;
function timedKeyUp(timedfunction) {
    clearTimeout(typetimer);
    typetimer = setTimeout(timedfunction, 700);
}