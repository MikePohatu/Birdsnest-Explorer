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
		:class="[isSelected ? 'selected' : '', 'advsearchbutton clickable noselect']"
		v-on:click.stop="onClicked"
		v-on:dblclick.stop="onDblClicked"
	>
		<div>{{condition.name}}.{{condition.property}}</div>
		<div>{{ searchDeets }}</div>
	</div>
</template>

<script setup lang="ts">
	import { ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
	import { SearchStorePaths } from "@/store/modules/SearchStore";
	import { useStore } from "@/store";
	import { computed } from "vue";
	
	const store = useStore();
	const props = defineProps({
		condition: {
			type: ValueCondition,
			required: true
		}
	});

	const isSelected = computed((): boolean => {
		return store.state.visualizer.search.selectedCondition === props.condition;
	});

	const searchDeets = computed((): string => {
		return (props.condition.not ? "Not " : "") + props.condition.operator + " " + props.condition.value + (props.condition.type === ConditionType.String && props.condition.caseSensitive ? "*" : "");
	});

	function onClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, this.condition);
	}

	function onDblClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, this.condition);
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_CONDITION);
	}
</script>