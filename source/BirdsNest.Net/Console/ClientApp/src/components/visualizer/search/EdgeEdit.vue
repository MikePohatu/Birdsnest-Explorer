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
	<div class="dialogWrapper">
		<div id="searchEdgeEdit" class="dialog">
			<fieldset class="fieldset small-4 cell">
				<legend>{{ $t('word_Relationship') }}</legend>

				<div class="input-group">
					<span class="input-group-label small-4">{{ $t('word_Identifier') }}</span>
					<input
						id="edgeIdentifier"
						type="search"
						autocomplete="off"
						placeholder="*"
						class="small-8 input-group-field"
						v-model="edge.name"
					/>
				</div>

				<div class="input-group">
					<span class="input-group-label small-3">{{ $tc('word_Type') }}</span>
					<select id="edgeType" class="small-9 input-group-field" v-model="edge.label">
						<option value selected>*</option>
						<option
							v-for="(value, name) in edgeDisplayNames"
							:key="name"
							:value="value"
							:title="edgeDataTypes[value].description"
						>{{name}}</option>
					</select>
				</div>

				<span>{{ $t('phrase_Limit_Hops') }}</span>

				<div class="switch tiny">
					<input
						class="switch-input"
						id="hopsSwitch"
						type="checkbox"
						name="hopsSwitch"
						v-model="limitHops"
					/>
					<label class="switch-paddle" for="hopsSwitch"></label>
				</div>

				<!-- sliders -->
				<div class="grid-x align-self-middle sliderwrapper">
					<div class="cell small-2">
						<div>{{ $t('word_Min') }}</div>
					</div>
					<div class="slidecontainer">
						<input
							type="range"
							min="0"
							:max="maxValue"
							v-model.number="min"
							class="slider"
							:disabled="!limitHops"
						/>
					</div>
					<div class="cell small-1">
						<input
							class="sliderval"
							type="number"
							min="0"
							:max="maxValue"
							v-model.number="min"
							:disabled="!limitHops"
							:title="$t('phrase_Minimum_hop_count')"
						/>
					</div>
				</div>

				<div class="grid-x align-self-middle sliderwrapper">
					<div class="cell small-2">
						<div>{{ $t('word_Max') }}</div>
					</div>
					<div class="slidecontainer">
						<input
							type="range"
							min="0"
							:max="maxValue"
							v-model.number="max"
							class="slider"
							:disabled="!limitHops"
						/>
					</div>
					<div class="cell small-1">
						<input
							class="sliderval"
							type="number"
							min="0"
							:max="maxValue"
							v-model.number="max"
							:disabled="!limitHops"
							:title="$t('phrase_Maximum_hop_count')"
						/>
					</div>
				</div>

				<!-- Direction -->
				<div>
					<span>{{ $t('word_Direction') }}</span>
					<span v-on:click="onDirectionClicked" class="clickable diricon">
						<span v-show="edgeDirRight">
							<i class="fas fa-arrow-right"></i>
						</span>
						<span v-show="!edgeDirRight">
							<i class="fas fa-arrow-left"></i>
						</span>
					</span>
				</div>
			</fieldset>

			<!-- close -->
			<button
				v-on:click="onCloseClicked"
				class="close-button"
				data-close
				:aria-label="$t('phrase_Close_query_dialog')"
				type="button"
			>
				<span aria-hidden="true">&times;</span>
			</button>
			<button
				v-on:click="onSaveEdge"
				id="searchEdgeSaveBtn"
				class="button searchbutton-wide small"
				:aria-label="$t('phrase_Save_relationship')"
				type="button"
			>{{ $t('word_Save') }}</button>
			<button
				v-on:click="onSaveAndAddCondClicked"
				class="button searchbutton-x-wide small"
				aria-label="Save and add condition"
				type="button"
			>{{ $t('phrase_Save_Add_Condition') }}</button>
		</div>
	</div>
</template>

<style scoped>
.sliderwrapper {
	margin: 10px;
}

.sliderval {
	padding: 3px;
	margin: 0 10px;
	width: 2rem;
	height: 2rem;
	vertical-align: middle;
	-moz-appearance: textfield;
}
.sliderval::-webkit-outer-spin-button,
input::-webkit-inner-spin-button {
	-webkit-appearance: none;
	margin: 0;
}

.slider {
	-webkit-appearance: none;
	width: 100%;
	height: 0.5rem;
	background: #cacaca;
	outline: none;
	-webkit-transition: opacity 0.15s ease-in-out;
	transition: opacity 0.15s ease-in-out;
	padding: 0;
	margin: 0;
}

.slider::-webkit-slider-thumb {
	-webkit-appearance: none;
	appearance: none;
	width: 0.8rem;
	height: 1.4rem;
	background: #1779ba;
	cursor: pointer;
}

.slider::-moz-range-thumb {
	width: 0.5rem;
	height: 1.4rem;
	background: #1779ba;
	cursor: pointer;
}

.diricon {
	margin: 0 10px;
}

/* override foundation 6 default for IE11 */
.input-group-field {
	flex-basis: auto;
}
</style>

<script setup lang="ts">
import { SearchEdge, copyEdge, ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
import { DataType } from "@/assets/ts/dataMap/DataType";
import { Dictionary } from "lodash";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { computed, ref } from "@vue/reactivity";
import { useStore } from "@/store";


	const props = defineProps({ source: { type: SearchEdge, required: true }});
	const store = useStore();

	let edge: SearchEdge = copyEdge(props.source);
	let maxValue = ref(20);

	const edgeDisplayNames = computed((): Dictionary<DataType> => {
		if (store.state.pluginManager === null) {
			return {};
		} else {
			return store.state.pluginManager.edgeDisplayNames;
		}
	});

	const edgeDataTypes = computed((): Dictionary<DataType> => {
		if (store.state.pluginManager === null) {
			return {};
		} else {
			return store.state.pluginManager.edgeDataTypes;
		}
	});

	const edgeDirRight = computed((): boolean => {
		if (edge.direction === ">") {
			return true;
		} else {
			return false;
		}
	});

	const limitHops = computed<boolean>({
		get (): boolean {
			return this.edge.min !== -1 && this.edge.max !== -1;
		},
		set (newValue) {
			if (!newValue) {
				this.min = -1;
				this.max = -1;
			} else {
				this.min = 1;
				this.max = 1;
			}
		}
	});

	const min = computed({
		get (): number {
			return this.edge.min;
		},
		set (newValue) {
			this.edge.min = newValue;
			if (this.edge.min > this.edge.max) {
				this.max = newValue;
			}
		}
	});
	
	const max = computed({
		get (): number {
			return this.edge.max;
		},
		set (newValue) {
			this.edge.max = newValue;
			if (this.edge.min > this.edge.max) {
				this.min = newValue;
			}
		}
	});
	

	function onSaveEdge(): void {
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_EDGE, this.edge);
	}

	function onDirectionClicked(): void {
		if (this.edge.direction === ">") {
			this.edge.direction = "<";
		} else {
			this.edge.direction = ">";
		}
	}

	function onCloseClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.CANCEL_ITEM_EDIT);
	}

	function onSaveAndAddCondClicked(): void {	
		const condition = new ValueCondition(ConditionType.String);
		condition.name = this.edge.name;
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_EDGE, this.edge);
		this.$store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION, condition);
	}
</script>