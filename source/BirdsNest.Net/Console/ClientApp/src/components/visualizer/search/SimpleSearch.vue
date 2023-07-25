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
	<div class="searchpane absolute-top-left">
		<div class="searchContainer">
			<div id="simpleQueryInput" class="input-group shrink margin-zero">
				<input
					id="simpleSearchTerm"
					v-model="term"
					@keyup.enter="onSearchClicked"
					v-on:input="onSearchChanged"
					list="simplecompletes"
					autocomplete="on"
					type="search"
					class="input-group-field"
					:placeholder="$t('word_Search')"
				/>
				<datalist id="simplecompletes">
					<option v-for="opt in autocompleteList" :key="opt">{{ opt }}</option>
				</datalist>
				<div class="input-group-button">
					<button
						v-on:click="onSearchClicked"
						class="button"
						:aria-label="$t('word_Search')"
						type="submit"
						:title="$t('word_Search')"
					>
						<i class="fas fa-search"></i>
					</button>
				</div>
			</div>
		</div>
		<div class="searchContainer">
			<button
				id="simpleSearchMinimize"
				v-on:click="onMinimizeClicked"
				class="button searchbutton small"
				:aria-label="$t('phrase_Minimize_search')"
				type="button"
				:title="$t('phrase_Minimize_search')"
			>
				<i class="fas fa-angle-up"></i>
			</button>
			<button
				id="modeToggle"
				v-on:click="onModeToggleClicked"
				class="button searchbutton small"
				:aria-label="$t('phrase_Advanced_mode')"
				type="button"
				:title="$t('phrase_Advanced_mode')"
			>
				<i class="fas fa-expand-alt"></i>
			</button>
			<SearchResults :id="'simpleresults'" />
		</div>
	</div>
</template>

<style scoped>
#simpleSearchTerm {
	width: 250px;
	min-width: 250px;
}
#simpleQueryInput {
	margin: 0 0 5px 0;
}
</style>

<script setup lang="ts">
import SearchResults from "./SearchResults.vue";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { Request, api } from "@/assets/ts/webcrap/apicrap";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { ref } from "vue";
import { useStore } from "@/store";

const debounceTimeout = 250;
const store = useStore();

let autocompleteList = ref<string[]>([]);
let term = ref("");
let autoCompleteCancelled = false;

const onSearchChanged = () => {
	autoCompleteCancelled = false;
	autocompleteDebounce();
};

const autocompleteDebounce = webcrap.misc.debounce(updateAutocomplete, debounceTimeout);

function updateAutocomplete(): void {
	if (autoCompleteCancelled) { return; }
	const url = "/api/graph/node/namevalues?searchterm=" + term.value;
	const newrequest: Request = {
		url: url,
		successCallback: data => {
			if (autoCompleteCancelled === false) {
				autocompleteList.value = data;
			}
		},
		errorCallback: () => {
			autocompleteList.value = [];
		},
	};
	api.get(newrequest);
}

function onMinimizeClicked(): void {
	store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
}

function onModeToggleClicked(): void {
	store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH_MODE);
}

function onSearchClicked(): void {
	store.dispatch(SearchStorePaths.actions.SIMPLE_SEARCH, term.value);

	//cancel the debounce & reset the autocomplete list to get it out of the users face
	autoCompleteCancelled = true;
	autocompleteList.value = [];
}

</script>