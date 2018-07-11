var nodedata = [
	{name:"Node0", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node1", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node2", scaling:1, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node3", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"},
	{name:"Node4", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node5", scaling:1, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node6", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"},
	{name:"Node7", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node8", scaling:1, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node9", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"},
	{name:"Node10", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node11", scaling:1, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node12", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"},
	{name:"Node13", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node14", scaling:1, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node15", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"},
	{name:"Node16", scaling:1, class:"fas fa-user", color:"orange", selcolor:"lightgreen", fill: "yellow"},
	{name:"Node17", scaling:1.5, class:"fas fa-user", color:"red", selcolor:"lightgreen", fill: "pink"},
	{name:"Node18", scaling:1, class:"fas fa-user", color:"green", selcolor:"lightgreen", fill: "lightgreen"}
];

var linkdata = [
	{source: nodedata[0], target: nodedata[1]},
	{source: nodedata[2], target: nodedata[5]},
	{source: nodedata[5], target: nodedata[8]},
	{source: nodedata[8], target: nodedata[11]},
	{source: nodedata[11], target: nodedata[14]},
	{source: nodedata[6], target: nodedata[11]},
	{source: nodedata[0], target: nodedata[5]},
	{source: nodedata[5], target: nodedata[14]},
	{source: nodedata[14], target: nodedata[17]},
	{source: nodedata[17], target: nodedata[18]}
	];

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

	//setup the edges
	var edges = zoomLayer.selectAll("line")
		.data(linkdata)
		.enter()
		.append("line")
		.attr("class","edges")
		.style("stroke", "rgb(6,120,155)")
		.style("stroke-width", 2);

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
		.attr("r", function(d) { return ((defaultsize * d.scaling)/2) + "px" })
		.attr("cx", function(d) { return ((defaultsize * d.scaling)/2) })
		.attr("cy", function(d) { return ((defaultsize * d.scaling)/2) })
		.style("stroke-width", function(d) { return (defaultstroke * d.scaling) + "px" })
		.attr("fill", function(d) { return d.fill })
		.style("stroke", function(d) { return d.color });
		

	nodes.append("i")
		.attr("height", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("width", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("x", function(d) { return ((defaultsize * d.scaling) * 0.2) })
		.attr("y",function(d) { return ((defaultsize * d.scaling) * 0.2)})
		.attr("class", function(d) { return d.class })
		.attr("color", function(d) { return d.color })
		.classed("nodeicon",true); 

	nodes.append("text")
		.text(function(d) { return d.name; }); 
	
	//setup simulation/force layout
	simulation.nodes(nodedata)
		.force("link", d3.forceLink().id(function(d) { return d.id; }))
		.force('charge', d3.forceManyBody().strength(-5)) 
		.force('center', d3.forceCenter(640 / 2, 480 / 2))
		.force('collision', d3.forceCollide().radius(function(d) { return ((defaultsize * d.scaling))}))
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
			updateNode(this, !(d.selected));
		}
		else {
			//if the ctrl key isn't down, unselect everything and select the node
			unselectAllNodes();
			updateNode(this, true);
		} 
	}

	function updateLocations() {
		edges.attr("x1", function(d) { return (d.source.x + ((defaultsize*d.source.scaling))/2)})
			.attr("y1", function(d) { return (d.source.y + ((defaultsize*d.source.scaling))/2) })
			.attr("x2", function(d) { return (d.target.x + ((defaultsize*d.target.scaling))/2)})
			.attr("y2", function(d) { return (d.target.y + ((defaultsize*d.target.scaling))/2) });

		nodes.attr("x",function(d) { return d.x; })
			.attr("y",function(d) { return d.y; })
			.attr("transform", function (d) { return "translate(" + d.x  + "," + d.y + ")" });	
	}

	function updateNode(element, isselected) {
		//update the node selection
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


