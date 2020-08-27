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
	<div v-on:click="onPaneClicked" id="advPane" class="searchpane absolute-top-left">
		<div>
			<fieldset class="fieldset">
				<legend>
					<span>Search</span>
					<div id="itemEditButtons">
						<span class="fieldset-spacer" />
						<!-- path buttons -->
						<button type="button" v-on:click.stop="onItemUpClicked" :disabled="editDisabled">
							<i class="itembutton clickable far fa-caret-square-up"></i>
						</button>
						<button type="button" v-on:click.stop="onItemDownClicked" :disabled="editDisabled">
							<i class="itembutton clickable far fa-caret-square-down"></i>
						</button>
						<button type="button" v-on:click.stop="onItemDeleteClicked" :disabled="isDeleteDisabled">
							<i class="itembutton clickable far fa-trash-alt"></i>
						</button>
						<button
							type="button"
							v-on:click.stop="onItemEditClicked"
							:disabled="editDisabled"
							:class="!editDisabled ? 'pulseenable' : ''"
						>
							<i class="itembutton clickable fas fa-edit"></i>
						</button>
						<button type="button" v-on:click.stop="onSearchNodeAddClicked">
							<i class="itembutton clickable fas fa-plus-circle"></i>
						</button>
					</div>
				</legend>

				<!-- path -->
				<div class="searchContainer">
					<div v-if="search.nodes.length > 0">
						<transition-group name="path" tag="div">
							<div v-for="item in fullPath" :key="item.id">
								<NodeIcon v-if="item.type === 'SearchNode'" :node="item" :key="item.id" />
								<EdgeIcon v-else :edge="item" :key="item.id" />
							</div>
						</transition-group>
					</div>
				</div>
			</fieldset>
		</div>

		<!-- Conditions -->
		<div>
			<fieldset class="fieldset">
				<legend>
					<span>Conditions</span>
					<div id="itemEditButtons">
						<span class="fieldset-spacer" />
						<!-- path buttons -->
						<button type="button" v-on:click.stop="onCondUpClicked" :disabled="condEditDisabled">
							<i class="itembutton clickable far fa-caret-square-up"></i>
						</button>
						<button type="button" v-on:click.stop="onCondDownClicked" :disabled="condEditDisabled">
							<i class="itembutton clickable far fa-caret-square-down"></i>
						</button>
						<button type="button" v-on:click.stop="onCondDeleteClicked" :disabled="condEditDisabled">
							<i class="itembutton clickable far fa-trash-alt"></i>
						</button>
						<button
							type="button"
							v-on:click.stop="onCondEditClicked"
							:disabled="condEditDisabled"
							:class="!condEditDisabled ? 'pulseenable' : ''"
						>
							<i class="itembutton clickable fas fa-edit"></i>
						</button>
						<button type="button" v-on:click.stop="onCondAddClicked" :disabled="!searchHasNodes">
							<i class="itembutton clickable fas fa-plus-circle"></i>
						</button>
					</div>
				</legend>
				<AndOrConditionIcon v-if="search.condition !== null" :condition="search.condition" />
			</fieldset>
		</div>

		<!-- Buttons start here -->
		<AdvancedSearchButtons />
		<SearchResults :id="'advresults'" />
		<div id="searchAlert" class="reveal">
			<div class="align-middle">
				<div>
					<span id="alertMessage"></span>
				</div>
				<div id="alertMessageLink"></div>
				<div>
					<button class="button" data-close aria-label="Close alert dialog" type="button">
						<span aria-hidden="true">OK</span>
					</button>
				</div>
			</div>
		</div>

		<ShareDialog />
	</div>
</template>


<style scoped>
#advPane {
	min-width: 310px;
}

#itemEditButtons {
	display: inline;
}

.fieldset {
	display: block;
	width: 100%;
	margin-top: 0;
	margin-bottom: 5px;
	margin-left: 0;
	margin-right: 0;
	padding: 2px 4px;
	min-height: 3rem;
}

.fieldset-spacer {
	margin-left: 5px;
	margin-right: 5px;
}

.advsearchlabel {
	color: gray;
	background-color: rgb(241, 241, 241);
	margin-top: 7px;
	margin-bottom: 2px;
	margin-left: 0;
	margin-right: 0;
	width: 25%;
}
.itembutton {
	margin-top: 0;
	margin-bottom: 0;
	margin-left: 5px;
	margin-right: 3px;
}

.path-enter-active,
.path-leave-active {
	transition: all 0.15s ease-in-out;
}

.path-enter,
.path-leave-to {
	opacity: 0;
	transform: translateY(-200px);
}

.path-move {
	transition: transform 0.3s;
}

.pulseenable {
	box-shadow: none;
	animation: pulse 2s 1;
}

