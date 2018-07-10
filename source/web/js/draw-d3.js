var nodedata = [
	{name:"Node0", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node1", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node2", scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node3", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
	{name:"Node4", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node5", scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node6", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
	{name:"Node7", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node8", scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node9", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
	{name:"Node10", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node11", scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node12", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
	{name:"Node13", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node14", scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node15", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
	{name:"Node16", scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
	{name:"Node17", scaling:1.5, class:"fas fa-user", color:"red", fill: "pink"},
	{name:"Node18", scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"}
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
	var defaultsize = 40;
	var defaultstroke = 3;

	var drawPane = d3.select("#"+selectid)
	var vis = drawPane.append("svg");

    

	var edges = vis.selectAll("line")
			.data(linkdata)
			.enter()
			.append("line")
			.attr("class","edges")
			.style("stroke", "rgb(6,120,155)");

	//build the nodes
	var nodes = vis.selectAll(".nodes")
		.data(nodedata)
		.enter()
		.append("g")
			.attr("class","nodes")
			.call(
				d3.drag().subject(this)
					.on('start',nodeDragStart)
					.on('drag',nodeDrag));

	//node layout
	nodes.append("svg:circle")
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
		.attr("color", function(d) { return d.color }); 

	nodes.append("text")
		.text(function(d) { return d.name; }); 
	
	//setup simulation/force layout
	simulation.nodes(nodedata)
		.force("link", d3.forceLink().id(function(d) { return d.id; }))
		.force('charge', d3.forceManyBody().strength(-5)) 
		.force('center', d3.forceCenter(640 / 2, 480 / 2))
		.force('collision', d3.forceCollide().radius(function(d) { return ((defaultsize * d.scaling))}))
		.on('tick', function () {		
			update();
		});
	
	simulation.velocityDecay(0.5);
	simulation.alphaDecay(0.02)
	simulation.force("link")
      .links(linkdata);

	function nodeDragStart(d){
		d.click_x = d3.event.x - d.x;  //calculate the difference between where the click is
		d.click_y = d3.event.y - d.y;  //vs where the 0 point of the object 
	}

	function nodeDrag(d){
		d.x = d3.event.x - d.click_x;
		d.y = d3.event.y - d.click_y;
		d.fx = d.x;
		d.fy = d.y;	
		update();
	}

	function pageDragStart(d){
		d.click_x = d3.event.x;  //calculate the difference between where the click is
		d.click_y = d3.event.y;  //vs where the 0 point of the object 
	}

	function pageDrag(d){
		d.x = d3.event.x - d.click_x;
		d.y = d3.event.y - d.click_y;
		update();
	}

	function update() {
		edges.attr("x1", function(d) { return (d.source.x + ((defaultsize*d.source.scaling))/2)})
			.attr("y1", function(d) { return (d.source.y + ((defaultsize*d.source.scaling))/2) })
			.attr("x2", function(d) { return (d.target.x + ((defaultsize*d.target.scaling))/2)})
			.attr("y2", function(d) { return (d.target.y + ((defaultsize*d.target.scaling))/2) });

		nodes.attr("x",function(d) { return d.x; })
			.attr("y",function(d) { return d.y; })
			.attr("transform", function (d) { return "translate(" + d.x  + "," + d.y + ")" });	
	}
}
