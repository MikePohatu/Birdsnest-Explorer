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
					<span class="input-group-label">{{ $tc('word_Type') }}</span>
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

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { Dictionary } from "vue-router/types/router";
import { DataType } from "@/assets/ts/dataMap/DataType";
import { SearchNode, copyNode, ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "../../../store/modules/SearchStore";

@Component
export default class NodeEdit extends Vue {
	@Prop({ type: Object as () => SearchNode, required: true })
	source: SearchNode;

	node: SearchNode = null;

	created(): void {
		this.node = copyNode(this.source);
	}

	get nodeDisplayNames(): Dictionary<DataType> {
		if (this.$store.state.pluginManager === null) {
			return {};
		} else {
			return this.$store.state.pluginManager.nodeDisplayNames;
		}
	}

	get nodeDataTypes(): Dictionary<DataType> {
		if (this.$store.state.pluginManager === null) {
			return {};
		} else {
			return this.$store.state.pluginManager.nodeDataTypes;
		}
	}

	saveNode(): void {
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_NODE, this.node);
	}

	onCloseClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.CANCEL_ITEM_EDIT);
	}
	saveNodeAndCond(): void {
		const condition = new ValueCondition(ConditionType.String);
		condition.name = this.node.name;
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_NODE, this.node);
		this.$store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION, condition);
	}

	deleteNode(): void {
		if (
			confirm(
				"Are you sure you want to delete " +
					this.$store.state.visualizer.search.selectedItem.name +
					" and any associated conditions?"
			)
		) {
			this.$store.commit(SearchStorePaths.mutations.Delete.EDIT_NODE);
		}
	}
}
</script>