var drawpane;
var zoomLayer;
var svg;
var simulation;
var force;
var linkdata = [];
var nodedata = [];

var playMode = false;
var shiftKey = false;
var ctrlKey = false;

var paneWidth = 640;
var paneHeight = 480;
var defaultsize = 40;
var edgelabelwidth = 70;

var currMaxRelated = 0
var currMinRelated = 9007199254740991;
var minScaling = 1;
var maxScaling = 4;





function drawGraph(selectid) {
	drawPane = d3.select("#"+selectid)
		.on("keydown", keydown)
		.on("keyup", keyup);

	svg = drawPane.append("svg")
		.attr("id","drawingsvg")		
		.on("click", pageClicked);

	//setup the zooming layer
	zoomLayer = svg.append("g");
	svg.call(d3.zoom()
		.scaleExtent([0.05, 5])
		.on("zoom", function() {
				zoomLayer.attr("transform", d3.event.transform);
			}));	

	simulation=d3.forceSimulation(nodedata);
	simulation.stop();

	simulation
		.force("link", d3.forceLink(linkdata)		
			.id(function(d) { return d.db_id; })
			.distance(175)
			.strength(1))
		.force('collision', d3.forceCollide().radius(function(d) { return (d.size * 2)}))
		.force('charge', d3.forceManyBody().strength(2)) 
		.force('center', d3.forceCenter(paneWidth / 2, paneHeight / 2))
		.on('tick', function () { updateLocations(); })
		.velocityDecay(0.3)
		.alphaDecay(0.1);
}

function addResultSet(json) {
	addEdges(json.edges);
	addNodes(json.nodes);
}

function restartLayout() { 
	//console.log('restartLayout');
	simulation.nodes(nodedata);

	simulation.force("link")
		.links(linkdata);

	simulation.alpha(1).restart();
}

function playLayout() { 
	playMode = true;

	d3.selectAll("#pausePlayIcon")
		.classed("fa-play",false)
		.classed("fa-pause",true);
	d3.selectAll("#pausePlayBtn")
		.attr('onclick', "pauseLayout()");
	restartLayout();
}

function pauseLayout() { 
	playMode = false;

	simulation.stop();
	d3.selectAll("#pausePlayIcon")
		.classed("fa-pause",false)
		.classed("fa-play",true);
	d3.selectAll("#pausePlayBtn")
		.attr('onclick','playLayout()');
}


/*
*****************************
setup edges and nodes
*****************************
*/

function addEdges(data) {
	//setup the edges
	let enteredges = zoomLayer.selectAll(".edges")
		.data(data, function(d) { return d.db_id; })
		.enter()
		.append("g")
		.attr("id",function(d) { return "edge_"+d.db_id; })
		.attr("class", function(d) { return d.label })
		.classed("edges",true);

	enteredges.append("path")
		.classed("wrapper",true)
		.attr("fill","none");

	enteredges.append("path")
		.classed("arrows",true);	
	
	let edgelabels = enteredges.append("g")
		.classed("edgelabel",true);

	edgelabels.append("text")
		.attr("text-anchor","middle")
		.attr("dominant-baseline","central")
		.text(function(d) {return d.label});

	let newcount = 0;
	//console.log("linkdata old count " + linkdata.length);

	enteredges.each( function(d) {
		newcount++;
		linkdata.push(d);
	});
}


function getAllNodeIds() {
	var nodeids = [];
	zoomLayer.selectAll(".nodes")
		.each(function (d) {
			nodeids.push(d.db_id);
		}
	);

	return nodeids;
}
	
