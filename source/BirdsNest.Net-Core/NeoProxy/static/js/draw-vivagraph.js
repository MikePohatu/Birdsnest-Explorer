function DrawGraph(selectid) {
	var graph = Viva.Graph.graph();
	
	graph.addLink(1, 2);

	var graphics = Viva.Graph.View.webglGraphics();
	/*var graphics = Viva.Graph.View.svgGraphics();*/
	
	// specify where it should be rendered:
	var renderer = Viva.Graph.View.renderer(graph, {
			graphics: graphics, 
			container: document.getElementById(selectid)
		});
	renderer.run();
}