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
	<div class="grid-x align-center">
		<!-- Up arrow or arrow shaft -->
		<div v-show="!isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-arrow-up arrowpart" />
		</div>
		<div v-show="isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-minus shaft arrowpart" />
		</div>

		<div
			:class="['cell advsearchbutton edgebutton clickable noselect', edge.label, [isSelectedItem ? 'selected' : '']]"
			v-on:click.stop="onEdgeClicked"
			v-on:dblclick.stop="onEdgeDblClicked"
		>{{text}}</div>

		<!-- Down arrow or arrow shaft -->
		<div v-show="isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-arrow-down arrowpart" />
		</div>
		<div v-show="!isDirRight" :class="['arrow', edge.label]">
			<i class="cell fas fa-minus shaft arrowpart" />
		</div>
	</div>
</template>

<style scoped>
.arrow .shaft {
	transform: rotate(90deg);
}

.arrowpart {
	background: none;
	width: fit-content;
}

.arrow {
	background: none;
	margin-top: 0;
	margin-bottom: 0;
	display: contents;
	padding: 0;
}

.edgebutton {
	margin-top: 0;
	margin-bottom: 0;
}
</style>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { SearchEdge } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from '../../../store/modules/SearchStore';
import webcrap from '@/assets/ts/webcrap/webcrap';

@Component
export default class EdgeIcon extends Vue {
	@Prop({ type: Object as () => SearchEdge, required: true })
	edge: SearchEdge;

	get isDirRight(): boolean {
		return this.edge.direction === ">";
	}

	get text(): string {
		let outtext = this.edge.name;
		if (webcrap.misc.isNullOrWhitespace(this.edge.label) === false) {
			outtext += " :" + this.edge.label;
		} 

		if (this.edge.min === -1) {
			outtext += " *";
		} else {
			outtext += " " + this.edge.min + ".." + this.edge.max;
		}

		return outtext;
	}

	onEdgeClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, this.edge);
	}

	onEdgeDblClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_ITEM, this.edge);
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_ITEM);
	}

	get isSelectedItem(): boolean {
		return this.$store.state.visualizer.search.selectedItem === this.edge;
	}
}
</script>