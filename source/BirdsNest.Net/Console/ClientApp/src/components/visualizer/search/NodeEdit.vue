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
	<div v-if="node !== null" class="dialogWrapper">
		<div id="searchNodeEdit" class="dialog">
			<fieldset class="fieldset cell">
				<legend>{{ $t('word_Node') }}</legend>
				
				<div class="input-group">
					<span class="input-group-label">{{ $t('word_Identifier') }}</span>
					<input
						id="nodeIdentifier"
						type="search"
						autocomplete="off"
						placeholder="*"
						class="input-group-field"
						v-model="node.name"
					/>
				</div>

				<div class="input-group">
					<span class="input-group-label">{{ $t('word_Type') }}</span>
					<select id="nodeType" class="input-group-field" v-model="node.label">
						<option value="">*</option>
						<option
							v-for="(value, name) in nodeDisplayNames"
							:key="name"
							:value="value"
							:title="nodeDataTypes[value].description"
						>{{name}}</option>
					</select>
				</div>
			</fieldset>
			<button
				class="close-button"
				v-on:click="onCloseClicked"
				:aria-label="$t('phrase_Close_query_dialog')"
				type="button"
			>
				<span aria-hidden="true">&times;</span>
			</button>
			<div>
				<button
					v-on:click="saveNode"
					class="button searchbutton-wide small"
					:aria-label="$t('phrase_Save_this_node')"
					type="button"
				>{{ $t('word_Save') }}</button>
				<button
					v-on:click="saveNodeAndCond"
					class="button searchbutton-x-wide small"
					:aria-label="$t('phrase_Save_this_node_and_add_condition')"
					type="button"
				>{{ $t('phrase_Save_Add_Condition')}}</button>
				<button
					v-on:click="deleteNode"
					class="alert button searchbutton-wide small"
					:aria-label="$t('phrase_Delete_this_node')"
					type="button"
				>
					<span>{{ $t('word_Delete') }}</span>
				</button>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { Dictionary } from "lodash";
import { DataType } from "@/assets/ts/dataMap/DataType";
import { SearchNode, copyNode, ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "../../../store/modules/SearchStore";
import { useStore } from "@/store";
import { computed } from "vue";

	const props = defineProps({ source: {type: Object, required: true}});
	const source = props.source as SearchNode;

	let node: SearchNode = copyNode(source);
	const store = useStore();


	const nodeDisplayNames = computed((): Dictionary<DataType> => {
		if (store.state.pluginManager === null) {
			return {};
		} else {
			return store.state.pluginManager.nodeDisplayNames;
		}
	});

	const nodeDataTypes = computed((): Dictionary<DataType> => {
		if (store.state.pluginManager === null) {
			return {};
		} else {
			return store.state.pluginManager.nodeDataTypes;
		}
	});

	function saveNode(): void {
		store.commit(SearchStorePaths.mutations.Save.EDIT_NODE, node);
	}

	function onCloseClicked(): void {
		store.commit(SearchStorePaths.mutations.CANCEL_ITEM_EDIT);
	}
	function saveNodeAndCond(): void {
		const condition = new ValueCondition(ConditionType.String);
		condition.name = node.name;
		store.commit(SearchStorePaths.mutations.Save.EDIT_NODE, node);
		store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION, condition);
	}

	function deleteNode(): void {
		if (
			confirm(
				"Are you sure you want to delete " +
					store.state.visualizer.search.selectedItem.name +
					" and any associated conditions?"
			)
		) {
			store.commit(SearchStorePaths.mutations.Delete.EDIT_NODE);
		}
	}
</script>