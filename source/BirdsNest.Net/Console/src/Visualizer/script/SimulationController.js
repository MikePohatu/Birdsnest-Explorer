"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
//import d3 from 'd3';
var d3_force_1 = require("d3-force");
var d3_selection_1 = require("d3-selection");
var SimulationController = /** @class */ (function () {
    function SimulationController() {
        var me = this;
        var velocityDecay = 0.5;
        var alphaDecay = 0.1;
        this.simRunning = false;
        this.graphsimulation = d3_force_1.forceSimulation();
        this.graphsimulation.stop();
        this.graphsimulation
            .force('collide', d3_force_1.forceCollide()
            .strength(0.7)
            .radius(function (d) { return d.size * 1.5; }))
            .on('end', function () { me.onSimulationFinished(); })
            .on('tick', function () { me.onGraphTick(); })
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
        this.connectsimulation = d3_force_1.forceSimulation();
        this.connectsimulation.stop();
        this.connectsimulation
            .force("link", d3_force_1.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(200)
            .strength(0.05))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
        this.meshsimulation = d3_force_1.forceSimulation();
        this.meshsimulation.stop();
        this.meshsimulation
            .force("link", d3_force_1.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(150))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
        this.treesimulation = d3_force_1.forceSimulation();
        this.treesimulation.stop();
        this.treesimulation
            .force("link", d3_force_1.forceLink()
            .id(function (d) { return d.db_id; })
            .distance(150))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
    }
    SimulationController.prototype.onGraphTick = function () {
        //console.log("SimulationController.onGraphTick");
        //console.log(meshsimulation.alpha());
        //var me = this;
        var k = this.graphsimulation.alpha();
        this.onPercentUpdated(100 - k * 100);
        d3_selection_1.selectAll(this.EdgeTag).each(function (d) {
            var src = d.source;
            var tar = d.target;
            if (tar.y < src.y + src.size / 4) {
                tar.y = src.y + src.size;
            }
            else {
                if (tar.tark !== k) {
                    tar.y += k * 8;
                    tar.tark = k;
                }
                if (src.srck !== k) {
                    src.y -= k * 6;
                    src.srck = k;
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
        //console.log("RestartSimulation: " + this.NodeTag);
        d3_selection_1.selectAll(this.NodeTag)
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
        if (this.simRunning === true) {
            this.meshsimulation.stop();
            this.graphsimulation.stop();
            this.treesimulation.stop();
            this.connectsimulation.stop();
            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            d3_selection_1.selectAll(this.NodeTag)
                .each(function (d) {
                //console.log("set x to startx: ");
                d.x = d.startx;
                d.y = d.starty;
            });
        }
        this.simRunning = false;
    };
    SimulationController.prototype.SetNodes = function (graphs, meshes, trees, connects) {
        //console.log("SimulationController.SetNodes");
        //console.log(graphs);
        this.StopSimulations();
        this.meshsimulation.nodes(meshes);
        this.treesimulation.nodes(trees);
        this.connectsimulation.nodes(connects);
        this.graphsimulation.nodes(graphs);
    };
    SimulationController.prototype.SetEdges = function (meshes, trees, connects) {
        //console.log("SimulationController.SetEdges");
        this.StopSimulations();
        this.meshsimulation.force("link").links(meshes);
        this.treesimulation.force("link").links(trees);
        this.connectsimulation.force("link").links(connects);
    };
    return SimulationController;
}());
exports.default = SimulationController;
//# sourceMappingURL=SimulationController.js.map