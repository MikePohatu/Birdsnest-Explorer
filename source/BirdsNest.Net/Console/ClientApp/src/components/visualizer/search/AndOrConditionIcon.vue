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
		v-if="!(isRoot && isEmptyRoot)"
		:class="[isRoot ? 'rootcondition' : '', isEmptyRoot && !isRoot ? 'emptyroot': '', isSelected ? 'selected' : '', 'clickable andor']"
		v-on:click.stop="onClicked"
		v-on:dblclick.stop="onDblClicked"
	>
		<div v-for="(cond, index) in conditions" :key="cond.id">
			<AndOrConditionIcon v-if="isAndOr(cond)" :condition="(cond as AndOrCondition)" />
			<ValueConditionIcon v-else :condition="(cond as ValueCondition)" />
			<div v-if="index < conditions.length - 1">{{ condition.type }}</div>
		</div>

		<button v-if="!isEmptyRoot" class="addBtn" type="button" v-on:click.stop="onAddClicked">
			<i class="icon clickable far fa-plus-square"></i>
		</button>
	</div>
</template>

<style scoped>
.andor {
	border-width: 2px;
	padding-top: 4px;
	padding-bottom: 4px;
	padding-right: 10px;
	padding-left: 5px;
	min-height: 25px;
	position: relative;
	border-style: solid;
	text-align: center;
	text-decoration: none;
	font-size: 0.8rem;
	margin-top: 0;
	margin-bottom: 0;
}

.andor.selected {
	border-width: 4px;
	padding-top: 2px;
	padding-bottom: 2px;
	padding-right: 8px;
	padding-left: 3px;
}

.addBtn {
	position: absolute;
	padding: 0;
	margin: 0;
	top: 50%;
	left: 100%;
	transform: translate(-50%, -50%);
	background-color: white;
}

.addBtn .icon {
	padding: 0;
	margin: 0;
}

.emptyroot {
	border-style: none;
}

.rootcondition {
	border-color: rgb(187, 187, 187);
}
</style>


<script setup lang="ts">
import { Condition, ConditionType, AndOrCondition, ValueCondition } from "@/assets/ts/visualizer/Search";
import ValueConditionIcon from "./ValueConditionIcon.vue";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { computed, ref, toRaw } from "@vue/reactivity";
import { useStore } from "@/store";

	const props = defineProps({condition: { type: Object, required: true }});
	const store = useStore();
	const condition = ref(props.condition as AndOrCondition);
	
	const name = computed((): string => {
		return condition.value.id;
	});

	const conditions = computed((): Condition[] => {
		return condition.value.conditions;
	});

	const isSelected = computed((): boolean => {
		return store.state.visualizer.search.selectedCondition === condition.value;
	});

	const isRoot = computed((): boolean => {
		// console.log({
		// 	condition: toRaw(condition), 
		// 	storeCondition: toRaw(store.state.visualizer.search.search.condition),
		// 	result: toRaw(condition) === toRaw(store.state.visualizer.search.search.condition)
		// 	});
		return toRaw(condition.value) === toRaw(store.state.visualizer.search.search.condition);
	});

	const isEmptyRoot = computed((): boolean => {
		return isRoot.value && condition.value.conditions.length === 0;
	});

	function isAndOr(cond: Condition): boolean {
		return cond.type === ConditionType.And || cond.type === ConditionType.Or;
	}

	function onClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, condition.value);
	}

	function onDblClicked(): void {
		store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, condition.value);
		store.commit(SearchStorePaths.mutations.Update.EDIT_CONDITION);
	}

	function onAddClicked(): void {
		store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION_PARENT, condition.value);
	}
</script>