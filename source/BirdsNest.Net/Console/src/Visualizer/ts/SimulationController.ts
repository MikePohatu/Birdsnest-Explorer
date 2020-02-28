// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
import * as d3 from "../js/visualizerD3";

interface ISimNode extends d3.SimulationNodeDatum {
    size: number;
    db_id: string;
    y: number;
    x: number;
    startx: number;
    starty: number;
    tark: number;
    srck: number;
}

interface ISimLink<ISimNode> extends d3.SimulationLinkDatum<ISimNode> {
    db_id: string;
    source: ISimNode;
    target: ISimNode;
}

export default class SimulationController {
    simRunning: boolean;
    onFinisedSimulation: CallableFunction;
    onFinishSimulation: CallableFunction;
    ProgressBarTag: string;
    TreeEdgeTag: string;
    NodeTag: string;
    onPercentUpdated: CallableFunction;
    graphsimulation: any;
    connectsimulation: any;
    meshsimulation: any;
    treesimulation: any;

    constructor() {
        var me = this;
        this.simRunning = false;
        

        this.graphsimulation = d3.forceSimulation();
        this.graphsimulation.stop();
        this.graphsimulation
            .force('collide', d3.forceCollide()
                .strength(0.9)
                .radius(function (d: ISimNode) { return d.size * 1.5; }))
            .on('end', function () { me.onSimulationFinished(); })
            .on('tick', function () { me.onGraphTick(); });

        this.connectsimulation = d3.forceSimulation();
        this.connectsimulation.stop();
        this.connectsimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(200)
                .strength(0.3));

        this.meshsimulation = d3.forceSimulation();
        this.meshsimulation.stop();
        this.meshsimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(200)
                .strength(0.6))
            .force("manybody", d3.forceManyBody()
                .strength(-30)
                .distanceMin(3)
                .distanceMax(1000));

        this.treesimulation = d3.forceSimulation();
        this.treesimulation.stop();
        this.treesimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(200)
                .strength(0.8));
    }

    onGraphTick () {
        //console.log("SimulationController.onGraphTick");       
        var k = this.graphsimulation.alpha();
        //console.log(k);
        this.onPercentUpdated(100 - k * 100);


        //check the tree nodes and shunt up or down to get into a tree layout. Exit the function if 
        //node is fixed i.e. has a fy (fixed y) property (https://github.com/d3/d3-force)
        d3.selectAll(this.TreeEdgeTag).each(function (d: ISimLink<ISimNode>) {
            var src = d.source as ISimNode;
            var tar = d.target as ISimNode;
            if (tar.y < src.y + src.size / 4) {
                if (tar.hasOwnProperty("fy")) { return; }
                tar.y = src.y + src.size;
            }
            else {
                if (tar.tark !== k) {
                    if (tar.hasOwnProperty("fy")) { return; }
                    tar.y += k * 8;
                    tar.tark = k;
                }
                if (src.srck !== k) {
                    if (src.hasOwnProperty("fy")) { return; }
                    src.y -= k * 6;
                    src.srck = k;
                }
            }
        });
    }


    onSimulationFinished () {
        //console.log("SimulationController.onSimulationFinished");
        //console.log(this.graphsimulation);
        this.simRunning = false;
        this.onFinishSimulation();
    }

    RestartSimulation () {
        var me = this;
        //console.log("RestartSimulation: " + this.NodeTag);
        d3.selectAll(this.NodeTag)
            .each(function (d: ISimNode) {
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
        //console.log("restarting simulation now");
        this.graphsimulation.alpha(1).restart();
        this.meshsimulation.alpha(1).restart();
        this.treesimulation.alpha(1).restart();
        this.connectsimulation.alpha(1).restart();       
    }

    StopSimulations () {
        if (this.simRunning === true) {
            this.meshsimulation.stop();
            this.graphsimulation.stop();
            this.treesimulation.stop();
            this.connectsimulation.stop();

            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            d3.selectAll(this.NodeTag)
                .each(function (d: ISimNode) {
                    //console.log("set x to startx: ");
                    d.x = d.startx;
                    d.y = d.starty;
                });
        }

        this.simRunning = false;
    }

    SetNodes (graphs, meshes, trees, connects) {
        //console.log("SimulationController.SetNodes");
        //console.log(graphs);
        this.StopSimulations();
        this.meshsimulation.nodes(meshes);
        this.treesimulation.nodes(trees);
        this.connectsimulation.nodes(connects);
        this.graphsimulation.nodes(graphs);

        let velocityDecay = 0.5;
        let alphaDecay = 0.1;
        

        if (this.graphsimulation.nodes().length > 3000) {
            velocityDecay = 0.2;
            alphaDecay = 0.5;
        }
        else if (this.graphsimulation.nodes().length > 1000) {
            velocityDecay = 0.35;
            alphaDecay = 0.3;
        }

        //console.log("velocityDecay: " + velocityDecay);
        //console.log("alphaDecay: " + alphaDecay);

        this.graphsimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.connectsimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.meshsimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.treesimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
    }

    SetEdges (meshes, trees, connects) {
        //console.log("SimulationController.SetEdges");
        this.StopSimulations();
        this.meshsimulation.force("link").links(meshes);
        this.treesimulation.force("link").links(trees);
        this.connectsimulation.force("link").links(connects);
    }
}