function addNodes(data) {
	//populate necessary additional data for the view
	loadNodeData(data);

	//build the nodes
	let enternodes = zoomLayer.selectAll(".nodes")
		.data(data, function(d) { return d.db_id; })
		.enter();

	let enternodesg = enternodes.append("g")
			.attr("id", function(d) { return "node_" + d.db_id; })
			.attr("class", function(d) { return d.label })
			.classed("nodes",true)
			.classed("selected", function(d) { 
				return d.selected;})
			.on("click", nodeClicked)
			.on("mouseover", nodeMouseOver)
			.on("mouseout", nodeMouseOut)
			.on("dblclick", nodeDblClicked)
			.call(
				d3.drag().subject(this)
					.on('drag',nodeDragged));

	//node layout
	enternodesg.append("circle")
		.classed("nodecircle",true)
		.attr("r", function(d) { return d.radius + "px"; })
		.attr("cx", function(d) { return d.radius; })
		.attr("cy", function(d) { return d.radius; });
		
	enternodesg.append("i")
		.attr("height", function(d) { return ( d.size * 0.6) + "px" })
		.attr("width", function(d) { return (d.size * 0.6) + "px" })
		.attr("x", function(d) { return (d.size * 0.2) })
		.attr("y",function(d) { return (d.size * 0.2)})
		.attr("class", function(d) { return iconmappings.getClassInfo(d.label); })
		.classed("nodeicon",true); 

	enternodesg.append("text")
		.text(function(d) { return d.name; })
		.attr("text-anchor","middle")
		.attr("dominant-baseline","central")
		.attr("font-size","10px")
		.attr("font-family","arial")
		.attr("transform",function(d) { return "translate(" + (d.size/2) + "," + (d.size + 7) + ")" }); 

	let newcount = 0;

	enternodes.each(function (d) {
		nodedata.push(d);
		newcount++;
		});
}
	

function findNode (id) {
	for (var i=0; i < nodedata.length; i++) {
		if (nodedata[i].db_id === id) { return nodedata[i]; }
	};
}

/*
*****************************
functions
*****************************
*/

function pinNode(d) {
	//console.log("pinNode");
	d.fx = d.x;
	d.fy = d.y;
	if (!d.pinned) {
		let ping = d3.selectAll("#node_" + d.db_id)
			.append("g")
				.classed("pin",true)
				.on("click", unpinNode);
		ping.append("circle")
			.attr("r",5*d.scaling)
			.attr("cx",5*d.scaling)
			.attr("cy",5*d.scaling);
		ping.append("i")
			.classed("fas fa-thumbtack",true)
			.attr("x",2*d.scaling)
			.attr("y",2*d.scaling)
			.attr("height",6*d.scaling)
			.attr("width",6*d.scaling);
		d.pinned = true;
	}
}

function unpinNode(d) {
	//console.log("unpinNode");
	if (d3.event.defaultPrevented) return; // dragged
	delete d.fx;
	delete d.fy;
	if (d.pinned) {
		d3.selectAll("#node_" + d.db_id).select(".pin").remove();
		d.pinned = false;
	}
}

function keydown() {
	shiftKey = d3.event.shiftKey || d3.event.metaKey;
	ctrlKey = d3.event.ctrlKey;
}

function keyup() {
	shiftKey = d3.event.shiftKey || d3.event.metaKey;
	ctrlKey = d3.event.ctrlKey;
}

function pageClicked(d){
	//console.log("pageClicked");
	if (d3.event.defaultPrevented) return; // dragged
	unselectAllNodes();		
}

function nodeDblClicked(d) {
	console.log("nodeDblClicked");
	if (d3.event.defaultPrevented) return; // dragged

	d3.event.stopPropagation();
	addRelated(d.db_id);
}

function nodeClicked(d) {
	//console.log("nodeClicked");
	if (d3.event.defaultPrevented) return; // dragged

	d3.event.stopPropagation();
	if (ctrlKey) {	
		//if ctrl key is down, just toggle the node		
		updateNodeSelection(d, !d.selected);
	}
	else {
		//if the ctrl key isn't down, unselect everything and select the node
		unselectAllOtherNodes(d);
		updateNodeSelection(d, true);
	} 
}

function updateNodeSelection(d, isselected) {
	//console.log("updateNodeSelection");
	d3.select("#node_"+ (d.db_id))
		.classed("selected", function() { 
			d.selected = isselected;
			return isselected;
		});
	if (isselected) {
		nodeShowDetailsSelected(d);
	}
	else {
		nodeHideDetailsSelected(d);
	}
}

function unselectAllOtherNodes(keptdatum) {
	//console.log("unselectAllOtherNodes");
	d3.selectAll(".selected")
		.classed("selected", function(d) { 
			if (keptdatum.db_id !== d.db_id) {
				nodeHideDetailsSelected(d);
				d.selected = false;
				return false; 
			}
		});			
}

