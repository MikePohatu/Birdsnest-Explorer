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
	<div id="pagewrapper" class="fillAvailable">
		<div id="graphwrapper" class="fillAvailable">
			<Graph />
			<Searches />
			<div id="graphstatus" class="grid-x">
				<span id="statusIcon" class="cell small-2"></span>
				<span id="statusMessage" class="cell small-10"></span>
			</div>
		</div>

		<NodeEdit v-if="editNode !== null" :source="editNode" />
		<EdgeEdit v-if="editEdge !== null" :source="editEdge" />
		<ValueConditionEdit v-if="editValCond !== null" :source="editValCond" />
		<AndOrConditionEdit v-if="editAndOrCondition !== null" :source="editAndOrCondition" />
		<NewConditionSelect v-if="newConditionParent !== null" />
	</div>
</template>


<style scoped>
#pagewrapper,
#graphwrapper {
	top: 0;
	bottom: 0;
	right: 0;
	left: 0;
}

#pagewrapper {
	position: relative;
}
</style>

<script lang="ts">
import { useStore } from "@/store";
import Searches from "@/components/visualizer/search/Searches.vue";
import NodeEdit from "@/components/visualizer/search/NodeEdit.vue";
import EdgeEdit from "@/components/visualizer/search/EdgeEdit.vue";
import ValueConditionEdit from "@/components/visualizer/search/ValueConditionEdit.vue";
import AndOrConditionEdit from "@/components/visualizer/search/AndOrConditionEdit.vue";
import NewConditionSelect from "@/components/visualizer/search/NewConditionSelect.vue";
import Graph from "@/components/visualizer/graph/Graph.vue";

import { auth } from "@/assets/ts/webcrap/authcrap";
import { SearchNode, SearchEdge, ValueCondition, AndOrCondition, Search } from "../assets/ts/visualizer/Search";
import { RouteLocation, RouteLocationRaw, RouteRecordRaw, useRoute } from "vue-router";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import { ApiNode } from "@/assets/ts/dataMap/ApiNode";
import { VisualizerStorePaths } from "../store/modules/VisualizerStore";
import LStore from "@/assets/ts/LocalStorageManager";
import { computed, defineComponent, watch } from "vue";


export default defineComponent({
	components: {
		Searches,
		NodeEdit,
		EdgeEdit,
		ValueConditionEdit,
		AndOrConditionEdit,
		NewConditionSelect,
		Graph,
	},
	setup() {
		const route = useRoute();
		const store = useStore();
		const selectedNodesList: number[] = []; // = [128, 118];

		const loadSharedSearch = function (route: RouteLocation): void {
			const encodedData = route.query.sharedsearch as string;
			if (encodedData !== undefined) {
				const decoded = JSON.parse(webcrap.misc.decodeUrlB64(encodedData)) as Search;
				store.commit(SearchStorePaths.mutations.Update.SEARCH, decoded);
			}
		}

		
		const selectedNodes = computed(():  ApiNode[] =>{
			return store.state.visualizer.selectedNodes;
		})

		const editNode = computed((): SearchNode => {
			return store.state.visualizer.search.editNode;
		})

		const editEdge = computed(():  SearchEdge => {
			return store.state.visualizer.search.editEdge;
		})

		const editValCond = computed(():  ValueCondition => {
			return store.state.visualizer.search.editValCondition;
		})

		const editAndOrCondition = computed(():  AndOrCondition => {
			return store.state.visualizer.search.editAndOrCondition;
		})

		const newCondition = computed((): AndOrCondition => {
			return store.state.visualizer.search.newCondition;
		});

		const newConditionParent = computed((): AndOrCondition => {
			return store.state.visualizer.search.newConditionParent;
		});

		watch(() => route, (to) => {
				loadSharedSearch(to);
		});

		return {
			selectedNodesList, selectedNodes, editNode, editEdge, editValCond, editAndOrCondition, newCondition, newConditionParent
		}
	},

	mounted(): void {
		auth.getValidationToken();
		this.loadSharedSearch(this.$route);
		const pending = LStore.popPendingResultSet();
		if (pending !== null) {
			this.$store.commit(VisualizerStorePaths.mutations.Add.PENDING_RESULTS, pending);
		}

		if (this.$store.state.visualizer.search.search.nodes.length > 0) {
			this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH);
			this.$store.commit(SearchStorePaths.mutations.TOGGLE_SEARCH_MODE);
		}
	},
	
	$route($route: any) {
		throw new Error("Method not implemented.");
	}
})
</script>