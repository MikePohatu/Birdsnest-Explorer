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
	<div ref="templateRoot" id="results" v-bind:class="{ hidden: !showResults }">
		<div v-bind:class="{ hidden: zeroResults } ">
			<div>
				{{ $t('word_Found') }}
				<a :data-toggle="dropdownId">{{ resultCount }}</a>
				{{ $t('word_results') }}.
				<a v-on:click="onAddToViewClicked">{{ $t('phrase_Add_to_view') }}</a>
			</div>

			<div
				:id="dropdownId"
				class="scrollable dropdown-pane"
				data-dropdown
				data-auto-focus="true"
				data-close-on-click="true"
			>
				<SearchResultsList v-if="resultCount > 0"  />
			</div>
		</div>
		<div
			v-if="(searchNotification !== null)"
			v-bind:class="{ loading: isSearching }"
		>{{ searchNotification }}</div>
	</div>
</template>


<style scoped>
#results {
	height: auto;
	min-width: 200px;
	margin: 5px;
}

.dropdown-pane {
	padding: 0.5rem !important;
	width: unset;
	min-width: 300px;
}
</style>

<script setup lang="ts">
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { computed, ref } from "vue";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import { initFoundationMounted } from "@/mixins/foundation";
import { Notify } from "@/assets/ts/Notifications";
import SearchResultsList from "./SearchResultsList.vue";  
	
const templateRoot = ref(null);
initFoundationMounted(templateRoot);

const props = defineProps({ id: { type: String, required: true } });
const store = useStore();
const { t } = useI18n();

const allLabel = computed<string>(()=> {
	return t('visualizer.search.ALL');
})

const dropdownId = computed<string>(() => {
	return "dropdown-" + props.id;
});

const resultCount = computed<number>(()=> {
	const count = store.state.visualizer.search.results === null ? 0 : store.state.visualizer.search.results.nodes.length;
	return count;
});

const showResults = computed<boolean>(() => {
	return (store.state.visualizer.search.results !== null) || (searchNotification.value !== null);
});

const zeroResults = computed<boolean>(() => {
	const results = store.state.visualizer.search.results;
	if (results === null) {
		return true;
	}
	return results.nodes.length === 0;
});

const searchNotification = computed<string>(() => {
	return store.state.visualizer.search.statusMessage;
});

const isSearching = computed<boolean>(() => {
	return store.state.visualizer.search.isSearching;
});

function onAddToViewClicked(): void {
	let proceed = true;

	if (store.state.visualizer.search.results.nodes.length > 300) {
		proceed = confirm(t('visualizer.search.count_warning').toString());
	}

	if (proceed && store.state.visualizer.search.results.nodes.length > 0) {
		Notify.Processing(t('visualizer.search.adding_results'));
		store.commit(VisualizerStorePaths.mutations.Add.PENDING_RESULTS, store.state.visualizer.search.results);
		store.commit(SearchStorePaths.mutations.Delete.RESULTS);
	}
}
</script>