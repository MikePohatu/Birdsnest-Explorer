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
	<path
		ref="root"
		:id="'edgebg_' + edge.dbId"
		visibility="visible"
		:class="['edgebg', {'selected': edge.selected }, {'edgebg-loop': edge.isLoop }]"
		v-on:click.exact.prevent="onEdgeClicked"
		v-on:click.ctrl.exact.prevent="onEdgeCtrlClicked"
		v-on:contextmenu.prevent="onEdgeCtrlClicked"
	></path>
</template>

<script setup lang="ts">
import { d3 } from "@/assets/ts/visualizer/d3";
import { SimLink } from '@/assets/ts/visualizer/SimLink';
import { SimNode } from '@/assets/ts/visualizer/SimNode';
import { bus, events } from "@/bus";
import { onMounted, ref } from "vue";


	const props = defineProps({ edge: { type: Object, required: true }});
	let edge = props.edge as SimLink<SimNode>;

	const root = ref(null);
	
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