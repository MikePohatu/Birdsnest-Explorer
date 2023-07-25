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
		:class="[isSelected ? 'selected' : '', 'advsearchbutton clickable noselect']"
		v-on:click.stop="onClicked"
		v-on:dblclick.stop="onDblClicked"
	>
		<div>{{ condition.name }}.{{ condition.property }}</div>
		<div>{{ searchDeets }}</div>
	</div>
</template>

<script setup lang="ts">
	import { ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
	import { SearchStorePaths } from "@/store/modules/SearchStore";
	import { useStore } from "@/store";
	import { computed, ref } from "vue";
	
	const store = useStore();
	const props = defineProps({
		condition: {
			type: ValueCondition,
			required: true
		}
	});

	const condition = ref<ValueCondition>(props.condition);

	const isSelected = computed((): boolean => {
		return store.state.visualizer.search.selectedCondition === condition.value;
	});

	const searchDeets = computed((): string => {
		return (condition.value.not ? "Not " : "") + condition.value.operator + " " + condition.value.value + (condition.value.type === ConditionType.String && condition.value.caseSensitive ? "*" : "");
	});

	function onClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, condition.value);
	}

	function onDblClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, condition.value);
		store.commit(SearchStorePaths.mutations.Update.EDIT_CONDITION);
	}
</script>