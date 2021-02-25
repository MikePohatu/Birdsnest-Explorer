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
	<div>
		<div :id="'related' + node.DbId" class="detailcard pane">
			<div class="detaillist">
				<div class="grid-x align-middle">
					<div class="cell shrink detailsHeader">
						<u>
							<b>{{ $t("word_Details") }}</b>
						</u>
					</div>
					<div class="cell small-1" :title="$t('visualizer.details.show_hide_node')">
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
						class="cell small-1"
						:title="$tc('visualizer.details.add_related', relatedNodeCount)"
					>
						<a v-on:click="onExpandClicked()">
							<i class="cell fas fa-expand-arrows-alt small-2"></i>
						</a>
					</div>
					<div class="cell small-1" :title="$t('word_Refresh')">
						<a v-on:click="onRefreshClicked()">
							<div class="cell fas fa-redo-alt small-2"></div>
						</a>
					</div>
					<div class="cell small-1" :title="$t('phrase_Remove_from_view')">
						<a v-on:click="onRemoveClicked()">
							<div class="cell fas fa-trash-alt small-2"></div>
						</a>
					</div>
				</div>
				<b>{{ $t("word_Name") }}:</b>
				{{ node.name }}
				<br />
				<b>dbId:</b>
				{{ node.dbId }}
				<br />
				<b>{{ $tc("word_Type", node.labels.length) }}:</b>
				{{ types }}
				<br />
				<b>{{ $t("word_Scope") }}:</b>
				{{ node.scope }}
				<br />
			</div>

			<!-- Properties -->
			<ul id="propsAccordion" class="vertical menu accordion-menu" data-accordion-menu>
				<li>
					<a href="#">{{ $t("word_Properties") }} ({{ propertyNames.length }}):</a>
					<ul class="menu vertical nested">
						<div class="menu nested" v-for="name in propertyNames" :key="name">
							<b>{{ name }}:</b>
							{{ node.properties[name] }}
							<br />
						</div>
					</ul>
				</li>
			</ul>

			<!-- Related -->
			<ul id="relatedAccordion" class="vertical menu accordion-menu" data-accordion-menu>
				<li>
					<a v-if="!detailsLoaded" href="#">{{ $t("word_Related") }} ({{ $t("word_Loading") }})</a>
					<a v-else href="#">{{ $t("word_Related") }} ({{ relatedNodeCount }}):</a>

					<!-- inbound relationships -->
					<ul class="menu vertical nested">
						<li v-if="inCount > 0">
							<a href="#">{{ $t("word_Inbound") }}</a>
							<ul
								v-for="labelledNodeList in inLabelledNodeLists"
								:key="labelledNodeList.name"
								class="menu vertical nested"
							>
								<li v-for="label in labels(labelledNodeList)" :key="label">
									<a href="#">{{ label }} ({{ labelledNodeList.labelledNodes[label].length }})</a>

									<ul class="menu vertical nested">
										<li
											v-for="labelledNode in getNodeSublistByLabel(labelledNodeList, label)"
											:key="labelledNode.dbId"
											class="menu nested"
											:title="labelledNode.labels.join(', ')"
										>
											{{ labelledNode.name }}<a v-on:click="addNode(labelledNode)" class="plus">(+)</a>
										</li>
										<li v-if="isExpanded(labelledNodeList, label) === false" class="menu nested">
											<a href="#" v-on:click="expandLabel(labelledNodeList, label)">{{ $t("word_More...") }}</a>
										</li>
									</ul>
								</li>
							</ul>
						</li>

						<!-- outbound relationships -->
						<li v-if="outCount > 0">
							<a href="#">{{ $t("word_Outbound") }}</a>
							<ul
								v-for="labelledNodeList in outLabelledNodeLists"
								:key="labelledNodeList.name"
								class="menu vertical nested"
							>
								<li v-for="label in labels(labelledNodeList)" :key="label">
									<a href="#">{{ label }} ({{ labelledNodeList.labelledNodes[label].length }})</a>

									<ul class="menu vertical nested">
										<li
											v-for="labelledNode in getNodeSublistByLabel(labelledNodeList, label)"
											:key="labelledNode.dbId"
											class="menu nested"
											:title="labelledNode.labels.join(', ')"
										>
											{{ labelledNode.name }}<a v-on:click="addNode(labelledNode)" class="plus">(+)</a>
										</li>
										<li v-if="isExpanded(labelledNodeList, label) === false" class="menu nested">
											<a href="#" v-on:click="expandLabel(labelledNodeList, label)">{{ $t("word_More...") }}</a>
										</li>
									</ul>
								</li>
							</ul>
						</li>
					</ul>
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

