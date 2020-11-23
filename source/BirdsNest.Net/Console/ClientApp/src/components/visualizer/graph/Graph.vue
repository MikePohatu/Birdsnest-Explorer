<!-- Copyright (c) 2019-2020 "20Road"
20Road Limited [https://www.20road.com]

This file is part of Birdsnest Explorer.

Birdsnest Explorer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see http://www.gnu.org/licenses/.
-->
<template>
	<div id="drawingpane" class="fillAvailable" v-on:keyup.delete="onGraphDeletePressed" tabindex="0">
		<link rel="stylesheet" type="text/css" href="/dynamic/plugins.css" />
		<svg id="drawingSvg">
			<g id="zoomlayer">
				<g id="graphBgLayer">
					<GraphNodeBg v-for="node in graphNodes" :key="'nbg'+node.dbId" :node="node" />
					<GraphEdgeBg v-for="edge in graphEdges" :key="'ebg'+edge.dbId" :edge="edge" />
				</g>
				<g id="edgesLayer">
					<GraphEdge v-for="edge in graphEdges" :key="edge.dbId" :edge="edge" />
				</g>
				<g id="nodesLayer">
					<GraphNode v-for="node in graphNodes" :key="node.dbId" v-bind:node="node" />
				</g>
			</g>
		</svg>

		<ViewControls />
		<div id="detailcardwrapper">
			<div v-for="item in detailsItems" :key="item.dbId">
				<NodeDetailCard v-if="item.itemType === 'node'" v-bind:node="item" />
				<EdgeDetailCard v-else v-bind:edge="item" />
			</div>
		</div>
	</div>
</template>


<style scoped>
#detailcardwrapper {
	position: absolute;
	top: 0;
	right: 0;
	max-height: 100%;
	overflow-y: auto;
	z-index: 200;
}

#drawingpane {
	overflow: hidden;
	position: relative;
	bottom: 0;
	height: 100%;
	font-family: "helvetica", arial, sans-serif;
}

/* bg color set on drawingSvg as well so it comes out when exporting to svg file */
#drawingpane, #drawingSvg {
	background-color: #e2e2e2;
}

#drawingSvg {
	height: 100%;
	width: 100%;
	z-index: 0;
	padding: 0;
}

#drawingpane >>> .cropBox {
	border: 1px dotted dodgerblue;
	fill: dodgerblue;
	fill-opacity: 0.1;
}
</style>

<style>
.graphbg {
	fill: white;
}

/* nodes */
.nodes {
	stroke-width: 3px;
	color: #474747;
	stroke: #474747;
	fill: #e5e5e5;
}

.nodes .icon {
	fill: #474747;
}

.selected .nodecircle {
	stroke-width: 5px;
	fill: #ffff99;
}

.nodetext {
	font-size: 10px;
	fill: #353535;
	stroke: none;
	font-family: "helvetica", arial, sans-serif;
	-webkit-user-select: none;
	-moz-user-select: none;
	-ms-user-select: none;
	user-select: none;
}

.pin {
	color: white;
	fill: #ff5b5b;
	stroke-width: 0;
}

.pin .icon {
	fill: white;
}

.nodes.disabled {
	color: #dddddd;
	stroke: #dddddd;
	fill: #fafafa;
}

.nodes.disabled text {
	fill: #dddddd;
}

/* edges */
.edgelabel text {
	font-size: 8px;
	fill: #353535;
	stroke: none !important;
	font-family: "helvetica", arial, sans-serif;
	-webkit-user-select: none;
	-moz-user-select: none;
	-ms-user-select: none;
	user-select: none;
}

.arrows {
	fill: none;
	stroke-linecap: square;
	stroke-linejoin: round;
	stroke: #353535;
}

.selected .arrows {
	stroke-width: 2px;
}

.selected .edgelabel {
	font-weight: bold;
}

.edges.disabled text {
	fill: #dddddd;
}

.edges.disabled .arrows {
	stroke: #dddddd;
}

.edgebg {
	stroke: white;
	stroke-width: 24px;
	fill: none;
}

.edgebg-loop {
	stroke-width: 12px;
}
</style>


<script lang="ts">
import { bus, events } from "@/bus";
import { routeDefs } from "@/router/index";
import { graphData } from "@/assets/ts/visualizer/GraphData";
import { Component, Vue } from "vue-property-decorator";

import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { d3 } from "@/assets/ts/visualizer/d3";

