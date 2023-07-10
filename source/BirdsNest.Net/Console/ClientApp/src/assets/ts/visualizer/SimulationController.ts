// Copyright (c) 2019-2023 "20Road"
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
import TreeForce from './TreeForce';
import { ForceLink, ForceRadial, Simulation } from 'd3';


export default class SimulationController {
    
    onFinishCallback: () => void;
    private store = useStore();
    private graphsimulation: Simulation<SimNode, SimLink<SimNode>>;
    private connectedCount = 0;
    private unconnectedCount = 0;

    constructor() {
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);

        this.graphsimulation = d3.forceSimulation<SimNode>()
            .stop().alpha(1)
            .force('collide', d3.forceCollide()
                .radius(function (d: SimNode) { return d.size + 20; }))
            .force("link", d3.forceLink()
                .id(function (d: SimLink<SimNode>) { return d.dbId; }))
            .force("tree", TreeForce())
            .force("forceY", d3.forceY()
                .strength(function (d: SimNode) {
                    return d.isTreeRoot ? 1 : 0.01;
                })
                .y(function(d: SimNode) {
                    return d.isTreeRoot ? d.starty : 0;
                })
            )
            .force("forceX", d3.forceX()
                .strength(function (d: SimNode) {
                    return d.isTreeRoot ? 0 : 0.01;
                })
                .x(0)
            )
            .force("radial", d3.forceRadial(0,0)
                .strength(function(d: SimNode) {
                    return d.isConnected ? 0 : 1;
                })
                .radius(() => {
                    return 600;
                })
            )
            .force("tick", ()=>{ this.onGraphTick(); })
            .on('end', ()=>{ this.onSimulationFinished(); });
    }

    private SetEdges(alledges, trees) {
        (this.graphsimulation.force("link") as ForceLink<SimNode, SimLink<SimNode>>).links(alledges);
        (this.graphsimulation.force("tree") as any).links(trees);
    }

    private SetNodes(allnodes) {
        this.graphsimulation.nodes(allnodes);

        let velocityDecay = 0.5;
        let alphaDecay = 0.1;

        if (allnodes.length > 3000) {
            velocityDecay = 0.2;
            alphaDecay = 0.5;
        }
        else if (allnodes.length > 1000) {
            velocityDecay = 0.35;
            alphaDecay = 0.3;
        }

        this.graphsimulation
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
    }

    onGraphTick() {     
        const k = this.graphsimulation.alpha();
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100 - k * 100);
    }

    onSimulationFinished() {
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_PROGRESS, 100);

        this.onFinishCallback();
    }

    RefreshData(): SimulationController {
        this.StopSimulations();

        this.SetNodes(graphData.graphNodes.Array);
        this.SetEdges(
            graphData.graphEdges.Array,
            graphData.treeEdges.Array
        );
        return this;
    }

    RestartSimulation() {
        if (this.store.state.visualizer.simRunning === true) {
            this.StopSimulations();
        }

        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, true);
        this.connectedCount = 0;
        this.unconnectedCount = 0;
        graphData.graphNodes.Array.forEach((d: SimNode) => {
            d.startx = d.x;
            d.starty = d.y;
            if (d.isConnected) { this.connectedCount++; }
            else { this.unconnectedCount++; }
        });
        
        (this.graphsimulation.force("radial") as ForceRadial<SimNode>).radius(600 + (Math.max(this.connectedCount,this.unconnectedCount)));
        this.graphsimulation.alpha(1).restart();
    }

    StopSimulations() {
        if (this.store.state.visualizer.simRunning === true) {
            this.graphsimulation.stop();

            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            graphData.graphNodes.Array.forEach((d: SimNode) => {
                d.x = d.startx;
                d.y = d.starty;
            });
        }
        
        this.store.commit(VisualizerStorePaths.mutations.Update.SIM_RUNNING, false);
    }

    clear() {
        this.SetNodes([]);
        this.SetEdges([],[]);
    }
}