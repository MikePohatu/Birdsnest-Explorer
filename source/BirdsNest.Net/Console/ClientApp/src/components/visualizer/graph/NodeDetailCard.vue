<!-- Copyright (c) 2019-2020 "20Road"
20Road Limited [https://20road.com]

This file is part of BirdsNest.

BirdsNest is free software: you can redistribute it and/or modify
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
	<div v-if="detailsLoaded" :id="'related'+node.DbId" class="detailcard pane">
		<div class="detaillist">
			<div class="grid-x align-middle">
				<div class="cell small-3">
					<u>
						<b>Details</b>
					</u>
				</div>
				<div class="cell small-1" title="Show/hide node">
					<a v-on:click="onEyeClicked()">
						<span v-show="node.enabled">
							<i class="cell far fa-eye small-2"></i>
						</span>
						<span v-show="!node.enabled">
							<i class="cell far fa-eye-slash small-2"></i>
						</span>
					</a>
				</div>
				<div
					v-if="relatedNodeIds.length > 0"
					class="cell small-1"
					:title="'Add ' + relatedNodeIds.length + ' related nodes'"
				>
					<a v-on:click="onExpandClicked()">
						<i class="cell fas fa-expand-arrows-alt small-2"></i>
					</a>
				</div>
				<div class="cell small-1" title="Remove from view">
					<a v-on:click="onRemoveClicked()">
						<div class="cell fas fa-trash-alt small-2"></div>
					</a>
				</div>
			</div>
			<b>Name:</b>
			{{node.name}}
			<br />
			<b>dbId:</b>
			{{node.dbId}}
			<br />
			<b>Types:</b>
			{{types}}
			<br />
			<b>Scope:</b>
			{{node.scope}}
			<br />
		</div>

		<div>
			<ul class="accordion" data-accordion data-allow-all-closed="true" data-multi-expand="true">
				<!-- Properties -->
				<li class="accordion-item" data-accordion-item>
					<a href="#" class="accordion-title">Properties ({{propertyNames.length}}):</a>
					<div class="accordion-content detaillist" data-tab-content>
						<div v-for="name in propertyNames" :key="name">
							<b>{{name}}:</b>
							{{node.properties[name]}}
							<br />
						</div>
					</div>
				</li>

				<!-- Related -->
				<li v-if="inCount > 0 || outCount > 0" class="accordion-item" data-accordion-item>
					<a href="#" class="accordion-title">Related ({{relatedNodeIds.length}}):</a>
					<div class="accordion-content detaillist" data-tab-content>
						<ul class="accordion" data-accordion data-allow-all-closed="true" data-multi-expand="true">
							<li v-if="inCount > 0" class="accordion-item" data-accordion-item>
								<a href="#" class="accordion-title">Inbound</a>
								<div class="accordion-content" data-tab-content>
									<ul
										v-for="listing in inListings"
										:key="listing.name"
										class="accordion"
										data-accordion
										data-allow-all-closed="true"
										data-multi-expand="true"
									>
										<li
											v-for="label in labels(listing)"
											:key="label"
											class="accordion-item"
											data-accordion-item
										>
											<a href="#" class="accordion-title">{{label}} ({{listing.labelledNodes[label].length}})</a>

											<div class="accordion-content" data-tab-content>
												<div v-for="nodeid in listing.labelledNodes[label]" :key="nodeid">
													{{details.relatedNodes[nodeid].name}}
													<a
														v-on:click="addNode(details.relatedNodes[nodeid])"
													>(+)</a>
												</div>
											</div>
										</li>
									</ul>
								</div>
							</li>

							<li v-if="outCount > 0" class="accordion-item" data-accordion-item>
								<a href="#" class="accordion-title">Outbound</a>
								<div class="accordion-content" data-tab-content>
									<ul
										v-for="listing in outListings"
										:key="listing.name"
										class="accordion"
										data-accordion
										data-allow-all-closed="true"
										data-multi-expand="true"
									>
										<li
											v-for="label in labels(listing)"
											:key="label"
											class="accordion-item"
											data-accordion-item
										>
											<a href="#" class="accordion-title">{{label}} ({{listing.labelledNodes[label].length}})</a>

											<div class="accordion-content" data-tab-content>
												<div v-for="nodeid in listing.labelledNodes[label]" :key="nodeid">
													{{details.relatedNodes[nodeid].name}}
													<a
														v-on:click="addNode(details.relatedNodes[nodeid])"
													>(+)</a>
												</div>
											</div>
										</li>
									</ul>
								</div>
							</li>
						</ul>
					</div>
				</li>
			</ul>
		</div>
	</div>
</template>


<style scoped>
.pane {
	min-width: 100px;
	background: white;
	border: 2px solid #d6d6d6;
	border-radius: 3px;
	padding: 5px;
	margin-left: 5px;
	margin-right: 5px;
	margin-top: 5px;
	margin-bottom: 5px;
}

.detailcard {
	border: 2px solid black;
	font-size: 12px;
	width: 270px;
	word-wrap: break-word;
	font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;
}

.detailcard .accordion-title {
	padding: 7px;
	/*border: 0px;*/
}

.detailcard .accordion-content {
	padding-left: 5px;
	padding-top: 0px;
	padding-bottom: 0px;
	padding-right: 0px;
	/*border: 0px;*/
}

.detailcard ul {
	margin: 0px;
}

.detaillist {
	padding-left: 7px;
	padding-top: 5px;
	padding-right: 5px;
	padding-bottom: 5px;
}

.currentActiveDetailCard {
	border-color: orange;
}
</style>


<script lang="ts">
import { bus, events } from "@/bus";
import { Component, Prop, Vue } from "vue-property-decorator";
import { RelatedDetails } from "@/assets/ts/dataMap/visualizer/RelatedDetails";
import { VForLabelledNodeList } from "@/assets/ts/dataMap/visualizer/VForLabelledNodeList";
import { Request, api } from "@/assets/ts/webcrap/apicrap";
import $ from "jquery";
import "foundation-sites";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { ApiNode } from "@/assets/ts/dataMap/ApiNode";

@Component
export default class NodeDetailCard extends Vue {
	@Prop({ type: Object, required: true })
	node: SimNode;

	mounted() {
		if (this.details === null) {
			this.updateDetails();
		} else {
			//double requestAnimationFrame required primarily for IE
			requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					$(this.$el).foundation();
				});
			});
		}
	}

	beforeDestoyed() {
		$(this.$el).foundation("_destroy");
	}

	get details(): RelatedDetails {
		return this.node.relatedDetails;
	}

	get detailsLoaded(): boolean {
		return this.details !== null;
	}
	get relatedNodeIds(): string[] {
		return Object.keys(this.details.relatedNodes);
	}

	get propertyNames(): string[] {
		return Object.keys(this.node.properties);
	}

	get types(): string {
		return this.node.labels.join(", ");
	}

	get inListings(): VForLabelledNodeList[] {
		return [this.details.inNodesByLabel, this.details.inNodesByEdgeLabel];
	}

	get inCount(): number {
		return (
			Object.keys(this.details.inNodesByLabel.labelledNodes).length +
			Object.keys(this.details.inNodesByEdgeLabel.labelledNodes).length
		);
	}

	get outListings(): VForLabelledNodeList[] {
		return [this.details.outNodesByLabel, this.details.outNodesByEdgeLabel];
	}

	get outCount(): number {
		return (
			Object.keys(this.details.outNodesByLabel.labelledNodes).length +
			Object.keys(this.details.outNodesByEdgeLabel.labelledNodes).length
		);
	}

	labels(listing): string[] {
		return Object.keys(listing.labelledNodes);
	}

	addNode(node: SimNode): void {
		bus.$emit(events.Notifications.Processing, "Adding node to view");
		this.$store.commit(VisualizerStorePaths.mutations.Add.PENDING_NODE, node);
	}

	onEyeClicked() {
		//bus.$emit(relatedDetailEvents.EYE_NODE_CLICKED, this.node);
		this.node.enabled = !this.node.enabled;
	}

	onExpandClicked() {
		const nodes: ApiNode[] = [];
		Object.keys(this.node.relatedDetails.relatedNodes).forEach(id => {
			nodes.push(this.node.relatedDetails.relatedNodes[id]);
		});

		if (nodes.length > 0) {
			bus.$emit(events.Notifications.Processing, "Adding related nodes to view");
			this.$store.commit(VisualizerStorePaths.mutations.Add.PENDING_NODES, nodes);
		}
	}

	onRemoveClicked() {
		bus.$emit(events.Visualizer.RelatedDetails.DeleteNodeClicked, this.node);
	}

	updateDetails(): void {
		const url = "/api/graph/node/related?id=" + this.node.dbId;

		const request: Request = {
			url: url,
			successCallback: (data: RelatedDetails[]) => {
				this.node.relatedDetails = data[0];
				this.$nextTick(() => {
					$(this.$el).foundation();
				});
			},
			errorCallback: () => {
				console.error("Error downloading detail data");
			},
		};
		api.get(request);
	}
}
</script>