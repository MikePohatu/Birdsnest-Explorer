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
	<div
		:class="[label, ' advsearchbutton nodebutton clickable noselect', [isSelectedItem ? 'selected' : '']]"
		v-on:dblclick.stop="onNodeDblClicked"
		v-on:click.stop="onNodeClicked"
	>{{ text }}</div>
</template>

<script setup lang="ts">
import { SearchNode } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { computed, reactive } from "vue";
import { useStore } from "@/store";
import webcrap from '@/assets/ts/webcrap/webcrap';

const props = defineProps({ node: { type: Object, required: true } });
const node = reactive(props.node as SearchNode);
const store = useStore();

const text = computed((): string => {
	if (webcrap.misc.isNullOrWhitespace(node.label)) {
		return node.name;
	} else {
		return node.name + " :" + node.label;
	}
});

const label = computed((): string => {
	//console.log({source: "NodeIcon", node: node, label: node.label});
	return node.label;
});

const isSelectedItem = computed((): boolean => {
	return store.state.visualizer.search.selectedItem === node;
});

function onNodeClicked(): void {
	store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, node);
}

function onNodeDblClicked(): void {
	store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, node);
	store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
}
</script>