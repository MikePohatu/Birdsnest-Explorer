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
	<g
		ref="root"
		visibility="hidden"
		:id="'edge_' + edge.dbId"
		:class="['edges', edge.label, subTypes, { selected: edge.selected }, [edge.enabled ? 'enabled' : 'disabled']]"
		v-on:click.exact.prevent="onEdgeClicked"
		v-on:click.ctrl.exact.prevent="onEdgeCtrlClicked"
		v-on:contextmenu.prevent="onEdgeCtrlClicked"
	>
		<path :class="['arrows', edge.label, subTypes]"></path>
		<g class="edgelabel">
			<text
				class="noselect"
				dominant-baseline="text-bottom"
				text-anchor="middle"
				transform="translate(0,-5)"
			>
				{{
					edge.label
				}}
			</text>
		</g>
	</g>
</template>

<script setup lang="ts">
import * as d3 from "d3";
import { SimLink } from "@/assets/ts/visualizer/SimLink";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { bus, events } from "@/bus";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { computed, onMounted, ref } from "vue";
import { useStore } from "@/store";

const props = defineProps({
	edge: { type: Object, required: true }
});
const edge = props.edge as SimLink<SimNode>;
const store = useStore();
const root = ref(null);

const subTypes = computed((): string[] => {
	const subTypeProps = store.state.pluginManager.subTypeProperties;
	const subs = [];

	if (Object.prototype.hasOwnProperty.call(subTypeProps, edge.label)) {
		const subProps: string[] = subTypeProps[edge.label];

		subProps.forEach((subType: string) => {
			if (Object.prototype.hasOwnProperty.call(edge.properties, subType)) {
				const sub = webcrap.misc.cleanCssClassName(String(edge.properties[subType]));
				if (webcrap.misc.isNullOrWhitespace(sub) === false) {
					subs.push(edge.label + "-" + sub);
				}
			}
		});
	}

	return subs;
});

//assign the d3 datum to the element so simulation can use it
onMounted(() => {
	d3.select(root.value).datum(edge);
});

function onEdgeClicked() {
	bus.emit(events.Visualizer.Edge.EdgeClicked, edge);
}

function onEdgeCtrlClicked() {
	bus.emit(events.Visualizer.Edge.EdgeCtrlClicked, edge);
}
</script>