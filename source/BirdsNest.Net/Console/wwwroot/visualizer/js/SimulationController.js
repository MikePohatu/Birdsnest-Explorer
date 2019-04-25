// requires D3js

function SimulationController() {
    var me = this;
    var velocityDecay = 0.5;
    var alphaDecay = 0.1;

    this.simRunning = false; 
    this.onFinishSimulation;
    this.ProgressBarTag;
    this.TreeEdgeTag;
    this.NodeTag;
    this.onPercentUpdated;

    this.graphsimulation = d3.forceSimulation();
    this.graphsimulation.stop();
    this.graphsimulation
        .force('collide', d3.forceCollide()
            .strength(0.7)
            .radius(function (d) { return d.size * 1.5; }))
        .on('end', function () { me.onSimulationFinished(); })
        .on('tick', function () { me.onGraphTick(); })
        .velocityDecay(velocityDecay)
        .alphaDecay(alphaDecay);

    this.connectsimulation = d3.forceSimulation();
    this.connectsimulation.stop();
    this.connectsimulation
        .force("link", d3.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(200)
            .strength(0.05))
        .velocityDecay(velocityDecay)
        .alphaDecay(alphaDecay);

    this.meshsimulation = d3.forceSimulation();
    this.meshsimulation.stop();
    this.meshsimulation
        .force("link", d3.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(150))
        .velocityDecay(velocityDecay)
        .alphaDecay(alphaDecay);

    this.treesimulation = d3.forceSimulation();
    this.treesimulation.stop();
    this.treesimulation
        .force("link", d3.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(150))
        .velocityDecay(velocityDecay)
        .alphaDecay(alphaDecay);
}





SimulationController.prototype.onGraphTick = function() {
    //console.log("SimulationController.onGraphTick");
    //console.log(meshsimulation.alpha());
    //var me = this;
    var k = this.graphsimulation.alpha();
    this.onPercentUpdated(100 - k * 100);

    d3.selectAll(this.EdgeTag).each(function (d) {
        if (d.target.y < d.source.y + d.source.size / 4) {
            d.target.y = d.source.y + d.source.size;
        }
        else {
            if (d.target.tark !== k) {
                d.target.y += k * 8;
                d.target.tark = k;
            }
            if (d.source.srck !== k) {
                d.source.y -= k * 6;
                d.source.srck = k;
            }
        }
    });
    //if (!perfmode) { updateLocations(); }
};


SimulationController.prototype.onSimulationFinished = function () {
    //console.log("SimulationController.onSimulationFinished");
    //console.log(this.graphsimulation);
    this.simRunning = false;
    this.onFinishSimulation();
};

SimulationController.prototype.RestartSimulation = function () {
    var me = this;
    //console.log("SimulationController.prototype.RestartSimulation: " + this.NodeTag);
    d3.selectAll(this.NodeTag)
        .each(function (d) {
            if (me.simRunning === false) {
                //console.log("set startx to x: ");
                d.startx = d.x;
                d.starty = d.y;
            }
            else {
                //console.log("set x to startx: ");
                d.x = d.startx;
                d.y = d.starty;
            }
            //console.log(d);
        });
    

    this.simRunning = true;
    this.meshsimulation.alpha(1).restart();
    this.treesimulation.alpha(1).restart();
    this.connectsimulation.alpha(1).restart();
    this.graphsimulation.alpha(1).restart();
};

SimulationController.prototype.StopSimulations = function () {
    this.meshsimulation.stop();
    this.graphsimulation.stop();
    this.treesimulation.stop();
    this.connectsimulation.stop();
};

SimulationController.prototype.SetNodes = function (graphs, meshes, trees, connects) {
    //console.log("SimulationController.SetNodes");
    //console.log(graphs);
    this.meshsimulation.nodes(meshes);
    this.treesimulation.nodes(trees);
    this.connectsimulation.nodes(connects);
    this.graphsimulation.nodes(graphs);
}

SimulationController.prototype.SetEdges = function (meshes, trees, connects) {
    //console.log("SimulationController.SetEdges");
    this.meshsimulation.force("link").links(meshes);
    this.treesimulation.force("link").links(trees);
    this.connectsimulation.force("link").links(connects);
}