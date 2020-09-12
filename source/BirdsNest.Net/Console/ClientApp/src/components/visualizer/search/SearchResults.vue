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
	<div id="results" v-bind:class="{hidden: !showResults}" >
		<div v-bind:class="{hidden: zeroResults}">
			<div>
				{{ $t('word_Found') }}
				<a :data-toggle="dropdownId">{{nodes.length}}</a> {{ $t('word_results') }}.
				<a v-on:click="onAddToViewClicked">{{ $t('phrase_Add_to_view') }}</a>
			</div>

			<div
				:id="dropdownId"
				class="scrollable dropdown-pane listings"
				data-dropdown
				data-auto-focus="true"
				data-close-on-click="true"
			>
				<div v-for="node in nodes" :key="node.dbId" class="listing" :title="node.labels.join(', ')">
					<a v-on:click="onAddClicked(node)">(+)</a>
					<span>{{node.name}}</span>
				</div>
			</div>
		</div>
		<div v-if="(searchNotification !== null)" v-bind:class="{loading: isSearching}">{{searchNotification}}</div>
	</div>
</template>


<style scoped>
#results {
	height: auto;
	min-width: 200px;
	margin: 5px;
}

.listings {
	font-size: 0.7em;
	padding: 5px 10px;
	min-width: 200px;
	max-height: 300px;
	max-width: 300px;
}

.listing {
	padding: 0;
	white-space: nowrap;
}

.listing span {
	vertical-align: middle;
	padding-left: 0.5em;
	padding-right: 0.5em;
}
</style>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { foundation } from "../../../mixins/foundation";
import { SearchNode } from "../../../assets/ts/visualizer/Search";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { SearchStorePaths } from "../../../store/modules/SearchStore";

import { bus, events } from "@/bus";

@Component({
	mixins: [foundation],
})
export default class SearchResults extends Vue {
	//The id be unique, or the foundation dropdown won't work.
	//It will be trying to work on same id
	@Prop({ type: String, required: true })
	id: string;

	get dropdownId(): string {
		return "dropdown-" + this.id;
	}
	get nodes(): SearchNode[] {
		const results = this.$store.state.visualizer.search.results;
		return results === null ? [] : results.nodes;
	}

	get showResults(): boolean {
		return (this.$store.state.visualizer.search.results !== null) || (this.searchNotification !== null);
	}

	get zeroResults(): boolean {
		const results = this.$store.state.visualizer.search.results;
		if (results === null) {
			return true;
		}
		return results.nodes.length === 0;
	}

	get searchNotification(): string {
		return this.$store.state.visualizer.search.statusMessage;
	}

	get isSearching(): boolean {
		return this.$store.state.visualizer.search.isSearching;
	}

	onAddClicked(node: SearchNode): void {
		this.$store.commit(VisualizerStorePaths.mutations.Add.PENDING_NODE, node);
	}

	onAddToViewClicked(): void {
		let proceed = true;

		if (this.$store.state.visualizer.search.results.nodes.length > 300) {
			proceed = confirm(this.$t('visualizer.search.count_warning').toString());
		}
					
		if (proceed && this.$store.state.visualizer.search.results.nodes.length > 0) {
			bus.$emit(events.Notifications.Processing, this.$t('visualizer.search.adding_results').toString());
			this.$store.commit(VisualizerStorePaths.mutations.Add.PENDING_RESULTS, this.$store.state.visualizer.search.results);
			this.$store.commit(SearchStorePaths.mutations.Delete.RESULTS);
		}
	}
}
</script>