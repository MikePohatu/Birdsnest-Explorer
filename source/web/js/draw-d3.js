function DrawGraph(selectid) {
	var defaultsize = 40;
	var defaultstroke = 3;

	var drawPane = d3.select("#"+selectid)
	var vis = drawPane.append("svg");

    var nodedata = [
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"},
		{scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
		{scaling:1.5, class:"fas fa-user", color:"red", fill: "pink"},
		{scaling:1, class:"fas fa-user", color:"green", fill: "lightgreen"}
	];

	var linkdata = [
		{source: nodedata[0], target: nodedata[1]},
		{source: nodedata[2], target: nodedata[5]},
		{source: nodedata[5], target: nodedata[8]},
		{source: nodedata[8], target: nodedata[11]},
		{source: nodedata[11], target: nodedata[14]},
		{source: nodedata[14], target: nodedata[17]}
		];

	spreadNodes(nodedata);

	

	var nodes = vis.selectAll(".nodes")
		.data(nodedata);

	//node drag drop functionality
	var drag_node = d3.drag().subject(this)
		.on('start',function (d) {
			d.click_x = d3.event.x - d.x;  //calculate the difference between where the click is
			d.click_y = d3.event.y - d.y;  //vs where the 0 point of the object 
		})
		.on('drag',function(d){
			d.x = d3.event.x - d.click_x;
			d.y = d3.event.y - d.click_y;

			//move the node
			d3.select(this).attr("transform", "translate(" + d.x  + "," + d.y + ")");	
			updateEdges();	
		});	


	updateEdges();

	//build the nodes
	var g = nodes.enter()
		.append("g")
			.attr("transform",function(d) { 
					return "translate(" + d.x + "," +d.y+")"
				})
			.attr("class","nodes")
			.call(drag_node);

	//node layout
	g.append("svg:circle")
		.attr("r", function(d) { return ((defaultsize * d.scaling)/2) + "px" })
		.attr("cx", function(d) { return ((defaultsize * d.scaling)/2) })
		.attr("cy", function(d) { return ((defaultsize * d.scaling)/2) })
		.style("stroke-width", function(d) { return (defaultstroke * d.scaling) + "px" })
		.attr("fill", function(d) { return d.fill })
		.style("stroke", function(d) { return d.color });
		

	g.append("i")
		.attr("height", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("width", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("x", function(d) { return ((defaultsize * d.scaling) * 0.2) })
		.attr("y",function(d) { return ((defaultsize * d.scaling) * 0.2)})
		.attr("class", function(d) { return d.class })
		.attr("color", function(d) { return d.color }); 

	//simulation/force layout
	const simulation = d3.forceSimulation()
		.force('charge', d3.forceManyBody().strength(-20)) 
		.force('center', d3.forceCenter(640 / 2, 480 / 2));
		
	simulation.nodes(nodes).on('tick', function () {
		console.log("sim stuff and things");
		g.attr("transform",function(d) { 
			return "translate(" + d.x + "," +d.y+")"
		});
		updateEdges();
	});

	//simulation.restart();
	//build edges
	function updateEdges()
	{
		var edges = vis.selectAll("line")
			.data(linkdata);

		edges.exit().remove();
		edges.enter()
			.append("line")
			.attr("class","edges")
			.style("stroke", "rgb(6,120,155)")
			.merge(edges)
				.attr("x1", function(d) { return (d.source.x + ((defaultsize*d.source.scaling))/2)})
				.attr("y1", function(d) { return (d.source.y + ((defaultsize*d.source.scaling))/2) })
				.attr("x2", function(d) { return (d.target.x + ((defaultsize*d.target.scaling))/2)})
				.attr("y2", function(d) { return (d.target.y + ((defaultsize*d.target.scaling))/2) });		
	}
}

function spreadNodes(nodeArray) {
	if (nodeArray.length>0) {
		var gap = 100;
		var countX = 1;
		var countY = 1;
		var maxCount = Math.floor(Math.sqrt(nodeArray.length));;

		nodeArray.forEach(function(node) {
			node.x = countX * gap;
			node.y = countY * gap;
			if (countY == maxCount) { 
				countX++; 
				countY = 1;
			}
			else { countY++; }
		});
	}	
}