import ViewControls from "./ViewControls.vue";
import NodeDetailCard from "./NodeDetailCard.vue";
import EdgeDetailCard from "./EdgeDetailCard.vue";
import GraphNode from "./GraphNode.vue";
import GraphNodeBg from "./GraphNodeBg.vue";
import GraphEdge from "./GraphEdge.vue";
import GraphEdgeBg from "./GraphEdgeBg.vue";

import webcrap from "@/assets/ts/webcrap/webcrap";
import { ResultSet } from "@/assets/ts/dataMap/ResultSet";
import SimulationController from "@/assets/ts/visualizer/SimulationController";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { SimLink } from "@/assets/ts/visualizer/SimLink";
import Slope from "@/assets/ts/visualizer/Slope";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import LStore from "../../../assets/ts/LocalStorageManager";

//import {VisualizerStorePaths} from "@/store/modules/VisualizerStore";

@Component({
	components: {
		GraphNode,
		GraphNodeBg,
		GraphEdge,
		GraphEdgeBg,
		ViewControls,
		NodeDetailCard,
		EdgeDetailCard,
	},
})
export default class Graph extends Vue {
	routeDefs = routeDefs;
	drawingPane;
	drawingSvg;
	zoomLayer;
	graphBgLayer;
	edgesLayer;
	nodesLayer;
	areaBox;
	simController = new SimulationController();
	graphData = graphData; //create hook to make it reactive
	zoomer;
	onResize = webcrap.misc.debounce(this.centerView, 500);
	bus = bus;
	controlEvents = events.Visualizer.Controls;

	watchers: Function[] = [];

	get graphNodes(): SimNode[] {
		return this.graphData.graphNodes.GetArray();
	}

	get graphEdges(): SimLink<SimNode>[] {
		return this.graphData.graphEdges.GetArray();
	}

	get detailsItems(): (SimNode | SimLink<SimNode>)[] {
		return this.graphData.detailsItems.GetArray();
	}

