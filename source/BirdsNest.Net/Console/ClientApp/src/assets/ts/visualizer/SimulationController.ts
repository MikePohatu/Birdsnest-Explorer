// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
import { d3 } from "./d3";
import { SimLink } from "./SimLink";
import { SimNode } from "./SimNode";
import { graphData } from './GraphData';
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import store from "@/store/index";




export default class SimulationController {
    onFinishSimulation: () => void;
    private graphsimulation;
    private connectsimulation;
    private meshsimulation;
    private treesimulation;

    constructor() {
        store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);


        this.graphsimulation = d3.forceSimulation();
        this.graphsimulation.stop();
        this.graphsimulation
            .force('collide', d3.forceCollide()
                .strength(0.9)
                .radius(function (d: SimNode) { return d.size + 20; }))
            .on('end', () => { this.onSimulationFinished(); })
            .on('tick', () => { this.onGraphTick(); });

        this.connectsimulation = d3.forceSimulation();
        this.connectsimulation.stop();
        this.connectsimulation
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; }));

        this.meshsimulation = d3.forceSimulation();
        this.meshsimulation.stop();
        this.meshsimulation
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; }))
            .force("manybody", d3.forceManyBody()
                .strength(-50));

        this.treesimulation = d3.forceSimulation();
        this.treesimulation.stop();
        this.treesimulation
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; })
                .strength(1));
    }

    onGraphTick() {
        //console.log("SimulationController.onGraphTick");       
        const k = this.graphsimulation.alpha();
        //console.log(k);
        store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100 - k * 100);


        //check the tree nodes and shunt up or down to get into a tree layout. Exit the function if 
        //node is fixed i.e. has a fy (fixed y) property (https://github.com/d3/d3-force)
        this.treesimulation.force("link").links().forEach((d: SimLink<SimNode>) => {
            const src = d.source as SimNode;
            const tar = d.target as SimNode;

            //console.log(src + ':' + tar);
            if (tar.y < src.y + src.size / 2) {
                if (Object.prototype.hasOwnProperty.call(tar, "fy")) { return; }
                tar.y = src.y + src.size;
            }
            else {
                if ((tar.tark !== k) && (Object.prototype.hasOwnProperty.call(tar, "fy") === false)) {
                    tar.y += k * 8;
                    tar.tark = k;
                }
                if ((src.srck !== k) && (Object.prototype.hasOwnProperty.call(src, "fy")===false)) {
                    src.y -= k * 6;
                    src.srck = k;
                }
            }
        });
    }


    onSimulationFinished() {
        //console.log("SimulationController.onSimulationFinished");
        store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
        store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100);

        this.onFinishSimulation(); //callback
    }

    RefreshData() {
        this.StopSimulations();

        this.SetNodes(
            graphData.graphNodes.GetArray(),
            graphData.meshNodes.GetArray(),
            graphData.treeNodes.GetArray(),
            graphData.connectNodes.GetArray()
        );
        this.SetEdges(
            graphData.meshEdges.GetArray(),
            graphData.treeEdges.GetArray(),
            graphData.connectEdges.GetArray()
        );
    }


    RestartSimulation() {
        if (store.state.visualizer.simRunning === true) {
            this.StopSimulations();
        }

        store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, true);

        this.graphsimulation.nodes().forEach((d: SimNode) => {
            d.startx = d.x;
            d.starty = d.y;
        });
        
        //console.log("restarting simulation now");
        this.graphsimulation.alpha(1).restart();
        this.meshsimulation.alpha(1).restart();
        this.treesimulation.alpha(1).restart();
        this.connectsimulation.alpha(1).restart();
    }

    StopSimulations() {
        //console.trace("StopSimulations");
        if (store.state.visualizer.simRunning === true) {
            this.meshsimulation.stop();
            this.graphsimulation.stop();
            this.treesimulation.stop();
            this.connectsimulation.stop();

            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            this.graphsimulation.nodes().forEach((d: SimNode) => {
                //console.log("set x to startx: ");
                d.x = d.startx;
                d.y = d.starty;
            });
        }
        
        store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
    }

    private SetNodes(graphs, meshes, trees, connects) {
        //console.log("SimulationController.SetNodes");
        //console.log(graphs);
        
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

    private SetEdges(meshes, trees, connects) {
        //console.log("SimulationController.SetEdges");
        this.meshsimulation.force("link").links(meshes);
        this.treesimulation.force("link").links(trees);
        this.connectsimulation.force("link").links(connects);
    }

    clear() {
        this.SetNodes([],[],[],[]);
        this.SetEdges([],[],[]);
    }
}