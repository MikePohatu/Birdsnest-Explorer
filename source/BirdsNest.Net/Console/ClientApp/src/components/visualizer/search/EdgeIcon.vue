<!-- Copyright (c) 2019-2023 "20Road"
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
	<div class="grid-x align-center">
		<!-- Up arrow or arrow shaft -->
		<div v-show="!isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-arrow-up arrowpart" />
		</div>
		<div v-show="isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-minus shaft arrowpart" />
		</div>

		<div
			:class="['cell advsearchbutton edgebutton clickable noselect', edge.label, [isSelectedItem ? 'selected' : '']]"
			v-on:click.stop="onEdgeClicked"
			v-on:dblclick.stop="onEdgeDblClicked"
		>{{text}}</div>

		<!-- Down arrow or arrow shaft -->
		<div v-show="isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-arrow-down arrowpart" />
		</div>
		<div v-show="!isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-minus shaft arrowpart" />
		</div>
	</div>
</template>

<style scoped>
.arrow .shaft {
	transform: rotate(90deg);
}

.arrowpart {
	background: none;
	width: fit-content;
}

.arrow {
	background: none;
	margin-top: 0;
	margin-bottom: 0;
	display: contents;
	padding: 0;
}

.edgebutton {
	margin-top: 0;
	margin-bottom: 0;
}
</style>

<script setup lang="ts">
import { SearchEdge } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from '@/store/modules/SearchStore';
import webcrap from '@/assets/ts/webcrap/webcrap';
import { computed } from "@vue/reactivity";
import { useStore } from "@/store";
	const props = defineProps({
		edge: {type: Object, required: true }
	});
	const edge = props.edge as SearchEdge;
	const store = useStore();

	const isDirRight = computed((): boolean => {
		return edge.direction === ">";
	});

	const text = computed((): string => {
		let outtext = edge.name;
		if (webcrap.misc.isNullOrWhitespace(edge.label) === false) {
			outtext += " :" + edge.label;
		} 

		if (edge.min === -1) {
			outtext += " *";
		} else {
			outtext += " " + edge.min + ".." + edge.max;
		}

		return outtext;
	});

	function onEdgeClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, edge);
	}

	function onEdgeDblClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, edge);
		store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
	}

	const isSelectedItem = computed((): boolean =>{
		return store.state.visualizer.search.selectedItem === edge;
	});
</script>