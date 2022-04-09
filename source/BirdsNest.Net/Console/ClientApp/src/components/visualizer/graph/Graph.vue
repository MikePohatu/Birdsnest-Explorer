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

#drawingpane :deep(.cropBox) {
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


<script setup lang="ts">
import { bus, events } from "@/bus";
import { routeDefs } from "@/router/index";
import { graphData } from "@/assets/ts/visualizer/GraphData";

import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";

import * as d3 from 'd3';

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
import LStore from "@/assets/ts/LocalStorageManager";
import { computed, nextTick, onBeforeUnmount, onMounted } from "vue";
import { useStore } from "@/store";
import { useRouter } from "vue-router";

	const store = useStore();
	const router = useRouter();

	let drawingPane;
	let drawingSvg;
	let zoomLayer;
	let graphBgLayer;
	let edgesLayer;
	let nodesLayer;
	let areaBox;
	let simController = new SimulationController();
	let zoomer;
	let onResize = webcrap.misc.debounce(centerView, 500);
	let controlEvents = events.Visualizer.Controls;

	let watchers = [];
	
	const graphNodes = computed<SimNode[]>(() => {
		//console.log({source:"graphNodes", graphNodes: graphData.graphNodes.Array});
		return graphData.graphNodes.Array;
	});

	const graphEdges = computed<SimLink<SimNode>[]>(() => {
		return graphData.graphEdges.Array;
	});

	const detailsItems = computed<(SimNode | SimLink<SimNode>)[]>(() => {
		return graphData.detailsItems.Array;
	});

	onCreated();
	function onCreated() {
		//#region control events

		//eventbus and event registrations. All registrations MUST be deregistered in the
		//beforeDestroy method. 
		bus.on(events.Visualizer.Controls.RefreshLayout, () => {
			simController.RestartSimulation();
		});
		bus.on(events.Visualizer.Controls.Select, () => {
			toggleNodeSelectMode();
		});
		bus.on(events.Visualizer.Controls.Invert, () => {
			graphData.invertSelectedItems().Commit();
			graphData.detailsItems.Clear();
		});
		bus.on(events.Visualizer.Controls.Crop, () => {
			toggleCropMode();
		});
		bus.on(events.Visualizer.Controls.CenterView, () => {
			centerView();
		});
		bus.on(events.Visualizer.Controls.Export, () => {
			LStore.storePendingNodeList(graphData.graphNodes.Array);
			const routeData = router.resolve({ path: routeDefs.report.path });
			window.open(routeData.href, "_blank");
		});
		bus.on(events.Visualizer.Controls.Search, value => {
			searchGraph(value as string);
		});
		bus.on(events.Visualizer.Controls.ClearView, () => {
			if (confirm("Are you sure you want to clear the current view?")) {
				graphData.reset();
			}
		});
		bus.on(events.Visualizer.Controls.RemoveNodes, () => {
			const selNodeIds = graphData.getSelectedNodeIds();
			if (selNodeIds.length === 0) {
				return;
			}
			removeNodeIds(selNodeIds);
		});
		//#endregion

		//#region node events
		bus.on(events.Visualizer.Node.NodePinClicked, id => {
			const node = graphData.graphNodes.GetDatum(id as string);
			unpinNode(node);
		});
		bus.on(events.Visualizer.Node.NodeClicked, (node: SimNode) => {
			graphData.clearSelectedItems();
			graphData.detailsItems.Clear();
			graphData.addSelection(node).Commit();
			graphData.detailsItems.Add(node).Commit();
			node.selected = true;
		});
		bus.on(events.Visualizer.Node.NodeCtrlClicked, (node: SimNode) => {
			if (node.selected) {
				graphData.removeSelection(node).Commit();
				graphData.detailsItems.Remove(node).Commit();
			} else {
				graphData.addSelection(node).Commit();
				graphData.detailsItems.Add(node).Commit();
			}
		});
		//#endregion

		//#region edge events
		bus.on(events.Visualizer.Edge.EdgeClicked, (edge: SimLink<SimNode>) => {
			graphData.clearSelectedItems();
			graphData.detailsItems.Clear();
			graphData.addSelection(edge).Commit();
			graphData.detailsItems.Add(edge).Commit();
			edge.selected = true;
		});
		bus.on(events.Visualizer.Edge.EdgeCtrlClicked, (edge: SimLink<SimNode>) => {
			if (edge.selected) {
				graphData.removeSelection(edge).Commit();
				graphData.detailsItems.Remove(edge).Commit();
			} else {
				graphData.addSelection(edge).Commit();
				graphData.detailsItems.Add(edge).Commit();
			}
		});
		//#endregion

		//#region detail card events
		bus.on(events.Visualizer.RelatedDetails.DeleteNodeClicked, (node: SimNode) => {
			removeNode(node);
		});
		//#endregion

		//#region eye control events
		bus.on(events.Visualizer.EyeControls.ToggleNodeLabel, label => {
			const current = graphData.graphNodeLabelStates[label as string];

			nodesLayer.selectAll(".nodes." + label).each((d: SimNode) => {
				d.enabled = !current;
			});
			graphData.graphNodeLabelStates[label as string] = !current;
		});

		bus.on(events.Visualizer.EyeControls.ToggleEdgeLabel, label => {
			const current = graphData.graphEdgeLabelStates[label as string];
			edgesLayer.selectAll(".edges." + label).each((d: SimLink<SimNode>) => {
				d.enabled = !current;
			});
			graphData.graphEdgeLabelStates[label as string] = !current;
		});

		bus.on(events.Visualizer.EyeControls.ShowAllNodeLabel, (enabled: boolean) => {
			Object.keys(graphData.graphNodeLabelStates).forEach((key: string) => {
				graphData.graphNodeLabelStates[key] = enabled;
			});

			nodesLayer.selectAll(".nodes").each((d: SimNode) => {
				d.enabled = enabled;
			});
		});

		bus.on(events.Visualizer.EyeControls.ShowllEdgeLabel, enabled => {
			Object.keys(graphData.graphEdgeLabelStates).forEach((key: string) => {
				graphData.graphEdgeLabelStates[key] = enabled as boolean;
			});

			edgesLayer.selectAll(".edges").each((d: SimNode) => {
				d.enabled = enabled as boolean;
			});
		});

		bus.on(events.Visualizer.EyeControls.InvertNodeLabel, () => {
			Object.keys(graphData.graphNodeLabelStates).forEach((label: string) => {
				const enabled = graphData.graphNodeLabelStates[label];
				graphData.graphNodeLabelStates[label] = !enabled;

				nodesLayer.selectAll(".nodes." + label).each((d: SimNode) => {
					d.enabled = !enabled;
				});
			});
		});

		bus.on(events.Visualizer.EyeControls.InvertEdgeLabel, () => {
			Object.keys(graphData.graphEdgeLabelStates).forEach((label: string) => {
				const enabled = graphData.graphEdgeLabelStates[label];
				graphData.graphEdgeLabelStates[label] = !enabled;

				edgesLayer.selectAll(".edges." + label).each((d: SimNode) => {
					d.enabled = !enabled;
				});
			});
		});
		//#endregion
	}

	onBeforeUnmount(() => {
		//console.log("Graph destroyed");
		graphData.reset();
		simController = null;
		watchers.forEach(unwatcher => {
			unwatcher();
		});

		watchers = [];
		window.removeEventListener("resize", onResize);

		bus.off(events.Visualizer.Controls.RefreshLayout);
		bus.off(events.Visualizer.Controls.Select);
		bus.off(events.Visualizer.Controls.Invert);
		bus.off(events.Visualizer.Controls.Crop);
		bus.off(events.Visualizer.Controls.CenterView);
		bus.off(events.Visualizer.Controls.Export);
		bus.off(events.Visualizer.Controls.Search);
		bus.off(events.Visualizer.Controls.ClearView);
		bus.off(events.Visualizer.Controls.RemoveNodes);
		//#endregion

		//#region node events
		bus.off(events.Visualizer.Node.NodePinClicked);
		bus.off(events.Visualizer.Node.NodeClicked);
		bus.off(events.Visualizer.Node.NodeCtrlClicked);
		//#endregion

		//#region edge events
		bus.off(events.Visualizer.Edge.EdgeClicked);
		bus.off(events.Visualizer.Edge.EdgeCtrlClicked);
		//#endregion

		//#region detail card events
		bus.off(events.Visualizer.RelatedDetails.DeleteNodeClicked);
		//#endregion

		//#region eye control events
		bus.off(events.Visualizer.EyeControls.ToggleNodeLabel);
		bus.off(events.Visualizer.EyeControls.ToggleEdgeLabel);
		bus.off(events.Visualizer.EyeControls.ShowAllNodeLabel);
		bus.off(events.Visualizer.EyeControls.ShowllEdgeLabel);
		bus.off(events.Visualizer.EyeControls.InvertNodeLabel);
		bus.off(events.Visualizer.EyeControls.InvertEdgeLabel);
	});

	onMounted(() => {
		drawingPane = d3.select<HTMLElement, null>("#drawingpane");
		drawingSvg = d3.select<SVGGraphicsElement, null>("#drawingSvg");
		zoomLayer = d3.select<SVGGraphicsElement, null>("#zoomlayer");
		graphBgLayer = d3.select<SVGGraphicsElement, null>("#graphBgLayer");
		edgesLayer = d3.select<SVGGraphicsElement, null>("#edgesLayer");
		nodesLayer = d3.select<SVGGraphicsElement, null>("#nodesLayer");

		store.dispatch(VisualizerStorePaths.actions.INIT);
		zoomer = d3.zoom()
			.scaleExtent([0.05, 5])
			.on("zoom", (e) => {
				// eslint-disable-next-line
				zoomLayer.attr("transform", (e as  any).transform);
			});
		resetDrawingEvents();
		centerView();
		window.addEventListener("resize", onResize);
		initWatchers();
		simController.onFinishSimulation = onLayoutFinished;
	});

	function initWatchers(): void {
		//pending nodes
		//console.log({source:"initWatchers"});
		const pendingNodesWatch = store.watch(
			() => {
				return store.state.visualizer.pendingNodes;
			},
			webcrap.misc.debounce(() => {
				if (store.state.visualizer.pendingNodes.length > 0) {
					bus.emit(events.Notifications.Processing, "Loading nodes");
					setTimeout(() => {
						graphData.addNodes(store.state.visualizer.pendingNodes).commitAll();
						store.commit(VisualizerStorePaths.mutations.Delete.PENDING_NODES);
						updateNodeSizes();
						refreshNodeConnections();
						bus.emit(events.Notifications.Clear);
					}, 1000);
				}
			}, 1000),{
				deep:true
			}
		);

		//pending result set
		const pendingResultsWatch = store.watch(
			() => {
				return store.state.visualizer.pendingResults;
			},
			webcrap.misc.debounce(() => {
				if (store.state.visualizer.pendingResults.length > 0) {
					bus.emit(events.Notifications.Processing, "Loading results");
					setTimeout(() => {
						store.state.visualizer.pendingResults.forEach((result: ResultSet) => {
							graphData.addResultSet(result);
						});
						graphData.commitAll();
						store.commit(VisualizerStorePaths.mutations.Delete.PENDING_RESULTS);
						updateNodeSizes();
						refreshNodeConnections();
						bus.emit(events.Notifications.Clear);
					}, 1000);
				}
			}, 500),{
				deep:true
			}
		);

		watchers.push(pendingNodesWatch);
		watchers.push(pendingResultsWatch);
	}

	function resetDrawingEvents() {
		//console.log({source:"resetDrawingEvents", drawingSvg: drawingSvg});
		drawingSvg
			.on("click", onPageClicked)
			.on("mousedown", null)
			.on("touchstart", null)
			.on("touchend", null)
			.on("mouseup", null)
			.call(zoomer);
	}

	function refreshNodeConnections(): void {
		const postData = JSON.stringify(graphData.graphNodes.IDs);

		const loopsrequest: Request = {
			url: "/api/graph/directloops",
			data: postData,
			postJson: true,
			successCallback: (data: ResultSet) => {
				data.edges.forEach(edge => {
					const exist = graphData.graphEdges.GetDatum(edge.dbId)
					if (exist !==null) { exist.shift = true; }
				});
				graphData.addResultSet(data).commitEdges();
				simController.RefreshData();
				simController.RestartSimulation();
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
				graphData.addResultSet(data);
				nodesLayer.selectAll(".nodes").call(
					d3.drag()
					.on("start", onNodeDragStart)
					.on("drag", onNodeDragged)
					.on("end", onNodeDragFinished)
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

	function searchGraph(value: string) {
		const lowerVal = value.toLowerCase();
		graphData.clearSelectedItems();
		graphData.graphNodes.Array.forEach(node => {
			if (node.name.toLowerCase().includes(lowerVal)) {
				graphData.addSelection(node).Commit();
			}
		});
	}

	//#region node dragged functions
	function onNodeDragged(e, d) {
		if (e.dx === 0 && 0 === e.dy) {
			return;
		}
		e.sourceEvent.stopPropagation();

		d.dragged = true;
		let nodes;
		//if (playMode === true) { pauseLayout(); }
		//if the node is selected the move it and all other selected nodes
		if (d.selected) {
			nodes = nodesLayer.selectAll(".selected").each((seld: SimNode) => {
				seld.x += e.dx;
				seld.y += e.dy;
				seld.startx = seld.x;
				seld.starty = seld.y;
				seld.currentX = seld.x;
				seld.currentY = seld.y;
				pinNode(d);
			});
		} else {
			nodes = nodesLayer.select("#node_" + d.dbId);
			d.x += e.dx;
			d.y += e.dy;
			d.startx = d.x;
			d.starty = d.y;
			d.currentX = d.x;
			d.currentY = d.y;
			pinNode(d);
		}

		updateNodeLocations(nodes, false);
	}

	function onNodeDragStart() {
		simController.StopSimulations();
	}

	function onNodeDragFinished(e, d) {
		//console.log("onNodeDragFinished");
		e.sourceEvent.stopPropagation();
		if (store.state.visualizer.playMode === true && d.dragged === true) {
			simController.RestartSimulation();
		}
		d.dragged = false;
	}

	function pinNode(d) {
		d.fx = d.x;
		d.fy = d.y;
		d.pinned = true;
	}

	function unpinNode(d) {
		delete d.fx;
		delete d.fy;
		d.pinned = false;
	}

	//#endregion

	//#region crop and select

	function startSelector() {
		drawingSvg
			.on("click", onDrawAreaBoxClick, true)
			.on("mousedown", onSelectorMouseDown, true)
			.on("touchstart", onSelectorMouseDown, true)
			.on(".zoom", null);
	}

	function drawAreaBox(areaBoxEl, oriCoords, newCoords) {
		areaBoxEl
			.attr("x", Math.min(oriCoords[0], newCoords[0]))
			.attr("y", Math.min(newCoords[1], oriCoords[1]))
			.attr("width", Math.abs(newCoords[0] - oriCoords[0]))
			.attr("height", Math.abs(newCoords[1] - oriCoords[1]));
	}

	function onDrawAreaBoxClick(e) {
		e.stopPropagation();
	}

	function onSelectorMouseDown(e) {
		e.stopPropagation();
		if (areaBox !== undefined) {
			areaBox.remove();
		}

		areaBox = drawingSvg
			.append("rect")
			.attr("id", "areaBox")
			.attr("class", "cropBox");

		const drawingSvgY = (drawingSvg.node() as Element).getBoundingClientRect().top;
		const drawingSvgX = (drawingSvg.node() as Element).getBoundingClientRect().left;
		const oriMouseX = e.pageX; 
		const oriMouseY = e.pageY;
		
		const mousemove = (ev) => {
			drawAreaBox(areaBox, [oriMouseX - drawingSvgX, oriMouseY - drawingSvgY], [ev.pageX - drawingSvgX, ev.pageY - drawingSvgY]);
		};
		const mouseup = (ev) => {
			//console.log("onSelectMouseDown mouseup");
			const newMouseX = ev.pageX - drawingSvgX;
			const newMouseY = ev.pageY - drawingSvgY;

			if (newMouseX !== oriMouseX && newMouseY !== oriMouseY) {
				selectMouseUpFunction(e);
			}

			if (areaBox !== undefined) {
				areaBox.remove();
			}

			store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);
			store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);

			//delay the re-register so any mouseup doesn't trigger a pageClick when it gets re-registered
			setTimeout(() => {
				resetDrawingEvents();
			}, 50);
		};

		drawingSvg
			.on("mousemove", mousemove)
			.on("mouseup", mouseup)
			.on("touchmove", mousemove)
			.on("touchend", mouseup);
	}

	//These are the functions to run after the selector has finished selecting. selectMouseUpFunction is called
	//from the mouseup function. Set this function, then call startSelector
	let selectMouseUpFunction: (e) => void;

	function toggleNodeSelectMode() {
		if (store.state.visualizer.selectModeActive) {
			resetDrawingEvents();
			store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);
		} else {
			store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, true);
			store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);

			selectMouseUpFunction = (e) => {
				const areaBoxEl = areaBox.node().getBoundingClientRect();
				d3.selectAll(".nodes.enabled").each((d: SimNode) => {
					const elem = drawingPane
						.select("#node_" + d.dbId + "_icon")
						.node()
						.getBoundingClientRect();

					if (
						areaBoxEl.top < elem.top &&
						areaBoxEl.bottom > elem.bottom &&
						areaBoxEl.left < elem.left &&
						areaBoxEl.right > elem.right
					) 
					{
						updateNodeSelection(d, true, false);
					} 
					else {
						if (e.ctrlKey === false) {
							updateNodeSelection(d, false, false);
						}
					}
				});
			};
			startSelector();
		}
	}

	function toggleCropMode() {
		if (store.state.visualizer.cropModeActive) {
			resetDrawingEvents();
			store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, false);
		} else {
			store.commit(VisualizerStorePaths.mutations.Update.CROP_ACTIVE, true);
			store.commit(VisualizerStorePaths.mutations.Update.SELECT_ACTIVE, false);

			selectMouseUpFunction = () => {
				const box = drawingPane.node().getBoundingClientRect();
				const areaBoxEl = areaBox.node().getBBox();
				const currentk = d3.zoomTransform(drawingSvg.node()).k;
				const k = Math.min(box.width / areaBoxEl.width, box.height / areaBoxEl.height);
				const areaBoxElCenterX = areaBoxEl.x + areaBoxEl.width / 2;
				const areaBoxElCenterY = areaBoxEl.y + areaBoxEl.height / 2;
				const movex = (box.width / 2 - areaBoxElCenterX) / currentk;
				const movey = (box.height / 2 - areaBoxElCenterY) / currentk;

				drawingSvg
					.transition()
					.duration(500)
					.ease(d3.easeCubicInOut)
					.call(zoomer.translateBy, movex, movey)
					.on("end", () => {
						drawingSvg
							.transition()
							.duration(500)
							.ease(d3.easeCubicInOut)
							.call(zoomer.scaleBy, k);
					});
			};
			startSelector();
		}
	}
	//#endregion

	function onPageClicked(e) {
		if (e.defaultPrevented) {
			return;
		} // dragged
		graphData.clearSelectedItems();
		if (areaBox !== undefined) {
			areaBox.remove();
		}
	}

	function onGraphDeletePressed() {
		//console.log("onGraphDeletePressed");
		bus.emit(controlEvents.RemoveNodes)
	}

	function onLayoutFinished() {
		//console.log("onLayoutFinished");
		const animate = !store.state.visualizer.perfMode;
		updateAllNodeLocations(animate);

		if (isScreenEmpty() === true) {
			centerView();
		}
	}

	function isScreenEmpty() {
		//console.log("isScreenEmpty");
		const box = drawingPane.node().getBoundingClientRect();
		const svgbox = drawingSvg.node().getBBox();
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
	function updateNodeSizes() {
		graphData.updateScale();
		simController.RefreshData();
		if (store.state.visualizer.perfMode === true) {
			graphNodes.value.forEach(node => {
				node.currentSize = node.size;
			});
			updateLocationsEdges();
		} else {
			//animated node size update. has to happen on next tick so DOM is updated
			nextTick(() => {
				const nodes = nodesLayer.selectAll(".nodes");
				const nodecount = nodesLayer.size();
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
								updateLocationsEdges();
							}
							return "";
						};
					});
			});
		}
	}

	function centerView() {
		//console.log("centerView");
		const box = drawingPane.node().getBoundingClientRect();
		const svgbox = drawingSvg.node().getBBox();
		const k = d3.zoomTransform(zoomLayer.node()).k;
		const movex = (box.width / 2 - svgbox.x - svgbox.width / 2) / k;
		const movey = (box.height / 2 - svgbox.y - svgbox.height / 2) / k;

		drawingSvg
			.transition()
			.duration(1000)
			.ease(d3.easeCubicInOut)
			.call(zoomer.translateBy, movex, movey);
	}

	function updateAllNodeLocations(animate: boolean) {
		const nodes = nodesLayer.selectAll(".nodes");
		updateNodeLocations(nodes, animate);
	}

	function updateNodeLocations(nodes, animate: boolean) {
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
							updateLocationsEdges();
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
			updateLocationsEdges();
		}
	}

	function updateLocationsEdges() {
		//console.log("updateLocationsEdges");
		const alledges = edgesLayer.selectAll(".edges").data(graphData.graphEdges.Array);
		const defaultshift = graphData.defaultNodeSize / 2.5;

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

				graphBgLayer
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
				graphBgLayer
					.select("#edgebg_" + d.dbId)
					.attr("transform", () => {
						return transform;
					})
					.attr("d", "M 0 0 "+ diagLine.length + " 0 Z");
			}
		});

		alledges.attr("visibility", "visible");
	}

	function updateNodeSelection(d:SimNode, isselected: boolean, showdetails: boolean) {
		//console.log({source: "updateNodeSelection", d: d});
		d.selected = isselected;
		if (isselected) {
			graphData.addSelection(d).Commit();
			if (showdetails) {
				graphData.detailsItems.Add(d).Commit();
			}
		} else {
			graphData.removeSelection(d).Commit();
			graphData.detailsItems.Remove(d).Commit();
		}
	}

	function removeNode(node: SimNode) {
		removeNodeIds([node.dbId]);
	}

	function removeNodeIds(ids: string[]) {
		let message;
		if (ids.length === 1) {
			message = "Are you sure you want to remove this node?";
		} else {
			message = "Are you sure you want to remove the " + ids.length + " selected nodes?";
		}

		if (confirm(message)) {
			getDirectEdgesForNodeList(ids, (data: ResultSet) => {
				const edgeids: string[] = [];
				data.edges.forEach(edge => {
					edgeids.push(edge.dbId);
				});

				graphData.removeIds(ids, edgeids);

				updateNodeSizes();
			});
		}
	}

	function getDirectEdgesForNodeList(nodelist, callback) {
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
</script>