	created() {
		//#region control events

		//eventbus and event registrations. All registrations MUST be deregistered in the
		//beforeDestroy method. 
		bus.$on(events.Visualizer.Controls.RefreshLayout, () => {
			this.simController.RestartSimulation();
		});
		bus.$on(events.Visualizer.Controls.Select, () => {
			this.toggleNodeSelectMode();
		});
		bus.$on(events.Visualizer.Controls.Invert, () => {
			this.graphData.invertSelectedItems();
			this.graphData.detailsItems.Clear();
		});
		bus.$on(events.Visualizer.Controls.Crop, () => {
			this.toggleCropMode();
		});
		bus.$on(events.Visualizer.Controls.CenterView, () => {
			this.centerView();
		});
		bus.$on(events.Visualizer.Controls.Export, () => {
			LStore.storePendingNodeList(graphData.graphNodes.GetArray());
			const routeData = this.$router.resolve({ path: routeDefs.report.path });
			window.open(routeData.href, "_blank");
		});
		bus.$on(events.Visualizer.Controls.Search, value => {
			this.searchGraph(value);
		});
		bus.$on(events.Visualizer.Controls.ClearView, () => {
			if (confirm("Are you sure you want to clear the current view?")) {
				this.graphData.reset();
			}
		});
		bus.$on(events.Visualizer.Controls.RemoveNodes, () => {
			const selNodeIds = this.graphData.getSelectedNodeIds();
			if (selNodeIds.length === 0) {
				return;
			}
			this.removeNodeIds(selNodeIds);
		});
		//#endregion

		//#region node events
		bus.$on(events.Visualizer.Node.NodePinClicked, id => {
			const node = this.graphData.graphNodes.GetDatum(id);
			this.unpinNode(node);
		});
		bus.$on(events.Visualizer.Node.NodeClicked, (node: SimNode) => {
			this.graphData.clearSelectedItems();
			this.graphData.detailsItems.Clear();
			this.graphData.addSelection(node);
			this.graphData.detailsItems.Add(node);
			node.selected = true;
		});
		bus.$on(events.Visualizer.Node.NodeCtrlClicked, (node: SimNode) => {
			if (node.selected) {
				this.graphData.removeSelection(node);
				this.graphData.detailsItems.Remove(node);
			} else {
				this.graphData.addSelection(node);
				this.graphData.detailsItems.Add(node);
			}
		});
		//#endregion

		//#region edge events
		bus.$on(events.Visualizer.Edge.EdgeClicked, (edge: SimLink<SimNode>) => {
			this.graphData.clearSelectedItems();
			this.graphData.detailsItems.Clear();
			this.graphData.addSelection(edge);
			this.graphData.detailsItems.Add(edge);
			edge.selected = true;
		});
		bus.$on(events.Visualizer.Edge.EdgeCtrlClicked, (edge: SimLink<SimNode>) => {
			if (edge.selected) {
				this.graphData.removeSelection(edge);
				this.graphData.detailsItems.Remove(edge);
			} else {
				this.graphData.addSelection(edge);
				this.graphData.detailsItems.Add(edge);
			}
		});
		//#endregion

		//#region detail card events
		bus.$on(events.Visualizer.RelatedDetails.DeleteNodeClicked, (node: SimNode) => {
			this.removeNode(node);
		});
		//#endregion

		//#region eye control events
		bus.$on(events.Visualizer.EyeControls.ToggleNodeLabel, label => {
			const current = this.graphData.graphNodeLabelStates[label];

			this.nodesLayer.selectAll(".nodes." + label).each((d: SimNode) => {
				d.enabled = !current;
			});
			this.graphData.graphNodeLabelStates[label] = !current;
		});

		bus.$on(events.Visualizer.EyeControls.ToggleEdgeLabel, label => {
			const current = this.graphData.graphEdgeLabelStates[label];
			this.edgesLayer.selectAll(".edges." + label).each((d: SimLink<SimNode>) => {
				d.enabled = !current;
			});
			this.graphData.graphEdgeLabelStates[label] = !current;
		});

		bus.$on(events.Visualizer.EyeControls.ShowAllNodeLabel, (enabled: boolean) => {
			Object.keys(this.graphData.graphNodeLabelStates).forEach((key: string) => {
				this.graphData.graphNodeLabelStates[key] = enabled;
			});

			this.nodesLayer.selectAll(".nodes").each((d: SimNode) => {
				d.enabled = enabled;
			});
		});

		bus.$on(events.Visualizer.EyeControls.ShowllEdgeLabel, enabled => {
			Object.keys(this.graphData.graphEdgeLabelStates).forEach((key: string) => {
				this.graphData.graphEdgeLabelStates[key] = enabled;
			});

			this.edgesLayer.selectAll(".edges").each((d: SimNode) => {
				d.enabled = enabled;
			});
		});

		bus.$on(events.Visualizer.EyeControls.InvertNodeLabel, () => {
			Object.keys(this.graphData.graphNodeLabelStates).forEach((label: string) => {
				const enabled = this.graphData.graphNodeLabelStates[label];
				this.graphData.graphNodeLabelStates[label] = !enabled;

				this.nodesLayer.selectAll(".nodes." + label).each((d: SimNode) => {
					d.enabled = !enabled;
				});
			});
		});

		bus.$on(events.Visualizer.EyeControls.InvertEdgeLabel, () => {
			Object.keys(this.graphData.graphEdgeLabelStates).forEach((label: string) => {
				const enabled = this.graphData.graphEdgeLabelStates[label];
				this.graphData.graphEdgeLabelStates[label] = !enabled;

				this.edgesLayer.selectAll(".edges." + label).each((d: SimNode) => {
					d.enabled = !enabled;
				});
			});
		});
		//#endregion
	}

	beforeDestroy() {
		//console.log("Graph destroyed");
		graphData.reset();
		this.simController = null;
		this.watchers.forEach(unwatcher => {
			unwatcher();
		});

		this.watchers = [];
		window.removeEventListener("resize", this.onResize);

		bus.$off(events.Visualizer.Controls.RefreshLayout);
		bus.$off(events.Visualizer.Controls.Select);
		bus.$off(events.Visualizer.Controls.Invert);
		bus.$off(events.Visualizer.Controls.Crop);
		bus.$off(events.Visualizer.Controls.CenterView);
		bus.$off(events.Visualizer.Controls.Export);
		bus.$off(events.Visualizer.Controls.Search);
		bus.$off(events.Visualizer.Controls.ClearView);
		bus.$off(events.Visualizer.Controls.RemoveNodes);
		//#endregion

		//#region node events
		bus.$off(events.Visualizer.Node.NodePinClicked);
		bus.$off(events.Visualizer.Node.NodeClicked);
		bus.$off(events.Visualizer.Node.NodeCtrlClicked);
		//#endregion

		//#region edge events
		bus.$off(events.Visualizer.Edge.EdgeClicked);
		bus.$off(events.Visualizer.Edge.EdgeCtrlClicked);
		//#endregion

		//#region detail card events
		bus.$off(events.Visualizer.RelatedDetails.DeleteNodeClicked);
		//#endregion

		//#region eye control events
		bus.$off(events.Visualizer.EyeControls.ToggleNodeLabel);
		bus.$off(events.Visualizer.EyeControls.ToggleEdgeLabel);
		bus.$off(events.Visualizer.EyeControls.ShowAllNodeLabel);
		bus.$off(events.Visualizer.EyeControls.ShowllEdgeLabel);
		bus.$off(events.Visualizer.EyeControls.InvertNodeLabel);
		bus.$off(events.Visualizer.EyeControls.InvertEdgeLabel);
	}

