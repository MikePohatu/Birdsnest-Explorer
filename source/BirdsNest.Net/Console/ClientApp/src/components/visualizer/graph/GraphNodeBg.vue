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
	<g :id="'nodebg_' + node.dbId" :transform="translate">
		<circle
			:r="radius + 10"
			:cx="radius"
			:cy="radius"
			:class="['graphbg' ,'nodebg', {'selected': node.selected }]"
			visibility="visible"
		></circle>
		<g v-show="pinned">
			<circle
				:class="['graphbg' ,'nodebg', {'selected': node.selected }]"
				:r="radius/2"
				:cx="radius/4"
				:cy="radius/4"
			></circle>
		</g>
	</g>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { d3 } from "@/assets/ts/visualizer/d3";
import { SimNode } from "@/assets/ts/visualizer/SimNode";

@Component
export default class GraphNodeBg extends Vue {
	@Prop({ type: Object, required: true })
	node: SimNode;

	mounted() {
		d3.select(this.$el).datum(this.node);
	}

	get translate(): string {
		return "translate(" + this.node.currentX + "," + this.node.currentY + ")";
	}

	get pinned(): boolean {
		return this.node.pinned;
	}

	get size(): number {
		return this.node.currentSize;
	}

	get radius(): number {
		return this.node.currentSize / 2;
	}
}
</script>