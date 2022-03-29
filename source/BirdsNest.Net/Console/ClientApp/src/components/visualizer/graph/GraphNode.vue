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
	<g ref="root"
		:id="'node_' + node.dbId"
		:class="['nodes', node.labels, subTypes, node.enabled ? 'enabled' : 'disabled', { selected: node.selected }]"
		draggable="true"
		cursor="pointer"
		:transform="translate"
		visibility="visible"
		v-on:click.exact.prevent="onNodeClicked"
		v-on:click.ctrl.exact.prevent="onNodeCtrlClicked"
		v-on:contextmenu.prevent="onNodeCtrlClicked"
	>
		<circle :id="'node_' + node.dbId + '_icon'" class="nodecircle" :r="radius" :cx="radius" :cy="radius"></circle>

		<text
			:font-size="radius"
			:x="radius"
			:y="radius * 1.05"
			dy="0.3em"
			:class="[node.labels, subTypes, 'icon', 'noselect']"
			v-html="icon"
		></text>
		<text class="nodetext noselect" text-anchor="middle" dominant-baseline="central" :transform="textTranslate">{{
			node.name
		}}</text>
		<text
			class="tooltip nodetext noselect"
			text-anchor="middle"
			dominant-baseline="central"
			:transform="tooltipTranslate"
			:font-size="radius"
			>{{ toolTip }}</text
		>

		<g v-show="pinned" class="pin" v-on:click.stop="onPinClicked">
			<circle :r="radius / 2.5" :cx="radius / 4" :cy="radius / 4"></circle>
			<text :x="radius / 4" :y="radius / 2.1" :font-size="radius / 2" class="icon">&#xf08d;</text>
		</g>
	</g>
</template>

<style scoped>
.tooltip {
	opacity: 0;
	font-size: 8px;
}

.nodes:hover .tooltip {
	opacity: 1;
	transition-delay: 0.7s;
}
</style>
<script setup lang="ts">
import { Dictionary } from "lodash";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { d3 } from "@/assets/ts/visualizer/d3";
import { bus, events } from "@/bus";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { computed, onMounted, ref } from "vue";
import { useStore } from "vuex";

	const props = defineProps({ node: { type: Object, required: true }});
	const node = props.node as SimNode;
	const store = useStore();
	const root = ref(null);

	let isSelected = ref(false);

	//assign the d3 datum to the element so simulation can use it
	onMounted(() => {
		//console.log(node);
		d3.select(root).datum(node);
	});

	const subTypes = computed((): string[] => {
		const finalSubTypes: string[] = [];
		const subTypeProps: Dictionary<string[]> = store.state.pluginManager.subTypeProperties;

		node.labels.forEach((label) => {
			if (Object.prototype.hasOwnProperty.call(subTypeProps, label)) {
				const subProps = subTypeProps[label];

				subProps.forEach((subProp: string) => {
					if (Object.prototype.hasOwnProperty.call(node.properties, subProp)) {
						const subType: string = webcrap.misc.cleanCssClassName(node.properties[subProp] as string);

						if (webcrap.misc.isNullOrWhitespace(subType) === false) {
							finalSubTypes.push(label + "-" + subType);
						}
					}
				});
			}
		});

		return finalSubTypes;
	});

	const icon = computed((): string => {
		const mappings = store.state.visualizer.iconMappings;
		return "&#x" + mappings.getMappingFromArray(node.labels) + ";";
	});

	const pinned = computed((): boolean => {
		return node.pinned;
	});

	const size = computed((): number => {
		return node.currentSize;
	});

	const radius = computed((): number => {
		return node.currentSize / 2;
	});

	const translate = computed((): string => {
		//console.log("translate(" + node.x + "," + node.y + ")");
		return "translate(" + node.currentX + "," + node.currentY + ")";
	});

	const textTranslate = computed((): string => {
		return "translate(" + radius.value + "," + (size.value + 10) + ")";
	});

	const tooltipTranslate = computed((): string => {
		return "translate(" + radius.value + "," + (size.value + 22) + ")";
	});

	const toolTip = computed((): string => {
		return node.labels.join(", ");
	});

	function onPinClicked(): void {
		bus.emit(events.Visualizer.Node.NodePinClicked, node.dbId);
	}

	function onNodeClicked(): void {
		bus.emit(events.Visualizer.Node.NodeClicked, node);
	}

	function onNodeCtrlClicked(): void {
		bus.emit(events.Visualizer.Node.NodeCtrlClicked, node);
	}
</script>