	mounted() {
		this.drawingPane = d3.select<HTMLElement, null>("#drawingpane");
		this.drawingSvg = d3.select<SVGGraphicsElement, null>("#drawingSvg");
		this.zoomLayer = d3.select<SVGGraphicsElement, null>("#zoomlayer");
		this.graphBgLayer = d3.select<SVGGraphicsElement, null>("#graphBgLayer");
		this.edgesLayer = d3.select<SVGGraphicsElement, null>("#edgesLayer");
		this.nodesLayer = d3.select<SVGGraphicsElement, null>("#nodesLayer");

		this.$store.dispatch(VisualizerStorePaths.actions.INIT);
		this.zoomer = d3
			.zoom()
			.scaleExtent([0.05, 5])
			.on("zoom", () => {
				this.zoomLayer.attr("transform", d3.event.transform);
			});
		this.resetDrawingEvents();
		this.centerView();
		window.addEventListener("resize", this.onResize);
		this.initWatchers();
		this.simController.onFinishSimulation = this.onLayoutFinished;
	}

	initWatchers(): void {
		//pending nodes
		const pendingNodesWatch = this.$store.watch(
			() => {
				return this.$store.state.visualizer.pendingNodes;
			},
			webcrap.misc.debounce(() => {
				if (this.$store.state.visualizer.pendingNodes.length > 0) {
					bus.$emit(events.Notifications.Processing, "Loading nodes");
					setTimeout(() => {
						this.graphData.addNodes(this.$store.state.visualizer.pendingNodes);
						this.$store.commit(VisualizerStorePaths.mutations.Delete.PENDING_NODES);
						this.updateNodeSizes();
						this.refreshNodeConnections();
						bus.$emit(events.Notifications.Clear);
					}, 1000);
				}
			}, 1000)
		);

		//pending result set
		const pendingResultsWatch = this.$store.watch(
			() => {
				return this.$store.state.visualizer.pendingResults;
			},
			webcrap.misc.debounce(() => {
				if (this.$store.state.visualizer.pendingResults.length > 0) {
					bus.$emit(events.Notifications.Processing, "Loading results");
					setTimeout(() => {
						this.$store.state.visualizer.pendingResults.forEach((result: ResultSet) => {
							this.graphData.addResultSet(result);
						});
						this.$store.commit(VisualizerStorePaths.mutations.Delete.PENDING_RESULTS);
						this.updateNodeSizes();
						this.refreshNodeConnections();
						bus.$emit(events.Notifications.Clear);
					}, 1000);
				}
			}, 500)
		);

		this.watchers.push(pendingNodesWatch);
		this.watchers.push(pendingResultsWatch);
	}

	resetDrawingEvents() {
		this.drawingSvg
			.on("click", this.onPageClicked)
			.on("mousedown", null)
			.on("touchstart", null)
			.on("touchend", null)
			.on("mouseup", null)
			.call(this.zoomer);
	}

