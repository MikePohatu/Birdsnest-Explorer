var json = '{\
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
		{"source": 450, "target": 1, "bidir":false},\
		{"source": 3, "target": 2, "bidir":false},\
		{"source": 9112, "target": 61, "bidir":false},\
		{"source": 4, "target": 33, "bidir":false},\
		{"source": 4, "target": 9112, "bidir":false},\
		{"source": 9112, "target": 100, "bidir":false},\
		{"source": 33, "target": 4, "bidir":false},\
		{"source": 15, "target": 33, "bidir":false},\
		{"source": 15, "target": 17, "bidir":false},\
		{"source": 17, "target": 18, "bidir":false}\
	]\
}';

//var jsonData = ""
var simulation=d3.forceSimulation();

function restartLayout(){ 
	simulation.alpha(1);
	simulation.restart();
}

function drawGraph(selectid) {
	var shiftKey = false;
	var ctrlKey = false;

	var defaultsize = 40;
	var defaultstroke = 3;
	var totalnodesize = (defaultstroke*2) + defaultsize;

	var jsonData = JSON.parse(json);
	var nodedata = jsonData.nodes;
	var linkdata = jsonData.edges;
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

	var drawPane = d3.select("#"+selectid)
		.style("fill","gray")
		.on("keydown", keydown)
		.on("keyup", keyup)
		.on("click", clicked);

	var svg = drawPane.append("svg");

	//setup the zooming layer
	var zoomLayer = svg.append("g");
	svg.call(d3.zoom()
		.scaleExtent([0.1, 5])
		.on("zoom", function() {
				zoomLayer.attr("transform", d3.event.transform);
			}));	

	//setup arrows for lines
	var defs = svg.append('svg:defs');
	
	defs.append('svg:marker')
			.attr('id', 'end-arrow')
			.attr('viewBox', '0 -5 10 10')
			.attr('refX', 6)
			.attr('markerWidth', 3)
			.attr('markerHeight', 3)
			.attr('orient', 'auto')
			.append('svg:path')
				.attr('d', 'M0,-5 L10,0 L0,5')
				.attr('fill', "rgb(6,120,155)");

	defs.append('svg:marker')
			.attr('id', 'start-arrow')
			.attr('viewBox', '0 -5 10 10')
			.attr('refX', 6)
			.attr('markerWidth', 3)
			.attr('markerHeight', 3)
			.attr('orient', 'auto')
			.append('svg:path')
				.attr('d', 'M0,0 L10,-5 L10,5')
				.attr('fill', "rgb(6,120,155)");

	//setup the edges
	var edges = zoomLayer.selectAll("line")
		.data(linkdata)
		.enter()
		.append("line")
		.attr("class","edges")
		.style("stroke", "rgb(6,120,155)")
		.style("stroke-width", 2)
		.style('marker-end', "url(#end-arrow)");

	//build the nodes
	var nodes = zoomLayer.selectAll(".nodes")
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

	//setup simulation/force layout
	simulation.nodes(nodedata)
		.force("link", d3.forceLink()
			.id(function(d) { return d.db_id; }))
		.force('charge', d3.forceManyBody().strength(-5)) 
		.force('center', d3.forceCenter(640 / 2, 480 / 2))
		.force('collision', d3.forceCollide().radius(function(d) { return (totalnodesize)}))
		.on('tick', function () {		
			updateLocations();
		});
	
	simulation.velocityDecay(0.5);
	simulation.alphaDecay(0.02)
	simulation.force("link")
		.links(linkdata);

	function nodeDragged(d){
		d3.event.sourceEvent.stopPropagation();
		//if the node is selected the move it and all other selected
		//nodes
		if (d.selected) { 
			nodes.filter(function(d) { return d.selected; })
			.each(function(d) { 
				d.x += d3.event.dx;
				d.y += d3.event.dy;
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
			var sourceX= d.source.x + d.source.cx;
			var sourceY = d.source.y + d.source.cy;
			var targetX = d.target.x + d.target.cx;
			var targetY = d.target.y + d.target.cy;

			// Total difference in x and y from source to target
			var diffX = targetX - sourceX;
			var diffY = targetY - sourceY;

			// Length of path from center of source node to center of target node
			var pathLength = Math.sqrt((diffX * diffX) + (diffY * diffY));

			///sin(angle) - some trig with angle taken opposite the 
			var sinA = diffY / pathLength;
			var cosA = diffX / pathLength;

			d3.select(this)
				.attr("x1", function (d) { return sourceX + (cosA * (d.source.radius+6)); } )
				.attr("y1", function (d) { return sourceY + (sinA * (d.source.radius+6)); } )
				.attr("x2", function(d) { return targetX - (cosA * (d.target.radius+6)); }) //the number adds a little padding
				.attr("y2", function(d) { return targetY - (sinA * (d.target.radius+6)); } );
		});
			
		nodes.attr("x",function(d) { return d.x; })
			.attr("y",function(d) { return d.y; })
			.attr("transform", function (d) { return "translate(" + d.x  + "," + d.y + ")" });	
	}

	function updateNodeSelection(element, isselected) {
		var node = d3.select(element)
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






