<!-- Copyright (c) 2019-2020 "20Road"
20Road Limited [https://20road.com]

This file is part of BirdsNest.

BirdsNest is free software: you can redistribute it and/or modify
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
	<div v-if="condition !== null" class="dialogWrapper">
		<div id="conditionEdit" class="dialog">
			<fieldset class="fieldset">
				<legend>Condition</legend>
				<div id="editControls">
					<!-- Reference -->
					<div class="input-group">
						<span class="input-group-label">Referenced Item</span>
						<select class="input-group-field" v-model="condition.name">
							<option v-for="item in availableItems" :value="item.name" :key="item.name">{{ item.name }}</option>
						</select>
					</div>

					<!-- Property -->
					<div class="input-group" v-if="condition.name !== null">
						<span class="input-group-label">Property</span>
						<select
							class="small-8 input-group-field"
							v-model="condition.property"
							:disabled="!isLabelSet"
						>
							<option v-for="prop in properties" :key="prop" :value="prop">{{prop}}</option>
						</select>
					</div>

					<!-- booleans -->
					<div v-if="condition.property !== null && propertyType === 'boolean'" class="input-group">
						<span class="input-group-label">=</span>
						<div class="input-group-button">
							<select class="input-group-field" v-model="condition.value">
								<option value="TRUE">true</option>
								<option value="FALSE">false</option>
							</select>
						</div>
					</div>

					<!-- string input -->
					<div v-if="condition.property !== null && propertyType === 'string'" class="input-group">
						<div class="input-group-button">
							<select v-model="condition.operator" class="input-group-field">
								<option v-for="op in operators" :key="op" :value="op">{{op}}</option>
							</select>
						</div>
						<input
							id="searchStringInput"
							v-model="condition.value"
							class="input-group-field"
							autocomplete="on"
							type="search"
							placeholder="*"
							v-on:input="autocompleteDebounce"
							list="completes"
						/>
						<datalist id="completes">
							<option v-for="opt in autocompleteList" :key="opt">{{opt}}</option>
						</datalist>
					</div>

					<!-- number input -->
					<div v-if="condition.property !== null && propertyType === 'number'" class="input-group">
						<div class="input-group-button">
							<select v-model="condition.operator" class="input-group-field">
								<option v-for="op in operators" :key="op" :value="op">{{op}}</option>
							</select>
						</div>
						<input
							v-model.number="condition.value"
							class="input-group-field"
							type="search"
							autocomplete="off"
							placeholder="*"
						/>
					</div>

					<!-- Not -->
					<div
						v-if="propertyType === 'string' || propertyType === 'number'"
						class="grid-x grid-margin-x align-top"
						title="Invert search (not equal, not startswith)"
					>
						<div class="cell shrink">NOT:</div>
						<div class="cell auto">
							<div class="switch tiny" style="display: inline;">
								<input
									id="searchNot"
									v-model="condition.not"
									class="switch-input"
									type="checkbox"
									name="searchNot"
								/>
								<label class="switch-paddle" for="searchNot">
									<span class="show-for-sr">Not:</span>
								</label>
							</div>
						</div>
					</div>

					<!-- case sensitive -->
					<div
						v-if="propertyType === 'string'"
						id="searchCaseOptions"
						class="grid-x grid-margin-x align-top"
						title="*Case sensitive search is faster"
					>
						<div class="cell shrink">Case Sensitive:</div>
						<div class="cell auto">
							<div class="switch tiny" style="display: inline;">
								<input
									v-model="condition.caseSensitive"
									class="switch-input"
									id="searchCase"
									type="checkbox"
									name="searchCase"
								/>
								<label class="switch-paddle" for="searchCase">
									<span class="show-for-sr">Case Sensitive</span>
								</label>
							</div>
						</div>
					</div>

					<!-- alerts -->
					<div v-if="showAlert" id="alert">
						<i id="alertIcon" class="fas fa-exclamation-triangle"></i>
						<span id="alertMessage">{{alertMessage}}</span>
					</div>
				</div>
				<!-- close  -->
				<button
					v-on:click="onCloseClicked"
					class="close-button"
					aria-label="Close condition dialog"
					type="button"
				>
					<span>&times;</span>
				</button>
				<div class="grix-x align-middle">
					<button
						id="searchConditionSaveBtn"
						class="button small searchbutton-wide"
						aria-label="Save this condition"
						type="button"
						v-on:click="onSaveClicked"
						:disabled="!saveable"
					>Save</button>
					<button
						id="searchConditionDeleteBtn"
						class="alert button small searchbutton-wide"
						aria-label="Delete this condition"
						type="button"
						v-on:click="onDeleteClicked"
					>Delete</button>
					<button
						id="searchConditionCancelBtn"
						class="button small searchbutton-wide"
						aria-label="Cancel"
						type="button"
						v-on:click="onCloseClicked"
					>Cancel</button>
				</div>
			</fieldset>
		</div>
	</div>
</template>


<style scoped>

#alert {
	padding: 5px;
}

