function DrawGraph(selectid) {
	var defaultsize = 40;
	var defaultstroke = 3;

	var vis = d3.select("#"+selectid)
            .append("svg");

    var nodes = [{x: 30, y: 50, scaling:1},
              {x: 300, y: 110, scaling:1.5},
              {x: 150, y: 200, scaling:0.6}];

	var links = [
		{source: nodes[0], target: nodes[1]},
		{source: nodes[2], target: nodes[1]}
		];

	var drag_node = d3.drag().subject(this)
		.on('start',function (d) {
			d.xdiff = d3.event.x - d.x;  //calculate the difference between where the click is
			d.ydiff = d3.event.y - d.y;  //vs where the 0 point of the object 
		})
		.on('drag',function(d){
			d.x = d3.event.x - d.xdiff;
			d.y = d3.event.y - d.ydiff;

			d3.select(this)
			.attr("transform", "translate(" + d.x  + "," + d.y + ")");
		});	

	vis.selectAll(".line")
		.data(links)
		.enter()
		.append("line")
			.attr("x1", function(d) { return (d.source.x + ((defaultsize*d.source.scaling))/2)})
			.attr("y1", function(d) { return (d.source.y + ((defaultsize*d.source.scaling))/2) })
			.attr("x2", function(d) { return (d.target.x + ((defaultsize*d.target.scaling))/2)})
			.attr("y2", function(d) { return (d.target.y + ((defaultsize*d.target.scaling))/2) })
			.style("stroke", "rgb(6,120,155)");

	g = vis.selectAll(".nodes")
		.data(nodes)
		.enter()
			.append("g")
				.attr("transform",function(d) { 
						return "translate(" + d.x + "," +d.y+")"
					})
				.attr("class","nodes")
				.call(drag_node);

	g.append("svg:circle")
		.attr("r", function(d) { return ((defaultsize * d.scaling)/2) + "px" })
		.attr("cx", function(d) { return ((defaultsize * d.scaling)/2) })
		.attr("cy", function(d) { return ((defaultsize * d.scaling)/2) })
		.style("stroke-width", function(d) { return (defaultstroke * d.scaling) + "px" })
		.attr("fill", "yellow")
		.style("stroke", "orange");
		

	g.append("i")
		.attr("height", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("width", function(d) { return ((defaultsize * d.scaling) * 0.6) + "px" })
		.attr("x", function(d) { return ((defaultsize * d.scaling) * 0.2) })
		.attr("y",function(d) { return ((defaultsize * d.scaling) * 0.2)})
		.attr("class","fas fa-user")
		.attr("color","orange"); 

	
}