@-webkit-keyframes pulse {
	0% {
		-webkit-box-shadow: 0 0 0 0 rgba(44, 140, 204, 0.4);
	}
	70% {
		-webkit-box-shadow: 0 0 0 10px rgba(44, 140, 204, 0);
	}
	100% {
		-webkit-box-shadow: 0 0 0 0 rgba(44, 140, 204, 0);
	}
}
@keyframes pulse {
	0% {
		-moz-box-shadow: 0 0 0 0 rgba(44, 140, 204, 0.4);
		box-shadow: 0 0 0 0 rgba(44, 140, 204, 0.4);
	}
	70% {
		-moz-box-shadow: 0 0 0 10px rgba(44, 140, 204, 0);
		box-shadow: 0 0 0 10px rgba(44, 140, 204, 0);
	}
	100% {
		-moz-box-shadow: 0 0 0 0 rgba(44, 140, 204, 0);
		box-shadow: 0 0 0 0 rgba(44, 140, 204, 0);
	}
}

#advPane >>> .advsearchbutton {
	position: relative;
	border-width: 2px;
	border-style: solid;
	padding: 4px 2px;
	text-align: center;
	text-decoration: none;
	font-size: 0.8em;
	margin-top: 0;
	margin-bottom: 0;
}

#advPane >>> .advsearchbutton.selected {
	border-width: 4px;
	padding: 2px 0;
}

#advPane >>> .nodebutton {
	border-radius: 999px;
	margin: 5px 0;
}

#advPane >>> .edgebutton {
	border-radius: 2px;
}
</style>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import NodeIcon from "./NodeIcon.vue";
import EdgeIcon from "./EdgeIcon.vue";
import SearchResults from "./SearchResults.vue";
import AndOrConditionIcon from "./AndOrConditionIcon.vue";
import ShareDialog from "./ShareDialog.vue";
import { Search, SearchItem, ConditionType } from "@/assets/ts/visualizer/Search";
import AdvancedSearchButtons from "./AdvancedSearchButtons.vue";
import { ResultSet } from "@/assets/ts/dataMap/ResultSet";
import { SearchStorePaths } from "../../../store/modules/SearchStore";

@Component({
	components: {
		NodeIcon,
		EdgeIcon,
		AndOrConditionIcon,
		AdvancedSearchButtons,
		SearchResults,
		ShareDialog,
	},
})
export default class AdvancedSearch extends Vue {
	get editDisabled(): boolean {
		return this.$store.state.visualizer.search.selectedItem === null;
	}

	get condEditDisabled(): boolean {
		return this.$store.state.visualizer.search.selectedCondition === null;
	}

	get search(): Search {
		return this.$store.state.visualizer.search.search;
	}

	get isDeleteDisabled(): boolean {
		if (this.$store.state.visualizer.search.selectedItem === null) {
			return true;
		}
		if (this.$store.state.visualizer.search.selectedItem.type !== "SearchNode") {
			return true;
		}
		return false;
	}

	get fullPath(): SearchItem[] {
		const search: Search = this.$store.state.visualizer.search.search;
		const path: SearchItem[] = [];
		for (let i = 0; i < search.edges.length; i++) {
			path.push(search.nodes[i]);
			path.push(search.edges[i]);
		}
		path.push(search.nodes[search.nodes.length - 1]);
		return path;
	}

	get results(): ResultSet {
		return this.$store.state.visualizer.search.results;
	}

	get searchHasNodes(): boolean {
		return this.$store.state.visualizer.search.search.nodes.length > 0;
	}

	onPaneClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, null);
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, null);
	}

	onSearchNodeAddClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Add.NEW_NODE);
	}
	onItemUpClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM_UP);
	}
	onItemDownClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM_DOWN);
	}
	onItemDeleteClicked(): void {
		const confirmed = confirm(
			"Are you sure you want to delete " +
				this.$store.state.visualizer.search.selectedItem.name +
				" and any associated conditions?"
		);

		if (confirmed) {
			this.$store.commit(SearchStorePaths.mutations.Delete.SELECTED_ITEM);
		}
	}
	onItemEditClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
	}

	onCondAddClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Add.NEW_CONDITION_PARENT, null);
	}

	onCondEditClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_CONDITION);
	}
	onCondUpClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION_UP);
	}
	onCondDownClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION_DOWN);
	}
	onCondDeleteClicked(): void {
		const cond = this.$store.state.visualizer.search.selectedCondition;
		let message: string; 
		if (cond.type === ConditionType.And || cond.type === ConditionType.Or) {
			message = "Are you sure you want to delete this condtion and any associated sub conditions?"
		} else {
			message = "Are you sure you want to delete this condtions?"
		}
		if (confirm(message)) {
			this.$store.commit(SearchStorePaths.mutations.Delete.SELECTED_CONDITION);
		}
	}
}
</script>