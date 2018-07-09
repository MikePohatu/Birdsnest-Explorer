function DrawGraph(selectid) {
	var defaultsize = 40;
	var defaultstroke = 3;

	var vis = d3.select("#"+selectid)
            .append("svg");

    var nodedata = [{x: 30, y: 50, scaling:1, class:"fas fa-user", color:"orange", fill: "yellow"},
              {x: 300, y: 110, scaling:1.5, class:"fas fa-user", color:"red", fill: "pink"},
              {x: 150, y: 200, scaling:0.6, class:"fas fa-user", color:"green", fill: "lightgreen"}];

	var linkdata = [
		{source: nodedata[0], target: nodedata[1]},
		{source: nodedata[2], target: nodedata[1]}
		];

	

	var nodes = vis.selectAll(".nodes")
		.data(nodedata);

	//build the edges
	function updateEdges()
	{
		var edges = vis.selectAll("line")
			.data(linkdata);

		edges.exit().remove();
		edges.enter()
			.append("line")
			.style("stroke", "rgb(6,120,155)")
			.merge(edges)
			.attr("x1", function(d) { return (d.source.x + ((defaultsize*d.source.scaling))/2)})
			.attr("y1", function(d) { return (d.source.y + ((defaultsize*d.source.scaling))/2) })
			.attr("x2", function(d) { return (d.target.x + ((defaultsize*d.target.scaling))/2)})
			.attr("y2", function(d) { return (d.target.y + ((defaultsize*d.target.scaling))/2) });
		
	}

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
	g = nodes.enter().
		append("g")
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
}
