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
	<div class="searchpane absolute-top-left">
		<div class="searchContainer">
			<div id="simpleQueryInput" class="input-group shrink margin-zero">
				<input
					id="simpleSearchTerm"
					v-model="term"
					@keyup.enter="onSearchClicked"
					v-on:input="autocompleteDebounce"
					list="simplecompletes"
					autocomplete="on"
					type="search"
					class="input-group-field"
					:placeholder="$t('word_Search')"
				/>
				<datalist id="simplecompletes">
					<option v-for="opt in autocompleteList" :key="opt">{{opt}}</option>
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

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import SearchResults from "./SearchResults.vue";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { Request, api } from "@/assets/ts/webcrap/apicrap";
import { SearchStorePaths } from "@/store/modules/SearchStore";

@Component({
	components: { SearchResults },
})
export default class SimpleSearch extends Vue {
	searchNotification = "";
	autocompleteList: string[] = [];
	term = "";

	updateAutocomplete(): void {
		const url = "/api/graph/node/namevalues?searchterm=" + this.term;
		const newrequest: Request = {
			url: url,
			successCallback: data => {
				this.autocompleteList = data;
			},
			errorCallback: () => {
				this.autocompleteList = [];
			},
		};
		api.get(newrequest);
	}

	get autocompleteDebounce(): Function {
		return webcrap.misc.debounce(this.updateAutocomplete, 250);
	}

	onMinimizeClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
	}

	onModeToggleClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH_MODE);
	}

	onSearchClicked(): void {
		this.$store.dispatch(SearchStorePaths.actions.SIMPLE_SEARCH, this.term);
		this.autocompleteList = [];  //reset the autocomplete list to get it out of the users face
	}
}
</script>