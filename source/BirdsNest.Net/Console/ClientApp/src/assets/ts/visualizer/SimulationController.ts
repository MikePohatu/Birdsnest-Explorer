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

import * as d3 from 'd3';
import { SimLink } from "./SimLink";
import { SimNode } from "./SimNode";
import { graphData } from './GraphData';
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { useStore } from "@/store";
import Spiral from './Spiral';



export default class SimulationController {
    
    onFinishCallback: () => void;
    private spiral = new Spiral(40);
    private store = useStore();
    private meshsimulation;
    private treesimulation;
    private graphsimulation;
    private tempfix: SimNode[] = []; //nodes temporarily fixed in place

    constructor() {
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);

        this.graphsimulation = d3.forceSimulation();
        this.graphsimulation.stop();
        this.graphsimulation   
            .force('collide', d3.forceCollide()
                .strength(0.8)
                .radius(function (d: SimNode) { return d.size + 30; }))
            .on('end', () => {
                this.onSimulationFinished();
            });

        this.treesimulation = d3.forceSimulation();
        this.treesimulation.stop();
        this.treesimulation
            .force('collide', d3.forceCollide()
                .strength(0.4)
                .radius(function (d: SimNode) { return d.size + 20; }))
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; })
                .strength(0.5))
            .on('end', () => {
                this.onTreeSimulationFinished();
            })
            .on('tick', () => { 
                this.onGraphTick(); 
                this.onTreeTick(); 
            });

        this.meshsimulation = d3.forceSimulation();
        this.meshsimulation.stop();
        this.meshsimulation           
            .force('collide', d3.forceCollide()
                .strength(0.4)
                .radius(function (d: SimNode) { return d.size + 20; }))
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; }))
            .force("manybody", d3.forceManyBody()
                .strength(50))
            .on('tick', () => { 
                this.onGraphTick(); 
            })
            .on('end', () => {
                this.onSimulationFinished();
            });
    }

    onGraphTick() {
        //console.log("SimulationController.onGraphTick");       
        const k = (this.meshsimulation.alpha() + this.treesimulation.alpha() + this.graphsimulation.alpha()) / 3;
        //console.log(k);
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100 - k * 100);
    }

    onTreeTick() {
        //check the tree nodes and shunt up or down to get into a tree layout. Exit the function if 
        //node is fixed i.e. has a fy (fixed y) property (https://github.com/d3/d3-force)   
        const k = this.treesimulation.alpha();
        this.treesimulation.force("link").links().forEach((d: SimLink<SimNode>) => {
            const src = d.source as SimNode;
            const tar = d.target as SimNode;

            //set src.fy it to fix it in place so it doesn't 'float up'
            //release tar so it can 'float down'. in the end only the root should remain
            //fixed
            src.fy = src.y;
            if (tar.pinned === false && Object.prototype.hasOwnProperty.call(tar, "fy"))
            {
                delete tar.fy;
            }
            
            //console.log(src + ':' + tar);
            if (tar.y < src.y + src.size) {
                if (Object.prototype.hasOwnProperty.call(tar, "fy")) { return; }
                this.spiral.Step();
                //spiral the x when moving to level with src so it doesn't all end up in a line
                //to one side of src
                tar.x = src.x + this.spiral.x; 
                tar.y = src.y + src.size;
            }
            else {
                if ((tar.tark !== k) && (Object.prototype.hasOwnProperty.call(tar, "fy") === false)) {
                    tar.y += k * 8;
                    tar.tark = k;
                }
            }
        });
    }

    onTreeSimulationFinished() {
        this.treesimulation.force("link").links().forEach((d: SimLink<SimNode>) => {
            const src = d.source as SimNode;
            const tar = d.target as SimNode;
            this.spiral.Reset();

            if (d.enabled) {
                if (tar.pinned === false) {
                    this.tempfix.push(tar);
                    tar.fy = tar.y;
                    tar.fx = tar.x;
                }
                if (src.pinned === false) {
                    this.tempfix.push(src);
                    src.fy = src.y;
                    src.fx = src.x;
                }
            }
        });
        
        this.meshsimulation.restart();
    }

    onSimulationFinished() {
        //console.log("SimulationController.onSimulationFinished");
        if (this.isRunning(this.meshsimulation) || this.isRunning(this.graphsimulation)) { return; } //wait for both to finish
        this.tempfix.forEach((node)=> {
            delete node.fy;
            delete node.fx;
        });
        this.tempfix = [];
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100);

        this.onFinishCallback(); //callback
    }

    private isRunning(sim): boolean {
        if (sim.alpha() < sim.alphaMin()) { return false; }
        else { return true;}
    }

    RefreshData(): SimulationController {
        this.StopSimulations();

        this.SetNodes(
            graphData.graphNodes.Array,
            graphData.meshNodes.Array,
            graphData.treeNodes.Array
        );
        this.SetEdges(
            graphData.meshEdges.Array,
            graphData.treeEdges.Array
        );
        return this;
    }

    RestartSimulation(treeonly?: boolean) {
        if (this.store.state.visualizer.simRunning === true) {
            this.StopSimulations();
        }

        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, true);

        graphData.graphNodes.Array.forEach((d: SimNode) => {
            d.startx = d.x;
            d.starty = d.y;
        });
        
        //console.log("restarting simulation now");
        //mesh simulation restarts when tree is finished
        this.meshsimulation.alpha(1);
        this.treesimulation.alpha(1).restart();
        this.graphsimulation.alpha(1).restart();
    }

    StopSimulations() {
        //console.trace("StopSimulations");
        if (this.store.state.visualizer.simRunning === true) {
            this.meshsimulation.stop();
            this.treesimulation.stop();  
            this.graphsimulation.stop();

            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            graphData.graphNodes.Array.forEach((d: SimNode) => {
                //console.log("set x to startx: ");
                d.x = d.startx;
                d.y = d.starty;
            });
        }
        this.tempfix = [];
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
    }

    private SetNodes(allnodes, meshes, trees) {
        //console.log("SimulationController.SetNodes");
        //console.log(graphs);
        
        this.meshsimulation.nodes(meshes);
        this.treesimulation.nodes(trees);
        this.graphsimulation.nodes(allnodes);

        let velocityDecay = 0.5;
        let alphaDecay = 0.1;


        if (graphData.graphNodes.Array.length > 3000) {
            velocityDecay = 0.2;
            alphaDecay = 0.5;
        }
        else if (graphData.graphNodes.Array.length > 1000) {
            velocityDecay = 0.35;
            alphaDecay = 0.3;
        }

        this.graphsimulation
            .velocityDecay(velocityDecay * 0.8)
            .alphaDecay(alphaDecay / 2);

        this.meshsimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.treesimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
    }

    private SetEdges(meshes, trees) {
        //console.log("SimulationController.SetEdges");
        this.meshsimulation.force("link").links(meshes);
        this.treesimulation.force("link").links(trees);
    }

    clear() {
        this.SetNodes([],[],[]);
        this.SetEdges([],[]);
    }
}