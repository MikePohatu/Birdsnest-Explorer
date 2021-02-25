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
	<g
		visibility="hidden"
		:id="'edge_' + edge.dbId"
		:class="['edges', edge.label, subTypes, { selected: edge.selected }, [edge.enabled ? 'enabled' : 'disabled']]"
		v-on:click.exact.prevent="onEdgeClicked"
		v-on:click.ctrl.exact.prevent="onEdgeCtrlClicked"
		v-on:contextmenu.prevent="onEdgeCtrlClicked"
	>
		<path :class="['arrows', edge.label, subTypes]"></path>
		<g class="edgelabel">
			<text class="noselect" dominant-baseline="text-bottom" text-anchor="middle" transform="translate(0,-5)">{{
				edge.label
			}}</text>
		</g>
	</g>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { d3 } from "@/assets/ts/visualizer/d3";
import { SimLink } from "@/assets/ts/visualizer/SimLink";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { bus, events } from "@/bus";
import webcrap from "@/assets/ts/webcrap/webcrap";

@Component
export default class GraphEdge extends Vue {
	@Prop({ type: Object, required: true })
	edge: SimLink<SimNode>;

	isSelected = false;

	get subTypes(): string {
		const subTypeProps = this.$store.state.pluginManager.subTypeProperties;

		if (Object.prototype.hasOwnProperty.call(subTypeProps, this.edge.label)) {
			const subProps: string[] = subTypeProps[this.edge.label];

			subProps.forEach((subType: string) => {
				const sub = webcrap.misc.cleanCssClassName(subType);
				if (webcrap.misc.isNullOrWhitespace(sub) === false) {
					return this.edge.label + "-" + sub;
				}
			});
		}

		return "";
	}

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