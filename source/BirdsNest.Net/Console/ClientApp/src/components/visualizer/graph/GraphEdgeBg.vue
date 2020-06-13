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
	<path
		:id="'edgebg_' + edge.dbId"
		visibility="visible"
		:class="['edgebg', {'selected': edge.selected }, {'edgebg-loop': edge.isLoop }]"
		v-on:click.exact.prevent="onEdgeClicked"
		v-on:click.ctrl.exact.prevent="onEdgeCtrlClicked"
		v-on:contextmenu.prevent="onEdgeCtrlClicked"
	></path>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { d3 } from "@/assets/ts/visualizer/d3";
import { SimLink } from '@/assets/ts/visualizer/SimLink';
import { SimNode } from '@/assets/ts/visualizer/SimNode';
import { bus, events } from "@/bus";



@Component
export default class GraphEdgeBg extends Vue {
	@Prop({ type: Object, required: true })
	edge: SimLink<SimNode>;

	//assign the d3 datum to the element so simulation can use it
	mounted() {
		d3.select(this.$el).datum(this.edge);
	}

	onEdgeClicked() {
		bus.$emit(events.Visualizer.Edge.EdgeClicked, this.edge);
	}

	onEdgeCtrlClicked() {
		bus.$emit(events.Visualizer.Edge.EdgeCtrlClicked, this.edge);
	}
}
</script>