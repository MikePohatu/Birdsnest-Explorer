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
	<div v-if="condition.value !== null" class="dialogWrapper">
		<div id="conditionEdit" class="dialog">
			<fieldset class="fieldset">
				<legend>{{ $t('word_Condition') }}</legend>
				<div id="editControls">
					<!-- Reference -->
					<div class="input-group">
						<span class="input-group-label">{{ $t('phrase_Referenced_Item') }}</span>
						<select
							class="input-group-field"
							v-model="condition.name"
							v-on:change="onSelectedItemChanged"
						>
							<option v-for="item in availableItems" :value="item.name" :key="item.name">{{ item.name }}</option>
						</select>
					</div>

					<!-- Property -->
					<div class="input-group" v-if="condition.name !== null">
						<span class="input-group-label">{{ $t('word_Property') }}</span>
						<select
							class="small-8 input-group-field"
							v-model="condition.property"
							:disabled="!isLabelSet"
							v-on:change="onPropertyChanged"
						>
							<option v-for="prop in properties" :key="prop" :value="prop">{{ prop }}</option>
						</select>
					</div>

					<!-- booleans -->
					<div v-if="condition.property !== null && propertyType === 'boolean'" class="input-group">
						<span class="input-group-label">=</span>
						<div class="input-group-button">
							<select class="input-group-field" v-model="condition.value">
								<option value="TRUE">{{ $t('word_true') }}</option>
								<option value="FALSE">{{ $t('word_false') }}</option>
							</select>
						</div>
					</div>

					<!-- string input -->
					<div v-if="condition.property !== null && propertyType === 'string'" class="input-group">
						<div class="input-group-button">
							<select v-model="condition.operator" class="input-group-field">
								<option v-for="op in operators" :key="op" :value="op">{{ op }}</option>
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
							<option v-for="opt in autocompleteList" :key="opt">{{ opt }}</option>
						</datalist>
					</div>

					<!-- number input -->
					<div v-if="condition.property !== null && propertyType === 'number'" class="input-group">
						<div class="input-group-button">
							<select v-model="condition.operator" class="input-group-field">
								<option v-for="op in operators" :key="op" :value="op">{{ op }}</option>
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
						:key="$t('visualizer.search.not_descript')"
					>
						<div class="cell shrink">{{ $t('word_Not') }}:</div>
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
									<span class="show-for-sr">{{ $t('word_Not') }}</span>
								</label>
							</div>
						</div>
					</div>

					<!-- case sensitive -->
					<div
						v-if="propertyType === 'string'"
						id="searchCaseOptions"
						class="grid-x grid-margin-x align-top"
						:title="$t('visualizer.search.case_sensitive_descript')"
					>
						<div class="cell shrink">{{ $t('phrase_Case_Sensitive') }}:</div>
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
									<span class="show-for-sr">{{ $t('phrase_Case_Sensitive') }}</span>
								</label>
							</div>
						</div>
					</div>

					<!-- alerts -->
					<div v-if="showAlert" id="alert">
						<i id="alertIcon" class="fas fa-exclamation-triangle"></i>
						<span id="alertMessage">{{ alertMessage }}</span>
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
						:aria-label="$t('word_Save')"
						type="button"
						v-on:click="onSaveClicked"
						:disabled="!saveable"
					>{{ $t('word_Save') }}</button>
					<button
						id="searchConditionDeleteBtn"
						class="alert button small searchbutton-wide"
						:aria-label="$t('word_Delete')"
						type="button"
						:disabled="!deleteable"
						v-on:click="onDeleteClicked"
					>{{ $t('word_Delete') }}</button>
					<button
						id="searchConditionCancelBtn"
						class="button small searchbutton-wide"
						:aria-label="$t('word_Cancel')"
						type="button"
						v-on:click="onCloseClicked"
					>{{ $t('word_Cancel') }}</button>
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

<script setup lang="ts">
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
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { SearchEdge } from "@/assets/ts/visualizer/Search";
import { computed, onMounted, ref } from "vue";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const props = defineProps({
	source: ValueCondition
})

const store = useStore();
const condition = ref<ValueCondition>(copyCondition(props.source));
const selectedItem = ref<SearchItem>(null);

const autocompleteList = ref<string[]>([]);
const alertMessage = ref("");
const showAlert = ref(false);
const saveable = ref(true);
const deleteable = ref(true);

onMounted(()=> {
	onSelectedItemChanged();
	//onPropertyChanged();
});


const availableItems = computed((): SearchItem[] => {
	const edges = store.state.visualizer.search.search.edges;
	const nodes = store.state.visualizer.search.search.nodes;
	return nodes.concat(edges);
});

