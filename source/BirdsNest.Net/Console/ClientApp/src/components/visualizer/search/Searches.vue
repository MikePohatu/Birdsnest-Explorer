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
	<div class="absolute-top-left">
		<transition name="searchpanes">
			<button
				v-show="!searchEnabled"
				id="searchMaximise"
				v-on:click="onExpandClicked"
				class="button absolute-top-left"
				aria-label="Show search"
				type="button"
				title="Search"
			>
				<i class="fas fa-search"></i>
			</button>
		</transition>
		<transition name="searchpanes">
			<SimpleSearch v-show="searchEnabled && simpleSearchMode" />
		</transition>
		<transition name="searchpanes">
			<AdvancedSearch v-show="searchEnabled && !simpleSearchMode" />
		</transition>
	</div>
</template>


<style scoped>
#searchMaximise {
	height: 40px;
	widows: 40px;
}

.searchpanes-enter-active,
.searchpanes-leave-active {
	transition: all 0.3s ease-in-out;
}

.searchpanes-enter,
.searchpanes-leave-to {
	opacity: 0;
	transform: translateY(-200px);
}
</style>

<style >
.searchpane {
	padding: 5px;
	background-color: rgba(255, 255, 255, 0.9);
}

.searchNotification {
	display: inline-block;
	height: 1.2em;
	min-width: 250px;
}

.searchContainer {
	margin: 0;
	margin-top: 0 !important;
	margin-bottom: 0 !important;
	padding: 0;
}

.searchbutton {
	height: 35px;
	width: 35px;
	margin: 2px;
}
.searchbutton-wide {
	height: 35px;
	width: 74px;
	margin: 2px;
}

.searchbutton-x-wide {
	height: 35px;
	width: 150px;
	margin: 2px;
}
</style>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import SimpleSearch from "./SimpleSearch.vue";
import AdvancedSearch from "./AdvancedSearch.vue";
import { SearchStorePaths } from "@/store/modules/SearchStore";

@Component({
	components: { SimpleSearch, AdvancedSearch },
})
// ({
//   mixins: [foundation]
// })
export default class Searches extends Vue {
	get searchEnabled(): boolean {
		return this.$store.state.visualizer.search.searchEnabled;
	}

	get simpleSearchMode(): boolean {
		return this.$store.state.visualizer.search.simpleSearchMode;
	}

	onExpandClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
	}
}
</script>