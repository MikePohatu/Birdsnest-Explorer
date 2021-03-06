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
	<div>
		<div class="eyelist scrollable">
			<div>
				<b>Nodes</b>
			</div>
			<ol id="eyeNodeLabelList" class="no-bullet">
				<li>
					<div>
						<a v-on:click="onNodeShowAllClicked" class="pad-right">Show all</a>
						<span>|</span>
						<a v-on:click="onNodeHideAllClicked" class="pad-left-right">Hide all</a>
						<span>|</span>
						<a v-on:click="onNodeInvertClicked" class="pad-left">Invert</a>
					</div>
				</li>
				<li v-for="(value, name) in graphNodeLabelStates" :key="name" class="eyeListItem" :label="name">
					<div>
						<span class="eye-icon">
							<a v-on:click="onNodeLabelEyeClicked(name)">
								<span v-show="value">
									<i class="far fa-eye"></i>
								</span>
								<span v-show="!value">
									<i class="far fa-eye-slash"></i>
								</span>
							</a>
						</span>
						<span>{{ name }}</span>
					</div>
				</li>
			</ol>
			<div>
				<b>Relationships</b>
			</div>
			<ol id="eyeEdgeLabelList" class="no-bullet">
				<li>
					<div>
						<a v-on:click="onEdgeShowAllClicked" class="pad-right">Show all</a>
						<span>|</span>
						<a v-on:click="onEdgeHideAllClicked" class="pad-left-right">Hide all</a>
						<span>|</span>
						<a v-on:click="onEdgeInvertClicked" class="pad-left">Invert</a>
					</div>
				</li>
				<li v-for="(value, name) in graphEdgeLabelStates" :key="name" class="eyeListItem" :label="name">
					<div>
						<span class="eye-icon">
							<a v-on:click="onEdgeLabelEyeClicked(name)">
								<span v-show="value">
									<i class="far fa-eye"></i>
								</span>
								<span v-show="!value">
									<i class="far fa-eye-slash"></i>
								</span>
							</a>
						</span>
						<span>{{ name }}</span>
					</div>
				</li>
			</ol>
		</div>
	</div>
</template>

<style scoped>
.eyelist {
	font-size: 0.8em;
	max-height: 300px;
}

.eye-icon {
	padding-right: 0.5em;
}

.pad-left {
	padding-left: 0.5em;
}

.pad-right {
	padding-right: 0.5em;
}

.pad-left-right {
	padding-right: 0.5em;
	padding-left: 0.5em;

}
</style>

<script lang="ts">
import { bus, events } from "@/bus";
import { Component, Vue } from "vue-property-decorator";
import { graphData } from "@/assets/ts/visualizer/GraphData";
import { Dictionary } from "vue-router/types/router";

@Component
export default class EyeControls extends Vue {
	graphData = graphData; //create hook to make it reactive

	get graphNodeLabelStates(): Dictionary<boolean> {
		return this.graphData.graphNodeLabelStates;
	}

	get graphEdgeLabelStates(): Dictionary<boolean> {
		return this.graphData.graphEdgeLabelStates;
	}

	onNodeShowAllClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.ShowAllNodeLabel, true);
	}

	onNodeHideAllClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.ShowAllNodeLabel, false);
	}

	onNodeInvertClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.InvertNodeLabel);
	}

	onEdgeShowAllClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.ShowllEdgeLabel, true);
	}

	onEdgeHideAllClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.ShowllEdgeLabel, false);
	}

	onEdgeInvertClicked(): void {
		bus.$emit(events.Visualizer.EyeControls.InvertEdgeLabel);
	}

	onNodeLabelEyeClicked(label: string): void {
		bus.$emit(events.Visualizer.EyeControls.ToggleNodeLabel, label);
	}

	onEdgeLabelEyeClicked(label: string): void {
		bus.$emit(events.Visualizer.EyeControls.ToggleEdgeLabel, label);
	}
}
</script>