	refreshNodeConnections(): void {
		const postData = JSON.stringify(this.graphData.graphNodes.GetIDs());

		const loopsrequest: Request = {
			url: "/api/graph/directloops",
			data: postData,
			postJson: true,
			successCallback: (data: ResultSet) => {
				data.edges.forEach(edge => {
					this.graphData.graphEdges.GetDatum(edge.dbId).shift = true;
				});
				this.simController.RefreshData();
				this.simController.RestartSimulation();
			},
			errorCallback: () => {
				// eslint-disable-next-line
				console.error("Error getting Edges");
			},
		};

		const newrequest: Request = {
			url: "/api/graph/edges",
			data: postData,
			postJson: true,
			successCallback: (data: ResultSet) => {
				this.graphData.addResultSet(data);
				this.nodesLayer.selectAll(".nodes").call(
					d3
						.drag()
						.on("start", this.onNodeDragStart)
						.on("drag", this.onNodeDragged)
						.on("end", this.onNodeDragFinished)
				);
				api.post(loopsrequest);
			},
			errorCallback: () => {
				// eslint-disable-next-line
				console.error("Error getting Edges");
			},
		};
		api.post(newrequest);
	}

	searchGraph(value: string) {
		const lowerVal = value.toLowerCase();
		this.graphData.clearSelectedItems();
		this.graphData.graphNodes.GetArray().forEach(node => {
			if (node.name.toLowerCase().includes(lowerVal)) {
				this.graphData.addSelection(node);
			}
		});
	}

	//#region node dragged functions
	onNodeDragged(d) {
		if (d3.event.dx === 0 && 0 === d3.event.dy) {
			return;
		}
		d3.event.sourceEvent.stopPropagation();

		d.dragged = true;
		let nodes;
		//if (playMode === true) { pauseLayout(); }
		//if the node is selected the move it and all other selected nodes
		if (d.selected) {
			nodes = this.nodesLayer.selectAll(".selected").each((seld: SimNode) => {
				seld.x += d3.event.dx;
				seld.y += d3.event.dy;
				seld.startx = seld.x;
				seld.starty = seld.y;
				seld.currentX = seld.x;
				seld.currentY = seld.y;
				this.pinNode(d);
			});
		} else {
			nodes = this.nodesLayer.select("#node_" + d.dbId);
			d.x += d3.event.dx;
			d.y += d3.event.dy;
			d.startx = d.x;
			d.starty = d.y;
			d.currentX = d.x;
			d.currentY = d.y;
			this.pinNode(d);
		}

		this.updateNodeLocations(nodes, false);
	}

	onNodeDragStart() {
		this.simController.StopSimulations();
	}

	onNodeDragFinished(d) {
		//console.log("onNodeDragFinished");
		d3.event.sourceEvent.stopPropagation();
		if (this.$store.state.visualizer.playMode === true && d.dragged === true) {
			this.simController.RestartSimulation();
		}
		d.dragged = false;
	}

	pinNode(d) {
		d.fx = d.x;
		d.fy = d.y;
		d.pinned = true;
	}

	unpinNode(d) {
		delete d.fx;
		delete d.fy;
		d.pinned = false;
	}

	//#endregion

	//#region crop and select

	startSelector() {
		this.drawingSvg
			.on("click", this.onDrawAreaBoxClick, true)
			.on("mousedown", this.onSelectorMouseDown, true)
			.on("touchstart", this.onSelectorMouseDown, true)
			.on(".zoom", null);
	}

	drawAreaBox(areaBoxEl, oriCoords, newCoords) {
		areaBoxEl
			.attr("x", Math.min(oriCoords[0], newCoords[0]))
			.attr("y", Math.min(newCoords[1], oriCoords[1]))
			.attr("width", Math.abs(newCoords[0] - oriCoords[0]))
			.attr("height", Math.abs(newCoords[1] - oriCoords[1]));
	}

	onDrawAreaBoxClick() {
		d3.event.stopPropagation();
	}

	onSelectorMouseDown(d, i, n) {
		//console.log("onSelectMouseDown");
		d3.event.stopPropagation();
		if (this.areaBox !== undefined) {
			this.areaBox.remove();
		}

		this.areaBox = this.drawingSvg
			.append("rect")
			.attr("id", "areaBox")
			.attr("class", "cropBox");

		const oriMouseX = d3.mouse(n[i])[0];
		const oriMouseY = d3.mouse(n[i])[1];

		const mousemove = (d, i, n) => {
			this.drawAreaBox(this.areaBox, [oriMouseX, oriMouseY], d3.mouse(n[i]));
		};
		const mouseup = (d, i, n) => {
			//console.log("onSelectMouseDown mouseup");
			const newMouseX = d3.mouse(n[i])[0];
			const newMouseY = d3.mouse(n[i])[1];

			if (newMouseX !== oriMouseX && newMouseY !== oriMouseY) {
				this.selectMouseUpFunction();
			}

			if (this.areaBox !== undefined) {
				this.areaBox.remove();
			}

			this.$store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);
			this.$store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);

