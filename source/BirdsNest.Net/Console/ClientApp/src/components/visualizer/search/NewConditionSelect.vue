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
	<div class="dialogWrapper">
		<div id="newCond" class="dialog">
			<fieldset class="fieldset">
				<legend>{{ $t('word_New') }}</legend>
				<div class="input-group">
					<span class="input-group-label small-4">{{ $t('word_Type') }}</span>
					<select v-model="type" class="small-8 input-group-field">
                        <option v-for="(value, name) in newTypes" :key="name" :value="value">{{ name }}</option>
					</select>
				</div>

				<button
					v-on:click="onCancelClicked"
					class="close-button"
					data-close
					:aria-label="$t('phrase_Close_dialog')"
					type="button"
				>
					<span aria-hidden="true">&times;</span>
				</button>
				<div class="align-middle">
					<div>
						<button
							class="button searchbutton-wide small"
							:aria-label="$t('word_Continue')"
							type="button"
							v-on:click="onOkClicked"
						>{{ $t('word_OK') }}</button>
						<button
							class="alert searchbutton-wide button small"
							:aria-label="$t('word_Cancel')"
							type="button"
							v-on:click="onCancelClicked"
						>{{ $t('word_Cancel') }}</button>
					</div>
				</div>
			</fieldset>
		</div>
	</div>
</template>

<script setup lang="ts">
import { NewConditionType, Condition, AndOrCondition, ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import { useStore } from "@/store";
import { computed, ref } from "vue";

let type = ref(NewConditionType.Value);
const store = useStore();

const newTypes = computed((): Dictionary<string> => {
	return NewConditionType;
});

function onCancelClicked(): void {
	store.commit(SearchStorePaths.mutations.CANCEL_NEW_CONDITION);
}

function onOkClicked(): void {
	let newcond: Condition;

	if (type.value === NewConditionType.Value) {
		newcond = new ValueCondition(ConditionType.String);
	} else {
		newcond = new AndOrCondition(type.value);
	}
	store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION, newcond);
}
</script>