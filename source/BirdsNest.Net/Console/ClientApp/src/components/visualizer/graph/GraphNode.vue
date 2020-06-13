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
	<g
		:id="'node_'+node.dbId"
		:class="['nodes', node.labels, subTypes, node.enabled ? 'enabled' : 'disabled', { 'selected' : node.selected }]"
		draggable="true"
		cursor="pointer"
		:transform="translate"
		visibility="visible"
		v-on:click.exact.prevent="onNodeClicked"
		v-on:click.ctrl.exact.prevent="onNodeCtrlClicked"
		v-on:contextmenu.prevent="onNodeCtrlClicked"
	>
		<title>Types: {{node.labels.join(', ')}}</title>
		<circle
			:id="'node_'+node.dbId + '_icon'"
			class="nodecircle"
			:r="radius"
			:cx="radius"
			:cy="radius"
		></circle>
		
		<text
			:font-size="radius"
			:x="radius"
			:y="radius*1.05"
			dy="0.3em"
			:class="[node.labels, subTypes, 'icon', 'noselect']"
			v-html="icon"
		></text>
		<text
			class="nodetext noselect"
			text-anchor="middle"
			dominant-baseline="central"
			:transform="textTranslate"
		>{{node.name}}</text>

		<g v-show="pinned" class="pin" v-on:click.stop="onPinClicked">
			<circle :r="radius/2.5" :cx="radius/4" :cy="radius/4"></circle>
			<text :x="radius/4" :y="radius/2.1" :font-size="radius/2" class="icon">&#xf08d;</text>
		</g>
	</g>
</template>


<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { d3 } from "@/assets/ts/visualizer/d3";
import { bus, events } from "@/bus";
import webcrap from "@/assets/ts/webcrap/webcrap";

@Component
export default class GraphNode extends Vue {
	@Prop({ type: Object, required: true })
	node: SimNode;
	isSelected = false;

	//assign the d3 datum to the element so simulation can use it
	mounted() {
		//console.log(this.node);
		d3.select(this.$el).datum(this.node);
	}

	get subTypes(): string[] {
		const subTypes: string[] = [];
		const subTypeProps = this.$store.state.pluginManager.subTypeProperties;

		this.node.labels.forEach(label => {
			if (Object.prototype.hasOwnProperty.call(subTypeProps,label)) {
				const subProp = subTypeProps[label];
				const subType = String(this.node.properties[subProp]);

				if (webcrap.misc.isNullOrWhitespace(subType) === false) {
					subTypes.push(label + '-'+ subType);
				}
			} 
		});
		
		return subTypes;
	}

	get icon(): string {
		const mappings = this.$store.state.visualizer.iconMappings;
		return "&#x"+ mappings.getMappingFromArray(this.node.labels) + ";";
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

	get translate(): string {
		//console.log("translate(" + this.node.x + "," + this.node.y + ")");
		return "translate(" + this.node.currentX + "," + this.node.currentY + ")";
	}

	get textTranslate(): string {
		return "translate(" + this.radius + "," + (this.size + 10) + ")";
	}

	onPinClicked(): void {
		bus.$emit(events.Visualizer.Node.NodePinClicked, this.node.dbId);
	}

	onNodeClicked(): void {
		bus.$emit(events.Visualizer.Node.NodeClicked, this.node);
	}

	onNodeCtrlClicked(): void {
		bus.$emit(events.Visualizer.Node.NodeCtrlClicked, this.node);
	}
}
</script>