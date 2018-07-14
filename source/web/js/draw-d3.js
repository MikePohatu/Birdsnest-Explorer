let json = '{\
	"nodes":[\
		{"db_id":450, "label": "AD_USER", "name":"Node0-bob the builder", "relatedcount":1,\
			"properties": {\
				"samaccountname":"bobB",\
				"name":"bob the builder",\
				"sid":"a;lkdjfaljdalkjdhf"\
			}\
		},\
		{"db_id":21, "label": "AD_GROUP","name":"Node1-group", "relatedcount":1,\
			"properties": {\
				"samaccountname":"Node1Grp",\
				"name":"Node1-group",\
				"sid":";lkajsdfljkh"\
			}\
		},\
		{"db_id":42, "label": "AD_COMPUTER","name":"Node2-Computer", "relatedcount":1},\
		{"db_id":3, "label": "AD_USER","name":"Node3", "relatedcount":1},\
		{"db_id":54, "label": "AD_USER","name":"Node4", "relatedcount":1},\
		{"db_id":564, "label": "FS_DATASTORE","name":"Node5-datastore", "relatedcount":1},\
		{"db_id":61, "label": "AD_USER","name":"Node6", "relatedcount":1},\
		{"db_id":75, "label": "AD_USER","name":"Node7", "relatedcount":1},\
		{"db_id":833, "label": "AD_USER","name":"Node8", "relatedcount":1},\
		{"db_id":9112, "label": "AD_USER","name":"Node9", "relatedcount":1},\
		{"db_id":100, "label": "AD_USER","name":"Node10", "relatedcount":1},\
		{"db_id":1, "label": "AD_USER","name":"Node11", "relatedcount":1},\
		{"db_id":2, "label": "AD_USER","name":"Node12", "relatedcount":1},\
		{"db_id":33, "label": "AD_USER","name":"Node13", "relatedcount":1},\
		{"db_id":4, "label": "FS_FOLDER","name":"Node14-folder", "relatedcount":1},\
		{"db_id":15, "label": "AD_USER","name":"Node15", "relatedcount":1},\
		{"db_id":16, "label": "AD_USER","name":"Node16", "relatedcount":1},\
		{"db_id":17, "label": "AD_USER","name":"Node17", "relatedcount":1.5},\
		{"db_id":18, "label": "AD_USER","name":"Node18", "relatedcount":1}\
	],\
	"edges":[\
		{"source": 4, "target": 564, "bidir":false, "label":"ConnectedTo"},\
		{"source": 21, "target": 4, "bidir":false, "label":"GivesAccessTo"},\
		{"source": 450, "target": 1, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 3, "target": 2, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 9112, "target": 61, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 4, "target": 33, "bidir":true, "label":"AD_MemberOf"},\
		{"source": 4, "target": 9112, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 9112, "target": 100, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 15, "target": 33, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 15, "target": 17, "bidir":false, "label":"AD_MemberOf"},\
		{"source": 17, "target": 18, "bidir":false, "label":"AD_MemberOf"}\
	]\
}';

let iconsjson = '{\
	"AD_USER":"fas fa-user",\
	"AD_COMPUTER":"fas fa-desktop",\
	"AD_GROUP":"fas fa-user-friends",\
	"BUILTIN_GROUP":"fas fa-user-friends",\
	"DEVICE":"fas fa-tablet-alt",\
	"FS_FOLDER":"fas fa-folder-open",\
	"FS_DATASTORE":"fas fa-hdd"\
}';

//let jsonData = ""
let simulation=d3.forceSimulation();

function restartLayout(){ 
	simulation.alpha(1);
	simulation.restart();
}

