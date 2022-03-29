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
					<div class="cell small-1" :title="$tc('visualizer.details.add_related', relatedNodeCount)">
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
						<li class="property" v-for="name in propertyNames" :key="name">
							<b>{{ name }}:</b>
							{{ node.properties[name] }}
							<br />
						</li>
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
											{{ labelledNode.name }}
											<a v-on:click="addNode(labelledNode)" class="plus">(+)</a>
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
											{{ labelledNode.name }}
											<a v-on:click="addNode(labelledNode)" class="plus">(+)</a>
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
	margin-right: 5px;
}

.detailcard .accordion-menu .nested.is-accordion-submenu-item {
	margin-left: 5px;
	margin-top: 0px;
	margin-bottom: 0px;
	margin-right: 5px;
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

.property {
	margin-left: 5px;
	margin-right: 5px;
}
</style>


<script setup lang="ts">
import { bus, events } from "@/bus";
import { RelatedDetails } from "@/assets/ts/dataMap/visualizer/RelatedDetails";
import { VForLabelledNodeList } from "@/assets/ts/dataMap/visualizer/VForLabelledNodeList";
import { Request, api } from "@/assets/ts/webcrap/apicrap";
import $ from "jquery";
import "foundation-sites";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { ApiNodeSimple } from "@/assets/ts/dataMap/ApiNodeSimple";
import { rootPaths } from "@/store";
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import { computed, nextTick, onMounted, ref } from "vue";
import { useStore } from "vuex";

// @Component({
// 	components: { Loading },
// })
const props = defineProps({ node: { type: Object, required: true } });
const node = props.node as SimNode;
const store = useStore();

let relatedNodeCount = ref(0);
let initRun = ref(false);
let labelExpands = ref<Dictionary<boolean>>({});

onMounted(() => {
	if (node.relatedDetails === null) {
		$("#relatedAccordion").foundation();
		$("#propsAccordion").foundation();
		initDetails();
	} else {
		relatedNodeCount.value = node.relatedDetails.relatedCount;
		refreshExapnds(node.relatedDetails);
		$("#relatedAccordion").foundation();
		$("#propsAccordion").foundation();

		//If you don't delay here, the full accordion will build before the
		//detail card will display
		setTimeout(() => {
			initRun.value = true;
			reInitAccordions();
		}, 500);
	}
});

//this function is primarily to provide logic based on the initRun variable
//which provides a delay when needed for rendering to happen in the right sequence
const details = computed((): RelatedDetails => {
	if (initRun) {
		return node.relatedDetails;
	} else {
		return null;
	}
});

const detailsLoaded = computed((): boolean => {
	return details !== null;
});

const propertyNames = computed((): string[] => {
	return Object.keys(node.properties);
});

const types = computed((): string => {
	return node.labels.join(", ");
});

const inLabelledNodeLists = computed((): VForLabelledNodeList[] => {
	if (details === null) {
		return [];
	} else {
		return [details.value.inNodesByLabel, details.value.inNodesByEdgeLabel];
	}
});

const inCount = computed((): number => {
	if (details === null) {
		return 0;
	} else {
		return (
			Object.keys(details.value.inNodesByLabel.labelledNodes).length +
			Object.keys(details.value.inNodesByEdgeLabel.labelledNodes).length
		);
	}
});

const outLabelledNodeLists = computed((): VForLabelledNodeList[] => {
	if (details === null) {
		return [];
	} else {
		return [details.value.outNodesByLabel, details.value.outNodesByEdgeLabel];
	}
});

const outCount = computed((): number => {
	if (details === null) {
		return 0;
	} else {
		return (
			Object.keys(details.value.outNodesByLabel.labelledNodes).length +
			Object.keys(details.value.outNodesByEdgeLabel.labelledNodes).length
		);
	}
});

function labels(labelledNodeList: VForLabelledNodeList): string[] {
	return Object.keys(labelledNodeList.labelledNodes);
}

function addNode(node: ApiNodeSimple): void {
	bus.emit(events.Notifications.Processing, "Adding node to view");
	store.dispatch(VisualizerStorePaths.actions.Request.NODE_ID, node.dbId);
}

function onEyeClicked(): void {
	node.enabled = !node.enabled;
}

function onExpandClicked(): void {
	bus.emit(events.Notifications.Processing, "Adding related nodes to view");
	store.dispatch(VisualizerStorePaths.actions.Request.RELATED_NODES, node.dbId);
}

function onRemoveClicked(): void {
	bus.emit(events.Visualizer.RelatedDetails.DeleteNodeClicked, node);
}

function reInitAccordions(): void {
	nextTick(() => {
		// eslint-disable-next-line
		// @ts-ignore
		Foundation.reInit($("#relatedAccordion")); // eslint-disable-line no-undef
	});
}

function onRefreshClicked(): void {
	const url = "/api/graph/node/related?id=" + node.dbId;

	const request: Request = {
		url: url,
		successCallback: (data: RelatedDetails[]) => {
			const updateddata = data[0];
			node.name = updateddata.node.name;
			node.properties = updateddata.node.properties;
			node.scope = updateddata.node.scope;
			node.labels = updateddata.node.labels;
			relatedNodeCount.value = updateddata.relatedCount;
			node.relatedDetails = updateddata;
			refreshExapnds(updateddata);
			reInitAccordions();
		},
		errorCallback: (jqXHR, status?: string, error?: string) => {
			store.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
			bus.emit(events.Notifications.Error, "Error refreshing node info: " + error);
		},
	};

	nextTick(() => {
		node.relatedDetails = null;
		api.get(request);
	});
}

function initDetails(): void {
	const url = "/api/graph/node/related?id=" + node.dbId;
	const request: Request = {
		url: url,
		successCallback: (data: RelatedDetails[]) => {
			const updateddata = data[0];
			node.relatedDetails = updateddata;
			relatedNodeCount.value = updateddata.relatedCount;
			refreshExapnds(updateddata);
			reInitAccordions();
		},
		errorCallback: (jqXHR?, status?: string, error?: string) => {
			// eslint-disable-next-line
			store.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
			bus.emit(events.Notifications.Error, "Error updating node related details: " + error);
		},
	};
	api.get(request);
	initRun.value = true;
}

//Menu/list expansion code
function getNodeSublistByLabel(list: VForLabelledNodeList, label: string): ApiNodeSimple[] {
	if (Object.prototype.hasOwnProperty.call(list.labelledNodes, label) == false) {
		console.error(label + " not found in list " + list.name);
		return [];
	}

	const array = list.labelledNodes[label];
	if (isExpanded(list, label) || array.length < 10) {
		return array;
	} else {
		return array.slice(0, 9);
	}
}

function getExpandName(list: VForLabelledNodeList, label: string): string {
	return list.name + label;
}

function isExpanded(list: VForLabelledNodeList, label: string): boolean {
	const expLabel = getExpandName(list, label);

	if (Object.prototype.hasOwnProperty.call(labelExpands.value, expLabel) == false) {
		console.error(expLabel + " not found in labelExpands list ");
		console.log({
			labelExpands: labelExpands.value,
			expLabel: expLabel,
		});
		return false;
	} else if (list.labelledNodes[label].length < 10) {
		return true;
	} else {
		return labelExpands.value[expLabel];
	}
}

function expandLabel(list: VForLabelledNodeList, label: string) {
	labelExpands.value[getExpandName(list, label)] = true;
	reInitAccordions();
}

function refreshExapnds(updateddata: RelatedDetails) {
	labelExpands.value = {};

	const lists: VForLabelledNodeList[] = [
		updateddata.inNodesByEdgeLabel,
		updateddata.outNodesByEdgeLabel,
		updateddata.inNodesByLabel,
		updateddata.outNodesByLabel,
	];

	lists.forEach((list: VForLabelledNodeList) => {
		Object.keys(list.labelledNodes).forEach((label: string) => {
			//$set(labelExpands.value, getExpandName(list, label), false);

			labelExpands.value[getExpandName(list, label)] = false;
		});
	});
}
</script>