			//delay the re-register so any mouseup doesn't trigger a pageClick when it gets re-registered
			setTimeout(() => {
				this.resetDrawingEvents();
			}, 50);
		};

		this.drawingSvg
			.on("mousemove", mousemove)
			.on("mouseup", mouseup)
			.on("touchmove", mousemove)
			.on("touchend", mouseup);
	}

	//These are the functions to run after the selector has finished selecting. selectMouseUpFunction is called
	//from the mouseup function. Set this function, then call startSelector
	selectMouseUpFunction: () => void;

	toggleNodeSelectMode() {
		if (this.$store.state.visualizer.selectModeActive) {
			this.resetDrawingEvents();
			this.$store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);
		} else {
			this.$store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, true);
			this.$store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);

			this.selectMouseUpFunction = () => {
				const areaBoxEl = this.areaBox.node().getBoundingClientRect();
				d3.selectAll(".nodes.enabled").each((d: SimNode) => {
					const elem = this.drawingPane
						.select("#node_" + d.dbId + "_icon")
						.node()
						.getBoundingClientRect();

					if (
						areaBoxEl.top < elem.top &&
						areaBoxEl.bottom > elem.bottom &&
						areaBoxEl.left < elem.left &&
						areaBoxEl.right > elem.right
					) {
						this.updateNodeSelection(d, true, false);
					} else {
						if (d3.event.ctrlKey === false) {
							this.updateNodeSelection(d, false, false);
						}
					}
				});
			};
			this.startSelector();
		}
	}

	toggleCropMode() {
		if (this.$store.state.visualizer.cropModeActive) {
			this.resetDrawingEvents();
			this.$store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);
		} else {
			this.$store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, true);
			this.$store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);

			this.selectMouseUpFunction = () => {
				const box = this.drawingPane.node().getBoundingClientRect();
				const areaBoxEl = this.areaBox.node().getBBox();
				const currentk = d3.zoomTransform(this.drawingSvg.node()).k;
				const k = Math.min(box.width / areaBoxEl.width, box.height / areaBoxEl.height);
				const areaBoxElCenterX = areaBoxEl.x + areaBoxEl.width / 2;
				const areaBoxElCenterY = areaBoxEl.y + areaBoxEl.height / 2;
				const movex = (box.width / 2 - areaBoxElCenterX) / currentk;
				const movey = (box.height / 2 - areaBoxElCenterY) / currentk;

				this.drawingSvg
					.transition()
					.duration(500)
					.ease(d3.easeCubicInOut)
					.call(this.zoomer.translateBy, movex, movey)
					.on("end", () => {
						this.drawingSvg
							.transition()
							.duration(500)
							.ease(d3.easeCubicInOut)
							.call(this.zoomer.scaleBy, k);
					});
			};
			this.startSelector();
		}
	}
	//#endregion

	onPageClicked() {
		if (d3.event.defaultPrevented) {
			return;
		} // dragged
		this.graphData.clearSelectedItems();
	}

	onGraphDeletePressed() {
		//console.log("onGraphDeletePressed");
		this.bus.$emit(this.controlEvents.RemoveNodes)
	}

	onLayoutFinished() {
		//console.log("onLayoutFinished");
		const animate = !this.$store.state.visualizer.perfMode;
		this.updateAllNodeLocations(animate);

		if (this.isScreenEmpty() === true) {
			this.centerView();
		}
	}

	isScreenEmpty() {
		//console.log("isScreenEmpty");
		const box = this.drawingPane.node().getBoundingClientRect();
		const svgbox = this.drawingSvg.node().getBBox();
		//console.log(box);
		//console.log(svgbox);

		if (
			svgbox.x > box.x + box.width ||
			svgbox.y > box.y + box.height ||
			svgbox.x + svgbox.width < box.x ||
			svgbox.y + svgbox.height < box.y
		) {
			//console.log("true");
			return true;
		}
		//console.log("false");
		return false;
	}

	//#region d3 stuff
	updateNodeSizes() {
		this.graphData.updateScale();
		this.simController.RefreshData();
		if (this.$store.state.visualizer.perfMode === true) {
			this.graphNodes.forEach(node => {
				node.currentSize = node.size;
			});
			this.updateLocationsEdges();
		} else {
			//animated node size update. has to happen on next tick so DOM is updated
			Vue.nextTick(() => {
				const nodes = this.nodesLayer.selectAll(".nodes");
				const nodecount = this.nodesLayer.size();
				nodes
					.transition()
					.duration(500)
					.tween("nodesize_tween", (d, i) => {
						const inter = d3.interpolateNumber(d.currentSize, d.size);
						return t => {
							d.currentSize = inter(t);
							d.cx = d.currentX + d.currentSize / 2;
							d.cy = d.currentY + d.currentSize / 2;
							//update the edge locations when you get to the end of the nodes list
							//don't run on the first 'tick'
							if (i === nodecount && t !== 0) {
								this.updateLocationsEdges();
							}
							return "";
						};
					});
			});
		}
	}

	centerView() {
		//console.log("centerView");
		const box = this.drawingPane.node().getBoundingClientRect();
		const svgbox = this.drawingSvg.node().getBBox();
		const k = d3.zoomTransform(this.zoomLayer.node()).k;
		const movex = (box.width / 2 - svgbox.x - svgbox.width / 2) / k;
		const movey = (box.height / 2 - svgbox.y - svgbox.height / 2) / k;

		this.drawingSvg
			.transition()
			.duration(1000)
			.ease(d3.easeCubicInOut)
			.call(this.zoomer.translateBy, movex, movey);
	}

	updateAllNodeLocations(animate: boolean) {
		const nodes = this.nodesLayer.selectAll(".nodes");
		this.updateNodeLocations(nodes, animate);
	}

	updateNodeLocations(nodes, animate: boolean) {
		//console.log("updateNodeLocations");
		//console.log(nodes);
		const duration = 750;
		const nodecount = nodes.size() - 1;

		if (animate === true) {
			nodes
				.transition("nodes_update")
				.ease(d3.easeCubic)
				.duration(duration)
				.tween("link_update", (d, i) => {
					return t => {
						const interx = d3.interpolateNumber(d.startx, d.x);
						const intery = d3.interpolateNumber(d.starty, d.y);
						d.currentX = interx(t);
						d.currentY = intery(t);
						d.cx = interx(t) + d.radius;
						d.cy = intery(t) + d.radius;

						//update the edge locations when you get to the end of the nodes list
						//don't run on the first 'tick'
						if (i === nodecount && t !== 0) {
							this.updateLocationsEdges();
						}
					};
				});
		} else {
			nodes.each((d: SimNode) => {
				d.currentX = d.x;
				d.currentY = d.y;
				d.cx = d.x + d.radius;
				d.cy = d.y + d.radius;
			});
			this.updateLocationsEdges();
		}
	}

	updateLocationsEdges() {
		//console.log("updateLocationsEdges");
		const alledges = this.edgesLayer.selectAll(".edges").data(this.graphData.graphEdges.GetArray());
		const defaultshift = this.graphData.defaultNodeSize / 2.5;

		alledges.each((d: SimLink<SimNode>, i, nodes) => {
			//console.log(d);
			const src = d.source as SimNode;
			const tar = d.target as SimNode;

			//loop check
			if (src === tar) {
				const edge = d3.select(nodes[i]);

				const start = { x: src.cx - src.radius / 2, y: src.currentY - (4 - src.scale) };
				const end = { x: src.cx + src.radius / 2, y: src.currentY - (4 - src.scale) };

				//the tangent points make a 45 degree so the arrow point can go on the 90
				const startTangent = { x: start.x - 50, y: start.y - 50 };
				const endTangent = { x: end.x + 50, y: end.y - 50 };

				const looppath =
					"M " +
					start.x +
					"," +
					start.y +
					" C" +
					startTangent.x +
					"," +
					startTangent.y +
					" " +
					endTangent.x +
					"," +
					endTangent.y +
					" " +
					end.x +
					"," +
					end.y;

				const arrowedPath =
					looppath +
					" L " +
					end.x +
					" " +
					(end.y - 7) +
					" L " +
					(end.x - 1) +
					" " +
					(end.y + 1) +
					" L " +
					(end.x + 7) +
					" " +
					end.y +
					" L " +
					end.x +
					" " +
					end.y +
					" ";

				edge.selectAll(".arrows").attr("d", arrowedPath);

				edge.selectAll(".edgelabel").attr("transform", () => {
					return "translate(" + src.cx + "," + (src.currentY - 40) + ")";
				});

				this.graphBgLayer
					.select("#edgebg_" + d.dbId)
					.attr("d", looppath)
					.each((d: SimLink<SimNode>) => {
						d.isLoop = true;
					});
			} else {
				const diagLine = new Slope(src.cx, src.cy, tar.cx, tar.cy);
				let lineshift = 0;

				if (d.shift === true) {
					lineshift = defaultshift;
				}

				const transform = "rotate(" +
							diagLine.deg +
							" " +
							diagLine.x1 +
							" " +
							diagLine.y1 +
							") " +
							"translate(" +
							diagLine.x1 +
							" " +
							(diagLine.y1 + lineshift) +
							")";

				const lineend = Math.max(diagLine.length - tar.radius - 5, 0);
				const linestart = Math.min(src.radius + 5, lineend);

				//move and rotate the edge line to the right spot
				const edge = d3.select(nodes[i]).attr("transform", () => {
					return transform;
				});

				// Reevaluate the path coordinates so it looks right
				edge.selectAll(".arrows").attr("d", "M " +
					linestart +
					" 0 " +
					"L " +
					lineend +
					" 0 " +
					"L " +
					(lineend - 5) +
					" -5 " +
					"L " +
					(lineend + 1) +
					" 0 " +
					"L " +
					(lineend - 5) +
					" 5 " +
					"L " +
					lineend +
					" 0 Z");

				edge
					.selectAll(".edgelabel")
					.attr("transform-origin", "30," + lineshift)
					.attr("transform", () => {
						//let translation;
						if (diagLine.x2 > diagLine.x1) {
							return "translate(" + diagLine.mid + ",0)";
						} else {
							//let axis = diagLine.getCoordsFromLength(diagLine.mid);
							return "translate(" + diagLine.mid + ",0) rotate(180)";
						}
					});

				//do the bg as well
				this.graphBgLayer
					.select("#edgebg_" + d.dbId)
					.attr("transform", () => {
						return transform;
					})
					.attr("d", "M 0 0 "+ diagLine.length + " 0 Z");
			}
		});

		alledges.attr("visibility", "visible");
	}

	updateNodeSelection(d, isselected, showdetails) {
		d.selected = isselected;
		if (isselected) {
			this.graphData.addSelection(d);
			if (showdetails) {
				this.graphData.detailsItems.Add(d);
			}
		} else {
			this.graphData.removeSelection(d);
			this.graphData.detailsItems.Remove(d);
		}
	}

	removeNode(node: SimNode) {
		this.removeNodeIds([node.dbId]);
	}

	removeNodeIds(ids: string[]) {
		let message;
		if (ids.length === 1) {
			message = "Are you sure you want to remove this node?";
		} else {
			message = "Are you sure you want to remove the " + ids.length + " selected nodes?";
		}

		if (confirm(message)) {
			this.getDirectEdgesForNodeList(ids, (data: ResultSet) => {
				const edgeids: string[] = [];
				data.edges.forEach(edge => {
					edgeids.push(edge.dbId);
				});

				this.graphData.removeIds(ids, edgeids);

				this.updateNodeSizes();
			});
		}
	}

	getEdgesForNodes(nodelist, callback) {
		const postdata = JSON.stringify(nodelist);
		const url = "/api/graph/edges";
		const newrequest: Request = {
			url: url,
			data: postdata,
			postJson: true,
			successCallback: callback,
			errorCallback: () => {
				// eslint-disable-next-line
				console.error("Error getting Edges");
			},
		};
		api.post(newrequest);
	}

	getDirectLoopsForNodes(nodelist, callback) {
		const postdata = JSON.stringify(nodelist);
		const url = "/api/graph/directloop";
		const newrequest: Request = {
			url: url,
			successCallback: callback,
			data: postdata,
			postJson: true,
			errorCallback: () => {
				// eslint-disable-next-line
				console.error("Error getting direct loops");
			},
		};
		api.post(newrequest);
	}

	getDirectEdgesForNodeList(nodelist, callback) {
		const url = "/api/graph/edges/direct";
		const newrequest: Request = {
			url: url,
			successCallback: callback,
			data: JSON.stringify(nodelist),
			postJson: true,
			errorCallback: () => {
				// eslint-disable-next-line
				console.error("Error getting direct loops");
			},
		};
		api.post(newrequest);
	}
	//#endregion
}
</script>