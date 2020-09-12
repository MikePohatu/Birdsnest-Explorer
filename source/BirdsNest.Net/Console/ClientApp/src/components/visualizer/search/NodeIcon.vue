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
	<div
		:class="[label, ' advsearchbutton nodebutton clickable noselect',[isSelectedItem ? 'selected' : '']]"
		v-on:dblclick.stop="onNodeDblClicked"
		v-on:click.stop="onNodeClicked"
	>{{text}}</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import { SearchNode } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "@/store/modules/SearchStore";
import webcrap from '../../../assets/ts/webcrap/webcrap';

@Component
export default class NodeIcon extends Vue {
	@Prop({ type: Object as () => SearchNode, required: true })
	node: SearchNode;

	get text(): string {
		if (webcrap.misc.isNullOrWhitespace(this.node.label)) {
			return this.node.name;
		} else {
			return this.node.name + " :" + this.node.label;
		}
	}

	get label(): string {
		return this.node.label;
	}

	get isSelectedItem(): boolean {
		return this.$store.state.visualizer.search.selectedItem === this.node;
	}

	onNodeClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, this.node);
	}

	onNodeDblClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, this.node);
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
	}
}
</script>