function unselectAllNodes() {
	//console.log("unselectAllNodes");
	d3.selectAll(".selected")
		.classed("selected", function(d) { 
			d.selected = false;
			return false; }); 

	d3.selectAll(".detailcard").remove();
}

function nodeShowDetailsSelected(d) {
	//console.log("nodeShowDetailsSelected");
	d3.select("#detailcardwrapper")
		.append("div")
			.attr("id","details_"+d.db_id)
			.attr("class","detailcard pane")
			.html(d.detailsHTML);
}

function nodeHideDetailsSelected(d) {
	//console.log("nodeHideDetailsSelected");
	d3.selectAll("#details_" + d.db_id).remove();
}

function nodeMouseOver(d) {
	//console.log("nodeMouseOver");
	d3.selectAll("#details_" + d.db_id)
		.classed("currentActiveDetailCard",true);
}

function nodeMouseOut(d) {
	//console.log("nodeMouseOut");
	d3.selectAll("#details_" + d.db_id)
		.classed("currentActiveDetailCard",false);
}

function nodeDragged(d){
	//console.log("nodeDragged");
	d3.event.sourceEvent.stopPropagation();
	//if the node is selected the move it and all other selected nodes
	if (d.selected) { 
		d3.selectAll(".selected")
			.each(function(d) { 
				d.x += d3.event.dx;
				d.y += d3.event.dy;
				pinNode(d);
			});
	}
	else {
		d.x += d3.event.dx;
		d.y += d3.event.dy; 
		pinNode(d);
	}

	updateLocations();
	if (playMode === true) { restartLayout(); }
}

function loadNodeData(newnodedata) {
	//console.log(newnodedata);
	let rangeUpdated = false;
	//evaluate the nodes to figure out the max and min size so we can work out the scaling
	newnodedata.forEach(function(d) {			
		if (d.relatedcount>currMaxRelated) {
			rangeUpdated = true;
			currMaxRelated = d.relatedcount;
		}
		if (d.relatedcount<currMinRelated) {
			rangeUpdated = true;
			currMinRelated = d.relatedcount;
		}
	});	
	let scalingRange = new Slope(currMinRelated, minScaling, currMaxRelated, maxScaling);

	//load the data and pre-calculate/set the values for each node 	
	newnodedata.forEach(function(d) {
		if (currMaxRelated>0) { d.scaling = scalingRange.getYFromX(d.relatedcount); }
		else { d.scaling = 1; }

		d.radius = ((defaultsize*d.scaling)/2); 
		d.x = 0;
		d.y = 0;
		d.cx = d.x + d.radius;
		d.cy = d.y + d.radius;
		d.pinned = false;
		d.selected = false;
		d.size = defaultsize * d.scaling;
		populateDetails(d);
	});	

	return newnodedata;	
}