#alertIcon {
	height: 24px;
	width: 24px;
	margin: 0;
	color: orange;
}

#alertMessage {
	margin: 0 10px;
	padding: 5px;
	vertical-align: text-bottom;
}

/* override foundation 6 default for IE11 */
.input-group-field {
	flex-basis: auto;
}
</style>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import {
	ValueCondition,
	SearchItem,
	GetNode,
	GetEdge,
	ConditionOperators,
	copyCondition,
} from "@/assets/ts/visualizer/Search";
import { DataType } from "@/assets/ts/dataMap/DataType";
import { SearchItemType } from "@/assets/ts/visualizer/Search";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { SearchStorePaths } from "../../../store/modules/SearchStore";
import { api, Request } from "@/assets/ts/webcrap/apicrap";

@Component
export default class ValueConditionEdit extends Vue {
	@Prop({ type: Object as () => ValueCondition, required: true })
	source: ValueCondition;

	condition: ValueCondition = null;

	autocompleteList: string[] = [];
	alertMessage = "";
	showAlert = false;
	saveable = true;

	created(): void {
		this.condition = copyCondition(this.source);
		if (this.condition.property === "" && this.dataType !== null) {
			this.condition.property = this.dataType.default;
		}
	}

	get autocompleteDebounce(): Function {
		return webcrap.misc.debounce(this.updateAutocomplete, 250);
	}
	get availableItems(): SearchItem[] {
		const edges = this.$store.state.visualizer.search.search.edges;
		const nodes = this.$store.state.visualizer.search.search.nodes;
		return nodes.concat(edges);
	}

	get selectedItem(): SearchItem {
		let item: SearchItem = null;
		item = GetNode(this.condition.name, this.$store.state.visualizer.search.search);
		if (item === null) {
			item = GetEdge(this.condition.name, this.$store.state.visualizer.search.search);
		}
		if (item !== null && webcrap.misc.isNullOrWhitespace(item.label)) {
			this.alertMessage = "Selected item must have a type set";
			this.saveable = false;
			this.showAlert = true;
		} else {
			this.alertMessage = "";
			this.showAlert = false;
			this.saveable = true;
		}
		return item;
	}

	get dataType(): DataType {
		if (webcrap.misc.isNullOrWhitespace(this.condition.name)) {
			return null;
		}
		if (this.selectedItem === null) {
			return null;
		}
		if (webcrap.misc.isNullOrWhitespace(this.selectedItem.label)) {
			return null;
		}
		const types = this.$store.state.pluginManager.nodeDataTypes[this.selectedItem.label];
		if (types) {
			return types;
		} else {
			return this.$store.state.pluginManager.edgeDataTypes[this.selectedItem.label];
		}
	}

	get properties(): string[] {
		if (this.dataType === null) {
			return [];
		} else {
			return this.dataType.propertyNames;
		}
	}

	get propertyType(): string {
		if (this.dataType !== null && webcrap.misc.isNullOrWhitespace(this.condition.property) === false) {
			this.condition.type = this.dataType.properties[this.condition.property].type;
			return this.condition.type;
		} else {
			return null;
		}
	}

	get operators(): string[] {
		if (webcrap.misc.isNullOrWhitespace(this.propertyType)) {
			return [];
		} else {
			return ConditionOperators[this.propertyType];
		}
	}

	get isLabelSet(): boolean {
		if (this.selectedItem === null || webcrap.misc.isNullOrWhitespace(this.selectedItem.label)) {
			return false;
		} else {
			return true;
		}
	}

	updateAutocomplete(): void {
		let url: string;
		if (this.selectedItem.type === SearchItemType.SearchNode) {
			url =
				"/api/graph/node/values?type=" +
				this.selectedItem.label +
				"&property=" +
				this.condition.property +
				"&searchterm=" +
				this.condition.value;
		} else {
			url =
				"/api/graph/edge/values?type=" +
				this.selectedItem.label +
				"&property=" +
				this.condition.property +
				"&searchterm=" +
				this.condition.value;
		}
		
		const newrequest: Request = {
			url: url,
			successCallback: data => {
				//console.log(data);
				this.autocompleteList = data;
			},
			errorCallback: () => {
				//console.error(error);
				this.autocompleteList = [];
			},
		};
		api.get(newrequest);
	}

	onCloseClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.CANCEL_CONDITION_EDIT);
	}

	onSaveClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_VALUE_CONDITION, this.condition);
	}

	onDeleteClicked(): void {
		if (confirm("Are you sure you want to delete the condition?")) {
			this.$store.commit(SearchStorePaths.mutations.Delete.EDIT_VALUE_CONDITION);
		}
	}
}
</script>