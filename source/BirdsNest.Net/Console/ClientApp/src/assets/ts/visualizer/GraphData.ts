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

// Main data store for the graph. Remember to commit changes from calling methods.
// Commit should not be directly called from GraphData in case multiple changes 
// are queued

import DatumStore from "./DatumStore";
import { ApiNode } from '@/assets/ts/dataMap/ApiNode';
import Spiral from "./Spiral";
import { ResultSet } from '@/assets/ts/dataMap/ResultSet';
import { ApiEdge } from '@/assets/ts/dataMap/ApiEdge';
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import { SimNode } from './SimNode';
import { SimLink } from './SimLink';
import Slope from "./Slope";
import { VisualizerStorePaths } from '@/store/modules/VisualizerStore';
import { store } from "@/store";
import { reactive } from "vue";

class GraphData {
    defaultNodeSize = 40;
    treeNodes = new DatumStore<SimNode>();
    treeEdges = new DatumStore<SimLink<SimNode>>();
    graphNodes = new DatumStore<SimNode>();
    graphEdges = new DatumStore<SimLink<SimNode>>();

    graphNodeLabelStates: Dictionary<boolean> = {};
    graphEdgeLabelStates: Dictionary<boolean> = {};
    private selectedItems = new DatumStore<SimNode | SimLink<SimNode>>();
    detailsItems = new DatumStore<SimNode | SimLink<SimNode>>();

    get NodeCount(): number {
        return this.graphNodes.Count;
    }

    get EdgeCount(): number {
        return this.graphEdges.Count;
    }

    //return whether new node was added
    private addNode(node: ApiNode): SimNode {
        if (this.graphNodes.KeyExists(node.dbId) === false) {
            //console.log("no existy");
            const radius = this.defaultNodeSize / 2;
            const cx = radius;
            const cy = radius;

            //d3 messes with data, and vue has already setup reactivity so clone the BirdsNestNode to a SimNode
            //this adds all the properties needed to work in the graph layout
            const simnode: SimNode = {
                itemType: 'node',
                dbId: node.dbId,
                name: node.name,
                properties: node.properties,
                scope: node.scope,
                labels: node.labels,
                enabled: false,
                x: 0,
                currentX: 0,
                currentY: 0,
                currentSize: this.defaultNodeSize,
                y: 0,
                startx: 0,
                starty: 0,
                k: 0,
                dragged: false,
                size: this.defaultNodeSize,
                radius: radius,
                cx: cx,
                cy: cy,
                pinned: false,
                selected: false,
                relatedDetails: null,
                scale: 1,
                isTreeRoot: false,
                isConnected: false,
            }

            //update label states
            const pluginmanager = store.state.pluginManager;

            simnode.labels.every((label) => {
                if (this.graphNodeLabelStates[label] === undefined) {
                    const dt = pluginmanager.nodeDataTypes[label];
                    if (dt) {
                        this.graphNodeLabelStates[label] = dt.enabled;  
                    } else {
                        this.graphNodeLabelStates[label] = true; //if nothing is defined for the type, assume enabled=true
                    }                    
                    simnode.enabled = this.graphNodeLabelStates[label];
                } else if (this.graphNodeLabelStates[label] === true) {
                    simnode.enabled = true;
                }

                if (simnode.enabled === true) {
                    //return false to exit the loop. if any label is enabled=true simnode is enabled
                    return false;
                } 
                return true;
            });

            this.graphNodes.Add(simnode);

            return simnode;
        } else {
            return null;
        }
    }

    addNodes(nodes: ApiNode[]): GraphData {
        const spiral = new Spiral(this.defaultNodeSize * 2);
        nodes.forEach(node => {
            const simnode = this.addNode(node);
            if (simnode !== null) {
                //console.log({source:"addNodes-simnode-notnull", simnode:simnode});
                simnode.x = spiral.x;
                simnode.y = spiral.y;
                simnode.currentX = spiral.x;
                simnode.currentY = spiral.y;
                simnode.startx = spiral.x;
                simnode.starty = spiral.y;
                spiral.Step();
            }
        });

        return this;
    }

