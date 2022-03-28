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
	<div
		:class="[label, ' advsearchbutton nodebutton clickable noselect',[isSelectedItem ? 'selected' : '']]"
		v-on:dblclick.stop="onNodeDblClicked"
		v-on:click.stop="onNodeClicked"
	>{{text}}</div>
</template>

<script setup lang="ts">
import { SearchNode } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { computed } from "@vue/reactivity";
import { useStore } from "vuex";
import webcrap from '../../../assets/ts/webcrap/webcrap';

	const props = defineProps({node: { type: SearchNode, required: true }});
	const store = useStore();

	const text = computed((): string => {
		if (webcrap.misc.isNullOrWhitespace(props.node.label)) {
			return props.node.name;
		} else {
			return props.node.name + " :" + props.node.label;
		}
	});

	const label = computed((): string => {
		return props.node.label;
	});

	const isSelectedItem  = computed((): boolean => {
		return store.state.visualizer.search.selectedItem === props.node;
	});

	function onNodeClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, props.node);
	}

	function onNodeDblClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, props.node);
		store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
	}
</script>