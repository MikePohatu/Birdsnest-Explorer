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
	<div v-foundation>
		<div class="grid-x align-left">
			<div id="advSearchcButtons" class="searchContainer">
				<button
					v-on:click="onMinimizeClicked"
					class="button searchbutton small"
					:aria-label="$t('phrase_Minimize_search')"
					type="button"
					:title="$t('phrase_Minimize_search')"
				>
					<i class="fas fa-angle-up"></i>
				</button>
				<button
					v-on:click="onModeToggleClicked"
					class="button searchbutton small"
					:aria-label="$t('phrase_Simple_mode')"
					type="button"
					:title="$t('phrase_Simple_mode')"
				>
					<i class="fas fa-compress-alt"></i>
				</button>
				<button
					v-on:click="onClearClicked"
					class="button searchbutton small"
					:aria-label="$t('phrase_Clear_search')"
					type="button"
					:title="$t('phrase_Clear_search')"
				>
					<i class="fas fa-ban"></i>
				</button>
				<button
					class="button searchbutton small"
					:aria-label="$t('phrase_Share_search')"
					type="button"
					:title="$t('phrase_Share_search')"
					v-on:click="onShareClicked"
				>
					<i class="fas fa-share-alt"></i>
				</button>
				<button
					v-on:click="onSearchClicked"
					class="button searchbutton-wide small"
					:aria-label="$t('word_Search')"
					type="button"
				>
					<span>{{ $t('word_Search') }}</span>
				</button>
			</div>
		</div>
	</div>
</template>


<style scoped>
#sharedialog {
	padding: 5px 15px;
	background: rgba(255, 255, 255, 1);
	opacity: 1;
	font-size: 0.8em;
}

.buttonrow {
	margin: 5px 0;
}
</style>

<script setup lang="ts">
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { useStore } from "@/store";
import { ref } from "vue";
import { vFoundation } from "@/mixins/foundation";

const store = useStore();
let searchRetry = ref(0);

function onMinimizeClicked(): void {
	store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
}

function onModeToggleClicked(): void {
	store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH_MODE);
}

function onClearClicked(): void {
	if (confirm("Are you sure you want to clear the search?")) {
		store.commit(SearchStorePaths.mutations.RESET);
	}
}

function onSearchClicked(): void {
	const search = store.state.visualizer.search.search;
	if (search.nodes.length > 0) {
		store.dispatch(SearchStorePaths.actions.SEARCH);
		searchRetry.value = 0;
	} else {
		searchRetry.value++;
		if (searchRetry.value > 2) {
			alert("You haven't added any items to the search");
		}
	}
}

function onShareClicked(): void {
	const search = store.state.visualizer.search.search;
	if (search.nodes.length > 0) {
		store.dispatch(SearchStorePaths.actions.UPDATE_SHARE);
	}
}
</script>