    private addEdge(edge: ApiEdge): void {
        // console.log("addEdge");
        // console.log(edge);
        if (this.graphEdges.KeyExists(edge.dbId) === false) {
            //d3 is going to replace the source and target properties with references to the nodes,
            //and vue reactivity is already set, so clone to a new SimLink
            const simlink: SimLink<SimNode> = {
                itemType: 'edge',
                dbId: edge.dbId,
                enabled: false,
                source: this.graphNodes.GetDatum(edge.source),
                target: this.graphNodes.GetDatum(edge.target),
                shift: false,
                label: edge.label,
                properties: edge.properties,
                k: 0,
                selected: false,
                isLoop: false,
            }

            if (this.graphEdgeLabelStates[edge.label] === undefined) {
                //this.graphEdgeLabelStates[edge.label] = true;
                const dt = store.state.pluginManager.edgeDataTypes[edge.label];
                if (dt) {
                    this.graphEdgeLabelStates[edge.label] = dt.enabled;
                } else {
                    this.graphEdgeLabelStates[edge.label] = true; //if nothing is defined for the type, assume enabled=true
                }
            }
            simlink.enabled = this.graphEdgeLabelStates[edge.label];

            this.graphEdges.Add(simlink);

            if (simlink.properties.layout as string === "tree") {
                const src = simlink.source;
                const tar = simlink.target;
                this.treeEdges.Add(simlink); 
                this.treeNodes.Add(src);
                this.treeNodes.Add(tar);
            }
            // newitemcount++;
        }
    }

    addResultSet(result: ResultSet): GraphData {
        // console.log("addResultSet");
        // console.log(result.edges);
        // console.log(result.nodes);

        this.addNodes(result.nodes);

        result.edges.forEach((edge: ApiEdge) => {
            this.addEdge(edge);
        });

        this.updatePerfMode();

        //console.log(this);
        return this;
    }

    commit(): void {
        this.graphNodes.Commit();
        this.treeNodes.Commit();
        this.graphEdges.Commit();
        this.treeEdges.Commit();
        this.updateEdges();
        this.selectedItems.Commit();
        this.detailsItems.Commit();
    }

    private updateEdges(): void {

        //find the tree roots
        //reset mesh nodes first
        this.graphNodes.Array.forEach((node)=> {
            node.isTreeRoot = false;
            node.isConnected = false;
        });

        //now find the roots of the tree. If a node is a source, first check it has not already
        //been found as a target, then set accordingly
        const tarTracking = {};
        this.treeEdges.Array.forEach((edge)=> {
            if (Object.prototype.hasOwnProperty.call(tarTracking, edge.source.dbId) === false) 
            { 
                edge.source.isTreeRoot = true;
            }
            edge.target.isTreeRoot = false;
            tarTracking[edge.target.dbId] = true;
        });
        this.graphEdges.Array.forEach((edge)=> { 
            edge.source.isConnected = true;
            edge.target.isConnected = true;
        });
    }

    addSelection(item: SimNode | SimLink<SimNode>): DatumStore<SimNode | SimLink<SimNode>> {
        this.selectedItems.Add(item);
        item.selected = true;
        return this.selectedItems;
    }

    removeSelection(item: SimNode | SimLink<SimNode>): DatumStore<SimNode | SimLink<SimNode>> {
        this.selectedItems.Remove(item);
        item.selected = false;
        return this.selectedItems;
    }

    clearSelectedItems(): GraphData {
        this.selectedItems.Array.forEach(d => {
            d.selected = false;
        });
        this.selectedItems.Clear();
        this.detailsItems.Clear();

        return this;
    }

    invertSelectedItems(): DatumStore<SimNode | SimLink<SimNode>> {
        this.graphNodes.Array.forEach(d => {
            if (d.selected) {
                this.selectedItems.Remove(d);
                d.selected = false;
            } else {
                if (d.enabled) {
                    this.selectedItems.Add(d);
                    d.selected = true;
                }
            }
            //d.selected = !d.selected;
        });
        return this.selectedItems;
    }

    getSelectedNodeIds(): string[] {
        return this.selectedItems.IDs;
    }