function drawGraph(selectid) {
	let shiftKey = false;
	let ctrlKey = false;

	let paneWidth = 640;
	let paneHeight = 480;
	let defaultsize = 40;
	let edgelabelwidth = 70;

	let jsonData = JSON.parse(json);
	let iconsdata = JSON.parse(iconsjson);
	let nodedata = jsonData.nodes;
	let linkdata = jsonData.edges;

	//load the data and pre-calculate/set the values for each node 
	jsonData.nodes.forEach(function(d) {			
			d.scaling = d.relatedcount;
			d.radius = ((defaultsize*d.scaling)/2); 
			d.x = 0;
			d.y = 0;
			d.cx = d.x + d.radius;
			d.cy = d.y + d.radius;
			d.size = defaultsize * d.scaling;
		});	


	//setup simulation/force layout, bind links to the correct node property etc
	simulation.nodes(nodedata)
		.force("link", d3.forceLink()
			.id(function(d) { return d.db_id; })
			.distance(function(d){ return 150; })
			)	
		.force('charge', d3.forceManyBody()) 
		.force('center', d3.forceCenter(paneWidth / 2, paneHeight / 2))
		.force('collision', d3.forceCollide().radius(function(d) { return (d.size)}))
		.on('tick', function () {		
			updateLocations();
		});
	
	simulation.velocityDecay(0.5);
	simulation.alphaDecay(0.02)
	simulation.force("link")
		.links(linkdata);

	let drawPane = d3.select("#"+selectid)
		.on("keydown", keydown)
		.on("keyup", keyup)
		.on("click", clicked);

	let svg = drawPane.append("svg")
		.classed("drawingpane",true);


	//setup the zooming layer
	let zoomLayer = svg.append("g");
	svg.call(d3.zoom()
		.scaleExtent([0.1, 5])
		.on("zoom", function() {
				zoomLayer.attr("transform", d3.event.transform);
			}));	


	//setup the edges
	let edges = zoomLayer.selectAll(".edges")
		.data(linkdata)
		.enter()
		.append("g")
			.attr("class", function(d) { return d.label })
			.classed("edges",true);

	edges.append("path")
		.classed("wrapper",true)
		.attr("fill","none");

	edges.append("path")
		.classed("arrows",true);	
	
	let edgelabels = edges.append("g")
		.classed("edgelabel",true);

	edgelabels.append("text")
		.attr("text-anchor","middle")
		.attr("dominant-baseline","central")
		.text(function(d) {return d.label});


	//build the nodes
	let nodes = zoomLayer.selectAll(".nodes")
		.data(nodedata)
		.enter()
		.append("g")
			.attr("class", function(d) { return d.label })
			.classed("nodes",true)
			
			.classed("selected", function(d) { 
				d.selected = false; 
				return d.selected;})
			.on("click", nodeClicked)
			.call(
				d3.drag().subject(this)
					.on('drag',nodeDragged));

	//node layout
	nodes.append("circle")
		.attr("r", function(d) { return d.radius + "px"; })
		.attr("cx", function(d) { return d.radius; })
		.attr("cy", function(d) { return d.radius; });
		
	nodes.append("i")
		.attr("height", function(d) { return ( d.size * 0.6) + "px" })
		.attr("width", function(d) { return (d.size * 0.6) + "px" })
		.attr("x", function(d) { return (d.size * 0.2) })
		.attr("y",function(d) { return (d.size * 0.2)})
		.attr("class", function(d) { return iconsdata[d.label] })
		.classed("nodeicon",true); 

	nodes.append("text")
		.text(function(d) { return d.name; })
		.attr("text-anchor","middle")
		.attr("dominant-baseline","central")
		.attr("font-size","10px")
		.attr("font-family","arial")
		.attr("transform",function(d) { return "translate(" + (d.size/2) + "," + (d.size + 7) + ")" }); 



	function nodeDragged(d){
		d3.event.sourceEvent.stopPropagation();
		//if the node is selected the move it and all other selected nodes
		if (d.selected) { 
			nodes.filter(function(d) { return d.selected; })
			.each(function(d) { 
				d.x += d3.event.dx;
				d.y += d3.event.dy;
				d.cx += d3.event.dx;
				d.cy += d3.event.dy;
				d.fx = d.x;
				d.fy = d.y;	
				lockNode(d);
			});
		}
		else {
			d.x += d3.event.dx;
			d.y += d3.event.dy; 
			lockNode(d);
		}

		updateLocations();
	}

	function lockNode(d) {
		d.fx = d.x;
		d.fy = d.y;
		d.locked = true;
	}

	function unlockNode(d) {
		delete d.fx;
		delete d.fy;
		d.locked = false;
	}

	function keydown() {
		shiftKey = d3.event.shiftKey || d3.event.metaKey;
		ctrlKey = d3.event.ctrlKey;
	}

	function keyup() {
		shiftKey = d3.event.shiftKey || d3.event.metaKey;
		ctrlKey = d3.event.ctrlKey;
	}

	function clicked(d){
		if (d3.event.defaultPrevented) return; // dragged
		simulation.stop();
		unselectAllNodes();		
	}

	function nodeClicked(d) {
		d3.event.stopPropagation();
		if (d3.event.defaultPrevented) return; // dragged
		if (ctrlKey) {	
			//if ctrl key is down, just toggle the node		
			updateNodeSelection(this, !(d.selected));
		}
		else {
			//if the ctrl key isn't down, unselect everything and select the node
			unselectAllNodes();
			updateNodeSelection(this, true);
		} 
	}

	function updateLocations() {
		edges.each(function(d) {
			let diagLine = new EdgeLine(d.source.cx, d.source.cy, d.target.cx, d.target.cy);

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
						let axis = diagLine.getCoords(diagLine.mid);
						return "translate(" + diagLine.mid + ",0) rotate(180)";
					}
				})
		});
			
		nodes.attr("x",function(d) { 
				d.cx = d.x + d.radius;
				return d.x; })
			.attr("y",function(d) { 
				d.cy = d.y + d.radius;
				return d.y; })
			.attr("transform", function (d) { return "translate(" + d.x  + "," + d.y + ")" });	
	}

	function updateNodeSelection(element, isselected) {
		let node = d3.select(element)
			.classed("selected", function(d) { 
				d.selected = isselected;
				return d.selected;
			});
		return node;
	}

	function unselectAllNodes() {
		nodes
			.classed("selected", function(d) { 
				d.selected = false;
				return d.selected; })
			.select(".nodeicon")
				.attr("color", function(d) { return d.color; }); 
	}
}


//based on an angled line, eval all the relevant details
function EdgeLine(x1, y1, x2, y2) {
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
}

// find coordinates of a point along the line from the source (x1,y1)
EdgeLine.prototype.getCoords = function(length)
{
	let ret = {
		x:(this.cosA / length), 
		y:(this.sinA / length)};
	return ret;
}


function ToolTip(nodedatum) {
	this.node = nodedatum;
	this.propertystring = function() {
		let s;

		if (nodedatum.properties) {
			this.propertyCount = nodedatum.properties.count;
			nodedatum.properties.keys(obj).forEach(function(key){
				s += key + ": " + obj[key];
			});
		}
		else {
			this.propertyCount = 0;
			s = "empty";
		}
		return s;
	}
}