function updateLocations() {
	let alledges = zoomLayer.selectAll(".edges").data(linkdata);

	alledges.each(function(d) {
		let diagLine = new Slope(d.source.cx, d.source.cy, d.target.cx, d.target.cy);

		//move and rotate the edge line to the right spot

		let edge = d3.select(this)
			.attr("transform", function() {
				return "rotate(" + diagLine.deg + " " + diagLine.x1 + " " + diagLine.y1 + ") " +
					"translate("+ diagLine.x1 + " " + diagLine.y1 + ")";
			});

		//now the hard bit. Reevaluate the path coordinates so it looks right
		edge.selectAll(".wrapper")
			.attr("d",function(d) {
				return "M 0 -3 " +
					"L "+ (diagLine.length) + " -3 "+
					"L "+ (diagLine.length) + " 3 "+ 
					"L 0 3 Z"; 
			});

		edge.selectAll(".arrows")
			.attr("d",function(d) {
				let line1start;
				
				let line1end = Math.max(diagLine.mid-30, 0);
				let line2start = Math.min(diagLine.mid + 30,diagLine.length);
				let line2end = Math.max(diagLine.length - d.target.radius - 8, line2start);
				let line2point = Math.max(line2end + 5, line2end);
				
				let path;
				if (d.bidir) { //bidirectional edge with double ended arrows
					let line1point = Math.min(d.source.radius + 5,line1end-5);
					line1start = Math.min(line1point + 5,line1end);

					path = "M " + line1point + " 0 " +
					"L " + line1start + " -3 " +
					"L " + line1start + " -1 " +
					"L " + line1end + " -1 " + 
					"L " + line1end + " 1 " +
					"L " + line1start + " 1 " +
					"L " + line1start + " 3 Z ";
				} 
				else { //single direction edge with single ended arrow
					line1start = Math.min(d.source.radius + 5,line1end);
					line1end = Math.max(diagLine.mid-30, line1start);

					path = "M " + line1start + " -1 " + 
					"L " + line1end + " -1 " + 
					"L " + line1end + " 1 " +
					"L " + line1start + " 1 Z ";
				}	

				path +=	"M " + line2start + " -1 " + 
					"L " + line2end + " -1 " +
					"L " + line2end + " -3 " +
					"L " + line2point + " 0 " +
					"L " + line2end + " 3 " +
					"L " + line2end + " 1 " +
					"L " + line2start + " 1 Z "; 
				return path;
			});

		edge.selectAll(".edgelabel")
			.attr("transform-origin","30,0")
			.attr("transform",function(d) { 
				let translation;
				if (diagLine.x2 > diagLine.x1) {
					return "translate(" + diagLine.mid  + ",0)";
				}
				else {
					let axis = diagLine.getCoordsFromLength(diagLine.mid);
					return "translate(" + diagLine.mid + ",0) rotate(180)";
				}
			})
	});
		
	zoomLayer.selectAll(".nodes").data(nodedata)
		.attr("x",function(d) { 
			d.cx = d.x + d.radius;
			return d.x; })
		.attr("y",function(d) { 
			d.cy = d.y + d.radius;
			return d.y; })
		.attr("transform", function (d) { return "translate(" + d.x  + "," + d.y + ")" });	
}

//based on an angled line, eval all the relevant details
function Slope(x1, y1, x2, y2) {
	this.x1 = x1;
	this.y1 = y1;
	this.x2 = x2;
	this.y2 = y2;
	this.xd = x2 - x1; //delta x
	this.yd = y2 - y1; //delta y

	this.length = Math.sqrt((this.xd * this.xd) + (this.yd * this.yd));
	this.mid = this.length / 2;
	this.deg = Math.atan2(this.yd, this.xd) * (180 / Math.PI);
	this.sinA = this.yd / this.length;
	this.cosA = this.xd / this.length;	
	this.tanA = this.yd / this.xd;
}

// find coordinates of a point along the line from the source (x1,y1)
Slope.prototype.getCoordsFromLength = function(length)
{
	let ret = {
		x:((this.cosA / length)+this.x1), 
		y:((this.sinA / length)+this.y1)};
	return ret;
}

Slope.prototype.getYFromX = function(x)
{
	return (this.tanA * (x-this.x1)) + this.y1;
}

Slope.prototype.getXFromY = function(y)
{
	return (this.tanA / (y-this.y1))+ this.x1;
}

function IconMappings(jsondata){
	this.mappings = jsondata;
}

IconMappings.prototype.getClassInfo = function(label) {
	if (this.mappings.hasOwnProperty(label)) { return this.mappings[label]; }
	else { return "fas fa-question"; }
}

function populateDetails(d) {
	d.detailsHTML = function () {
		let s = "<u><b>Details</b></u><br>" +
			"<b>Name:</b> " + d.name + "<br>" +
			"<b>db_id:</b> " + d.db_id + "<br>" +
			"<b>Label:</b> " + d.label + "<br>" +
			"<b>Related:</b> " + d.relatedcount + "<br><br><b><u>Properties</u></b><br>";

		if (d.properties) {
			d.propertyCount = d.properties.count;
			for(let key in d.properties) {
				s += "<b>" + key + ":</b> " + d.properties[key] + "<br>";
			}
		}
		else {
			d.propertyCount = 0;
			s += "empty<br>";
		}
		return s;
	}
}