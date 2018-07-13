let json = '{\
	"nodes":[\
		{"db_id":450,"name":"Node0", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":21,"name":"Node1", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":42,"name":"Node2", "relatedcount":1, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":3,"name":"Node3", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"},\
		{"db_id":54,"name":"Node4", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":564,"name":"Node5", "relatedcount":1, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":61,"name":"Node6", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"},\
		{"db_id":75,"name":"Node7", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":833,"name":"Node8", "relatedcount":1, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":9112,"name":"Node9", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"},\
		{"db_id":100,"name":"Node10", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":1,"name":"Node11", "relatedcount":1, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":2,"name":"Node12", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"},\
		{"db_id":33,"name":"Node13", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":4,"name":"Node14", "relatedcount":1, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":15,"name":"Node15", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"},\
		{"db_id":16,"name":"Node16", "relatedcount":1, "class":"fas fa-user", "color":"orange", "selcolor":"lightgreen", "fill": "yellow"},\
		{"db_id":17,"name":"Node17", "relatedcount":1.5, "class":"fas fa-user", "color":"red", "selcolor":"lightgreen", "fill": "pink"},\
		{"db_id":18,"name":"Node18", "relatedcount":1, "class":"fas fa-user", "color":"green", "selcolor":"lightgreen", "fill": "lightgreen"}\
	],\
	"edges":[\
		{"source": 450, "target": 1, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 3, "target": 2, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 9112, "target": 61, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 4, "target": 33, "bidir":true, "label":"AD_MemberOf", "color":"green"},\
		{"source": 4, "target": 9112, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 9112, "target": 100, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 15, "target": 33, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 15, "target": 17, "bidir":false, "label":"AD_MemberOf", "color":"green"},\
		{"source": 17, "target": 18, "bidir":false, "label":"AD_MemberOf", "color":"green"}\
	]\
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

	let bgcolor = "#FBFBFB";
	let paneWidth = 640;
	let paneHeight = 480;
	let defaultsize = 40;
	let defaultstroke = 3;
	let totalnodesize = (defaultstroke*2) + defaultsize;
	let edgelabelwidth = 70;

	let jsonData = JSON.parse(json);
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
			d.totalsize = totalnodesize * d.scaling;
		});	

	//setup simulation/force layout, bind links to the correct node property etc
	simulation.nodes(nodedata)
		.force("link", d3.forceLink()
			.id(function(d) { return d.db_id; }))
		.force('charge', d3.forceManyBody().strength(-5)) 
		.force('center', d3.forceCenter(paneWidth / 2, paneHeight / 2))
		.force('collision', d3.forceCollide().radius(function(d) { return (totalnodesize)}))
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
		.style("background-color",bgcolor);

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
			.classed("edges",true);

	edges.append("path")
		.classed("wrapper",true)
		.attr("fill","none");

	edges.append("path")
		.classed("arrows",true)
		.attr("fill",function(d){ return d.color;});	
	
	let edgelabels = edges.append("g")
		.classed("edgelabel",true)
		.style("transform-origin","center");

/*	edgelabels.append("rect")
		.attr("height",14)
		.attr("width",edgelabelwidth)
		.style("stroke","black")
		.style("fill",bgcolor);*/

	edgelabels.append("text")
		.attr("alignment-baseline","center")
		.attr("y",2)
		.attr("x",5)
		.attr("font-size","8px")
		.attr("font-family","arial")
		.text(function(d) {return d.label});

	//build the nodes
	let nodes = zoomLayer.selectAll(".nodes")
		.data(nodedata)
		.enter()
		.append("g")
			.classed("nodes",true)
			.classed("selected", function(d) { 
				d.selected = false; 
				d.previouslySelected = false; 
				return d.selected;})
			.on("click", nodeClicked)
			.call(
				d3.drag().subject(this)
					.on('drag',nodeDragged));

	//node layout
	nodes.append("circle")
		.attr("r", function(d) { return d.radius + "px"; })
		.attr("cx", function(d) { return d.radius; })
		.attr("cy", function(d) { return d.radius; })
		.style("stroke-width", function(d) { return defaultstroke + "px" })
		.attr("fill", function(d) { return d.fill })
		.style("stroke", function(d) { return d.color });
		
	nodes.append("i")
		.attr("height", function(d) { return ( d.size * 0.6) + "px" })
		.attr("width", function(d) { return (d.size * 0.6) + "px" })
		.attr("x", function(d) { return (d.size * 0.2) })
		.attr("y",function(d) { return (d.size * 0.2)})
		.attr("class", function(d) { return d.class })
		.attr("color", function(d) { return d.color })
		.classed("nodeicon",true); 

	nodes.append("text")
		.text(function(d) { return d.name; })
		.attr("text-anchor","left")
		.attr("font-size","10px")
		.attr("font-family","arial")
		.attr("transform",function(d) { return "translate(" + (d.totalsize + 2) 
			+ "," + ((d.totalsize /2) + 2) + ")" }); 

	function nodeDragged(d){
		d3.event.sourceEvent.stopPropagation();
		//if the node is selected the move it and all other selected
		//nodes
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
			let diagLine = new EdgeLine(d.source.cx,d.source.cy,d.target.cx,d.target.cy);

			let arrowLength = diagLine.hypLen;
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
						"L "+ (diagLine.hypLen) + " -3 "+
						"L "+ (diagLine.hypLen) + " 3 "+ 
						"L 0 3 Z"; 
				});

			edge.selectAll(".arrows")
				.attr("d",function(d) {
					return "M 0 -1 " + 
						"L "+ (arrowLength - 5) + " -1 " + 
						"L "+ (arrowLength - 5) + " -3 " + 
						"L "+ (arrowLength) + " 0 "+
						"L "+ (arrowLength - 5) + " 3 "+ 
						"L "+ (arrowLength - 5) +" 1 "+
						"L 0 1 Z"; 
				});

			edge.selectAll(".edgelabel")
				.attr("transform",function(d) { 
					if (diagLine.x2 > diagLine.x1) {
						return "translate(" + (d.source.radius + (arrowLength/2)-(edgelabelwidth/2)) + ",-7)";
					}
					else {
						return "rotate(180) translate(" + (d.source.radius - 
							(arrowLength/2)+(edgelabelwidth/2)) + ",-7)";
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
				d.previouslySelected = isselected;
				return d.selected;
			})
			.select(".nodeicon")
				.attr("color", function(d) { return isselected ? "#3C3C3C" : d.color; });
		return node;
	}

	function unselectAllNodes() {
		nodes
			.classed("selected", function(d) { 
				d.selected = false;
				d.previouslySelected = false;
				return d.selected; })
			.select(".nodeicon")
				.attr("color", function(d) { return d.color; }); 
	}
}


//based on an angled line, eval all the relevant details so we can
// find coordinates of a point along the line from the source x1,y1,
// and the angle of the line
function EdgeLine(x1, y1, x2, y2) {
	this.x1 = x1;
	this.y1 = y1;
	this.x2 = x2;
	this.y2 = y2;
	this.xd = x2 - x1; //delta x
	this.yd = y2 - y1; //delta y

	this.hypLen = Math.sqrt((this.xd * this.xd) + (this.yd * this.yd));
	this.deg = Math.atan2(this.yd, this.xd) * (180 / Math.PI);
	this.sinA = this.yd / this.hypLen;
	this.cosA = this.xd / this.hypLen;	
}

EdgeLine.prototype.getCoords = function(length)
{
	let ret;
	ret.x = this.cosA / length;
	ret.y = this.sinA / length;
	return ret;
}