const dataType = computed((): DataType => {
	//console.log({source:"dataType", conditionName: condition.value.name, selectedItem.valueValue: selectedItem.value});
	if (webcrap.misc.isNullOrWhitespace(condition.value.name)) {
		return null;
	}
	if (selectedItem.value === null) {
		return null;
	}
	if (webcrap.misc.isNullOrWhitespace(selectedItem.value.label)) {
		return null;
	}
	const types = store.state.pluginManager.nodeDataTypes[selectedItem.value.label];
	if (types) {
		return types;
	} else {
		return store.state.pluginManager.edgeDataTypes[selectedItem.value.label];
	}
});

const properties = computed((): string[] => {
	if (dataType.value === null) {
		return [];
	} else {
		return dataType.value.propertyNames;
	}
});

function onPropertyChanged() {
	//console.log({source:"onPropertyChanged", dataType: dataType.value, condition: condition.value});
	if (dataType.value === null) {
		condition.value.type = null;
	} 
	else if (Object.prototype.hasOwnProperty.call(dataType.value.properties, [condition.value.property])) {
		condition.value.type = dataType.value.properties[condition.value.property].type;
	} 
	else {
		const firstkey = Object.keys(dataType.value.properties)[0];
		condition.value.type = dataType.value.properties[firstkey].type;
	}
}

const propertyType = computed((): string => {
	//console.log({source: "propertyType", dataType: dataType.value, condition.value: condition.value});
	if (dataType.value !== null && webcrap.misc.isNullOrWhitespace(condition.value.property) === false) {
		return condition.value.type;
	} else {
		return null;
	}
});

const operators = computed((): string[] => {
	if (webcrap.misc.isNullOrWhitespace(propertyType.value)) {
		return [];
	} else {
		return ConditionOperators[propertyType.value];
	}
});

const isLabelSet = computed((): boolean => {
	//console.log({source: "isLabelSet", selectedItemValue: selectedItem.value});
	if (selectedItem.value === null || webcrap.misc.isNullOrWhitespace(selectedItem.value.label)) {
		return false;
	} else {
		return true;
	}
});

function onSelectedItemChanged(): void {
	let item: SearchItem = null;
	let isNode = true;

	if (webcrap.misc.isNullOrWhitespace(condition.value.name)) {
		saveable.value = false;
		deleteable.value = false;
		showAlert.value = false;
		return;
	}

	item = GetNode(condition.value.name, store.state.visualizer.search.search);

	if (item === null) {
		item = GetEdge(condition.value.name, store.state.visualizer.search.search);
		isNode = false;
	}
	
	if (item !== null) {
		if (webcrap.misc.isNullOrWhitespace(item.label)) {
			alertMessage.value = t('visualizer.search.error_no_selected_type_set').toString();
			saveable.value = false;
			deleteable.value = false;
			showAlert.value = true;
		} else if (isNode === false) {
			const edge = item as SearchEdge;
			if ((edge.min === 1 && edge.max === 1) === false) {
				alertMessage.value = t('visualizer.search.multi_hop_cond_not_supported').toString();
				saveable.value = false;
				deleteable.value = false;
				showAlert.value = true;
			}
		}
		else {
			alertMessage.value = "";
			showAlert.value = false;
			saveable.value = true;
			deleteable.value = true;
		}
	}
	else {
		alertMessage.value = t('visualizer.search.error_getting_named_item').toString();
		showAlert.value = true;
		saveable.value = false;
		deleteable.value = false;
	}

	selectedItem.value = item;

	//if selected item type has changed, properties need to be re-checked
	if (dataType.value === null) {
		condition.value.property = "";
	}
	else if (webcrap.misc.isNullOrWhitespace(condition.value.property)) {
		condition.value.property = dataType.value.default;
	}
	else {
		if (dataType.value.propertyNames.includes(condition.value.property) === false) {
			condition.value.property = dataType.value.default;
		}
	}
}

const autocompleteDebounce: () => void = webcrap.misc.debounce(updateAutocomplete, 250);

function updateAutocomplete(): void {
	let url: string;

	if (selectedItem.value.type === SearchItemType.SearchNode) {
		url =
			"/api/graph/node/values?type=" +
			selectedItem.value.label +
			"&property=" +
			condition.value.property +
			"&searchterm=" +
			condition.value.value;
	} else {
		url =
			"/api/graph/edge/values?type=" +
			selectedItem.value.label +
			"&property=" +
			condition.value.property +
			"&searchterm=" +
			condition.value.value;
	}

	const newrequest: Request = {
		url: url,
		successCallback: data => {
			autocompleteList.value = data;
		},
		errorCallback: (error) => {
			// eslint-disable-next-line
			console.error(error);
			autocompleteList.value = [];
		},
	};
	api.get(newrequest);
}

function onCloseClicked(): void {
	store.commit(SearchStorePaths.mutations.CANCEL_CONDITION_EDIT);
}

function onSaveClicked(): void {
	store.commit(SearchStorePaths.mutations.Save.EDIT_VALUE_CONDITION, condition.value);
}

function onDeleteClicked(): void {
	if (confirm(t('confirm_value_condition_delete').toString())) {
		store.commit(SearchStorePaths.mutations.Delete.EDIT_VALUE_CONDITION);
	}
}
</script>