    removeIds(nodeIds: string[], edgeIds: string[]): GraphData {
        nodeIds.forEach(id => {
            this.removeNodeId(id);
        });

        edgeIds.forEach(id => {
            this.removeEdgeId(id);
        });
        this.cleanupLabelStates();
        this.updatePerfMode();
        return this;
    }

    private removeNodeId(id: string): GraphData {
        const node = this.graphNodes.GetDatum(id);
        if (node !== null) {
            this.removeNode(node);
        }
        return this;
    }

    private removeEdgeId(id: string): GraphData {
        const edge = this.graphEdges.GetDatum(id);
        if (edge !== null) {
            this.removeEdge(edge);
        }
        return this;
    }

    removeData(nodes: SimNode[], edges: SimLink<SimNode>[] ): GraphData {
        nodes.forEach(node => {
            this.removeNode(node);
        });
        edges.forEach(edge => {
            this.removeEdge(edge);
        });

        this.cleanupLabelStates();
        this.updatePerfMode();
        return this;
    }

    private cleanupLabelStates(): void {
        //check the current labels in the graph and remove any that have been removed
        const currNodeLabels: Dictionary<boolean> = {};
        const currEdgeLabels: Dictionary<boolean> = {};
        this.graphNodes.Array.forEach(node => {
            node.labels.forEach(label => {
                currNodeLabels[label] = true;
            });
        });
        this.graphEdges.Array.forEach(edge => {
            currEdgeLabels[edge.label] = true;
        });
        
        Object.keys(this.graphNodeLabelStates).forEach(label => {
            if (Object.prototype.hasOwnProperty.call(currNodeLabels, label) === false) {
                delete this.graphNodeLabelStates[label];
                //Vue.delete(this.graphNodeLabelStates,label); //previously use vue.delete
            }
        });

        Object.keys(this.graphEdgeLabelStates).forEach(label => {
            if (Object.prototype.hasOwnProperty.call(currEdgeLabels, label) === false) {
                delete this.graphEdgeLabelStates[label];
                //Vue.delete(this.graphEdgeLabelStates, label); //previously use vue.delete
            }
        });

        // console.log("cleanupLabelStates");
        // console.log(this);
    }
    private removeNode(node: SimNode): GraphData {
        this.treeNodes.Remove(node);
        this.graphNodes.Remove(node);
        this.detailsItems.Remove(node);
        this.selectedItems.Remove(node);
        return this;
    }

    private removeEdge(edge: SimLink<SimNode>): GraphData {
        this.treeEdges.Remove(edge);
        this.graphEdges.Remove(edge);
        return this;
    }

    reset() {
        this.treeNodes = new DatumStore<SimNode>();
        this.treeEdges = new DatumStore<SimLink<SimNode>>();
        this.graphNodes = new DatumStore<SimNode>();
        this.graphEdges = new DatumStore<SimLink<SimNode>>();

        this.graphEdgeLabelStates = {};
        this.graphNodeLabelStates = {};
        this.selectedItems = new DatumStore<SimNode>();
        this.detailsItems = new DatumStore<SimNode>();
    }

    updateScale() {
        //'pre-load the max scope = 20 so if most node scopes are 1, they'll all end up 
        //at the bottom of the slope
        const minScope = 1;
        const minScaling = 1;
        const maxScaling = 3;
        let maxScope = 20;

        this.graphNodes.Array.forEach((node) => {
            if (node.scope > maxScope) {
                maxScope = node.scope;
            }
        });

        const scalingRange = new Slope(minScope, minScaling, maxScope, maxScaling);

        this.graphNodes.Array.forEach((d) => {
            d.scale = scalingRange.getYFromX(d.scope);
            d.size = this.defaultNodeSize * d.scale;
            d.radius = d.size / 2;
            d.cx = d.x + d.radius;
            d.cy = d.y + d.radius;
        });
    }

    private updatePerfMode() {
        if (this.graphNodes.Array.length > 300) {
            store.commit(VisualizerStorePaths.mutations.Update.PERF_MODE, true);
        } else {
            store.commit(VisualizerStorePaths.mutations.Update.PERF_MODE, false);
        }
    }
}

export const graphData = reactive(new GraphData());