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
	<div>
		<div id="viewcontrols">
			<div id="viewcontrolbtns" class="grid-x">
				<ControlButton
					icon="fas fa-sync-alt"
					title="Refresh layout"
					v-on:click.native="bus.$emit(controlEvents.RefreshLayout)"
					:class="[{ spinner: simRunning}, { 'button-active': simRunning}]"
				/>
				<ControlButton
					v-show="!playMode"
					icon="fas fa-play"
					title="Play/pause layout"
					v-on:click.native="playMode = true"
				/>
				<ControlButton
					v-show="playMode"
					icon="fas fa-pause"
					title="Play/pause layout"
					v-on:click.native="playMode = false"
				/>
				<ControlButton
					icon="fas fa-expand"
					title="Select tool"
					v-on:click.native="bus.$emit(controlEvents.Select)"
					:class="{ 'button-active': selectActive}"
				/>
				<ControlButton
					icon="fas fa-random"
					title="Invert selection"
					v-on:click.native="bus.$emit(controlEvents.Invert)"
				/>
				<ControlButton icon="far fa-eye" title="Hide/show items" data-toggle="eyeLabelListWrapper" />
				<ControlButton
					icon="fas fa-trash-alt"
					title="Remove node from layout"
					v-on:click.native="bus.$emit(controlEvents.RemoveNodes)"
				/>
				<ControlButton
					icon="fas fa-crop-alt"
					title="Crop zoom tool"
					v-on:click.native="bus.$emit(controlEvents.Crop)"
					:class="{ 'button-active': cropActive}"
				/>
				<ControlButton
					icon="fas fa-crosshairs"
					title="Center layout"
					v-on:click.native="bus.$emit(controlEvents.CenterView)"
				/>
				<ControlButton
					icon="fas fa-file-export"
					title="Export to report view"
					v-on:click.native="bus.$emit(controlEvents.Export)"
				/>
				<ControlButton
					icon="fas fa-search"
					title="Search in current view"
					data-toggle="searchGraphBoxWrapper"
				/>
				<ControlButton
					icon="fas fa-ban"
					title="Clear current view"
					:ishighlighted="true"
					v-on:click.native="bus.$emit(controlEvents.ClearView)"
				/>
			</div>
			<div id="progress" :style="{ width: progressWidth }"></div>
		</div>

		<div
			id="searchGraphBoxWrapper"
			class="dropdown-pane top"
			data-dropdown
			data-auto-focus="true"
			data-close-on-click="true"
		>
			
			<div class="input-group shrink margin-zero">
				<input
					id="searchGraphTerm"
					type="search"
					class="input-group-field"
					placeholder="Search"
					v-model="searchVal"
					@keyup.enter="onSearchClicked"
					@keyup.delete.stop
				/>
				<div class="input-group-button">
					<button
						id="searchGraphBtn"
						class="button"
						aria-label="Search"
						type="submit"
						title="Search"
						v-on:click="onSearchClicked"
					>
						<i class="fas fa-search"></i>
					</button>
				</div>
			</div>
			<div>
				<!-- close  -->
				<button
					class="close-button"
					aria-label="Close graph search"
					type="button"
					data-toggle="searchGraphBoxWrapper"
				>
					<span>&times;</span>
				</button>
			</div>
		</div>

		<div
			id="eyeLabelListWrapper"
			class="dropdown-pane top"
			data-dropdown
			data-auto-focus="true"
			data-close-on-click="true"
			
		>
			<EyeControls />
		</div>
	</div>
</template>

<script lang="ts">
import { bus, events } from "@/bus";
import { Component, Vue } from "vue-property-decorator";
import ControlButton from "./ControlButton.vue";
import EyeControls from "./EyeControls.vue";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { foundation } from "@/mixins/foundation";

@Component({
	components: { ControlButton, EyeControls },
	mixins: [foundation],
})
export default class ViewControls extends Vue {
	bus = bus;
	controlEvents = events.Visualizer.Controls;
	searchVal = "";

	get simRunning(): boolean {
		return this.$store.state.visualizer.simRunning;
	}

	get progressWidth(): string {
		return this.$store.state.visualizer.simProgress + "%";
	}

	get playMode(): boolean {
		return this.$store.state.visualizer.playMode;
	}
	set playMode(value) {
		this.$store.commit(VisualizerStorePaths.mutations.Update.PLAY_MODE, value);
	}

	get selectActive(): boolean {
		return this.$store.state.visualizer.selectModeActive;
	}

	get cropActive(): boolean {
		return this.$store.state.visualizer.cropModeActive;
	}

	onSearchClicked(): void {
		bus.$emit(events.Visualizer.Controls.Search, this.searchVal);
	}
}
</script>

<style scoped>
#viewcontrols {
	position: fixed;
	bottom: 3px;
	left: 0;
	height: 35px;
	min-width: 275px;
	padding: 0;
	background: white;
	border: 2px solid #d6d6d6;
	border-top-style: none;
	border-left-style: none;
	margin: 0;
}

#viewcontrolbtns {
	padding-left: 3px;
	padding-right: 3px;
}

#progress {
	display: block;
	height: 5px;
	padding: 0;
	margin: 0;
	background-color: cornflowerblue;
	z-index: 5;
}

#searchGraphBoxWrapper {
	padding-top: 1em;
	padding-right: 2rem;
	padding-bottom: 0;
}

.close-button {
	right: 0.3rem;
	top: 0rem;
}
</style>