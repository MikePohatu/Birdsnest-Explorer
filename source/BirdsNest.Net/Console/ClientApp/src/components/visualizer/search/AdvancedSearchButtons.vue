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
	<div>
		<div class="grid-x align-left">
			<div id="advSearchcButtons" class="searchContainer">
				<button
					v-on:click="onMinimizeClicked"
					class="button searchbutton small"
					aria-label="Minimise search"
					type="button"
					title="Minimise search"
				>
					<i class="fas fa-angle-up"></i>
				</button>
				<button
					v-on:click="onModeToggleClicked"
					class="button searchbutton small"
					aria-label="Simple mode"
					type="button"
					title="Simple search mode"
				>
					<i class="fas fa-compress-alt"></i>
				</button>
				<button
					v-on:click="onClearClicked"
					class="button searchbutton small"
					aria-label="Clear search"
					type="button"
					title="Clear search"
				>
					<i class="fas fa-ban"></i>
				</button>
				<button
					class="button searchbutton small"
					aria-label="Share search"
					type="button"
					title="Share search"
					v-on:click="onShareClicked"
				>
					<i class="fas fa-share-alt"></i>
				</button>
				<button
					v-on:click="onSearchClicked"
					class="button searchbutton-wide small"
					aria-label="Submit search"
					type="button"
				>
					<span>Search</span>
				</button>
			</div>
			<div v-if="searchNotification !== ''" class="searchNotification">{{searchNotification}}</div>
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

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { foundation } from "../../../mixins/foundation";
import { SearchStorePaths } from "../../../store/modules/SearchStore";

@Component({
	mixins: [foundation],
})
export default class AdvancedSearchButtons extends Vue {
	searchNotification = "";
	cypherquery = "";
	shareUrl = "";

	onMinimizeClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
	}

	onModeToggleClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH_MODE);
	}

	onClearClicked(): void {
		if (confirm("Are you sure you want to clear the search?")) {
			this.$store.commit(SearchStorePaths.mutations.RESET);
		}
	}

	onSearchClicked(): void {
		const search = this.$store.state.visualizer.search.search;
		if (search.nodes.length > 0) {
			this.$store.dispatch(SearchStorePaths.actions.SEARCH);
		}
	}

	onShareClicked(): void {
		const search = this.$store.state.visualizer.search.search;
		if (search.nodes.length > 0) {
			this.$store.dispatch(SearchStorePaths.actions.UPDATE_SHARE);
		}
	}

	
}
</script>