.detailcard a {
	padding: 7px;
	/*border: 0px;*/
}

.detailcard .accordion-menu .nested.is-accordion-submenu {
	margin-left: 5px;
}

.detailcard .accordion-menu .nested.is-accordion-submenu-item {
	margin-left: 5px;
	margin-top: 0px;
	margin-bottom: 0px;
	margin-right: 0px;
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

.detailsHeader {
	margin-right: 0.5rem;
}

.accordion-menu .is-accordion-submenu a.plus {
	padding-top: 0.3em;
}
</style>


<script lang="ts">
import { bus, events } from "@/bus";
import { Component, Prop, Vue } from "vue-property-decorator";
import { RelatedDetails } from "@/assets/ts/dataMap/visualizer/RelatedDetails";
import { VForLabelledNodeList } from "@/assets/ts/dataMap/visualizer/VForLabelledNodeList";
import { Request, api } from "@/assets/ts/webcrap/apicrap";
import $ from "jquery";
import "foundation-sites/dist/js/foundation.es6";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { ApiNodeSimple } from "@/assets/ts/dataMap/ApiNodeSimple";
import { rootPaths } from "@/store";
import Loading from "@/components/Loading.vue";
import { Dictionary } from "vue-router/types/router";

@Component({
	components: { Loading },
})
export default class NodeDetailCard extends Vue {
	@Prop({ type: Object, required: true })
	node: SimNode;
	relatedNodeCount = 0;
	initRun = false;
	labelExpands: Dictionary<boolean> = {};

	mounted() {
		if (this.node.relatedDetails === null) {
			$("#relatedAccordion").foundation();
			$("#propsAccordion").foundation();
			this.initDetails();
		} else {
			this.relatedNodeCount = this.node.relatedDetails.relatedCount;
			this.refreshExapnds(this.node.relatedDetails);
			$("#relatedAccordion").foundation();
			$("#propsAccordion").foundation();

			//If you don't delay here, the full accordion will build before the
			//detail card will display
			setTimeout(() => {
				this.initRun = true;
				this.reInitAccordions();
			}, 500);
		}
	}

	beforeDestoyed() {
		$("#relatedAccordion").foundation("_destroy");
		$("#propsAccordion").foundation("_destroy");
	}

	//this function is primarily to provide logic based on the initRun variable
	//which provides a delay when needed for rendering to happen in the right sequence
	get details(): RelatedDetails {
		if (this.initRun) {
			return this.node.relatedDetails;
		} else {
			return null;
		}
	}

	get detailsLoaded(): boolean {
		return this.details !== null;
	}

	get propertyNames(): string[] {
		return Object.keys(this.node.properties);
	}

	get types(): string {
		return this.node.labels.join(", ");
	}

	get inLabelledNodeLists(): VForLabelledNodeList[] {
		if (this.details === null) {
			return [];
		} else {
			return [this.details.inNodesByLabel, this.details.inNodesByEdgeLabel];
		}
	}

	get inCount(): number {
		if (this.details === null) {
			return 0;
		} else {
			return (
				Object.keys(this.details.inNodesByLabel.labelledNodes).length +
				Object.keys(this.details.inNodesByEdgeLabel.labelledNodes).length
			);
		}
	}

	get outLabelledNodeLists(): VForLabelledNodeList[] {
		if (this.details === null) {
			return [];
		} else {
			return [this.details.outNodesByLabel, this.details.outNodesByEdgeLabel];
		}
	}

	get outCount(): number {
		if (this.details === null) {
			return 0;
		} else {
			return (
				Object.keys(this.details.outNodesByLabel.labelledNodes).length +
				Object.keys(this.details.outNodesByEdgeLabel.labelledNodes).length
			);
		}
	}

	labels(labelledNodeList: VForLabelledNodeList): string[] {
		return Object.keys(labelledNodeList.labelledNodes);
	}

	addNode(node: ApiNodeSimple): void {
		bus.$emit(events.Notifications.Processing, "Adding node to view");
		this.$store.dispatch(VisualizerStorePaths.actions.Request.NODE_ID, node.dbId);
	}

	onEyeClicked(): void {
		this.node.enabled = !this.node.enabled;
	}

	onExpandClicked(): void {
		bus.$emit(events.Notifications.Processing, "Adding related nodes to view");
		this.$store.dispatch(VisualizerStorePaths.actions.Request.RELATED_NODES, this.node.dbId);
	}

	onRemoveClicked(): void {
		bus.$emit(events.Visualizer.RelatedDetails.DeleteNodeClicked, this.node);
	}

	reInitAccordions(): void {
		this.$nextTick(() => {
			// eslint-disable-next-line
			// @ts-ignore
			Foundation.reInit($("#relatedAccordion")); // eslint-disable-line no-undef
		});
	}

	onRefreshClicked(): void {
		const url = "/api/graph/node/related?id=" + this.node.dbId;

		const request: Request = {
			url: url,
			successCallback: (data: RelatedDetails[]) => {
				const updateddata = data[0];
				this.node.name = updateddata.node.name;
				this.node.properties = updateddata.node.properties;
				this.node.scope = updateddata.node.scope;
				this.node.labels = updateddata.node.labels;
				this.relatedNodeCount = updateddata.relatedCount;
				this.node.relatedDetails = updateddata;
				this.refreshExapnds(updateddata);
				this.reInitAccordions();
			},
			errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
				this.$store.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
				bus.$emit(events.Notifications.Error, "Error refreshing node info: " + error);
			},
		};

		this.$nextTick(() => {
			this.node.relatedDetails = null;
			api.get(request);
		});
	}
	
	initDetails(): void {
		const url = "/api/graph/node/related?id=" + this.node.dbId;
		const request: Request = {
			url: url,
			successCallback: (data: RelatedDetails[]) => {
				const updateddata = data[0];
				this.node.relatedDetails = updateddata;
				this.relatedNodeCount = updateddata.relatedCount;
				this.refreshExapnds(updateddata);
				this.reInitAccordions();
			},
			errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
				// eslint-disable-next-line
				this.$store.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
				bus.$emit(events.Notifications.Error, "Error updating node related details: " + error);
			},
		};
		api.get(request);
		this.initRun = true;
	}


	//Menu/list expansion code
	getNodeSublistByLabel(list: VForLabelledNodeList, label: string): ApiNodeSimple[] {
		if (Object.prototype.hasOwnProperty.call(list.labelledNodes, label) == false) {
			console.error(label + " not found in list " + list.name);
			return [];
		}

		const array = list.labelledNodes[label];
		if (this.isExpanded(list, label) || array.length < 10) {
			return array;
		} else {
			return array.slice(0, 9);
		}
	}

	getExpandName(list: VForLabelledNodeList, label: string): string {
		return list.name + label;
	}

	isExpanded(list: VForLabelledNodeList, label: string): boolean {
		const expLabel = this.getExpandName(list, label);

		if (Object.prototype.hasOwnProperty.call(this.labelExpands, expLabel) == false) {
			console.error(expLabel + " not found in labelExpands list ");
			console.log({
				labelExpands: this.labelExpands,
				expLabel: expLabel
			});
			return false;
		} else if (list.labelledNodes[label].length < 10) {
			return true;
		} else {
			return this.labelExpands[expLabel];
		}
	}

	expandLabel(list: VForLabelledNodeList, label: string) {
		this.labelExpands[this.getExpandName(list, label)] = true;
		this.reInitAccordions();
	}

	refreshExapnds(updateddata: RelatedDetails) {
		this.labelExpands = {};

		const lists: VForLabelledNodeList[] = [
			updateddata.inNodesByEdgeLabel,
			updateddata.outNodesByEdgeLabel,
			updateddata.inNodesByLabel,
			updateddata.outNodesByLabel,
		];

		lists.forEach((list: VForLabelledNodeList) => {
			Object.keys(list.labelledNodes).forEach((label: string) => {
				this.$set(this.labelExpands, this.getExpandName(list, label), false);
			});
		});
	}
}
</script>