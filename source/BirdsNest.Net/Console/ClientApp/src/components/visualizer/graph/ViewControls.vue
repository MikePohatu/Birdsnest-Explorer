<!-- Copyright (c) 2019-2023 "20Road"
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
	<div v-foundation>
		<div id="viewcontrols">
			<div id="viewcontrolbtns" class="grid-x">
				<ControlButton
					icon="fas fa-sync-alt"
					:title="$t('visualizer.menu.refresh')"
					v-on:click.native="bus.emit(controlEvents.RefreshLayout)"
					:class="[{ spinner: simRunning }, { 'button-active': simRunning }]"
				/>
				<ControlButton
					v-show="!playMode"
					icon="fas fa-play"
					:title="$t('visualizer.menu.play_pause')"
					v-on:click.native="playMode = true"
				/>
				<ControlButton
					v-show="playMode"
					icon="fas fa-pause"
					:title="$t('visualizer.menu.play_pause')"
					v-on:click.native="playMode = false"
				/>
				<ControlButton
					icon="fas fa-expand"
					:title="$t('visualizer.menu.select')"
					v-on:click.native="bus.emit(controlEvents.Select)"
					:class="{ 'button-active': selectActive }"
				/>
				<ControlButton
					icon="fas fa-random"
					:title="$t('visualizer.menu.invert')"
					v-on:click.native="bus.emit(controlEvents.Invert)"
				/>
				<ControlButton
					icon="fas fa-eye"
					:title="$t('visualizer.menu.hide_show_items')"
					data-toggle="eyeLabelListWrapper"
				/>
				<ControlButton
					icon="fas fa-trash-alt"
					:title="$t('visualizer.menu.remove_node')"
					v-on:click.native="bus.emit(controlEvents.RemoveNodes)"
				/>
				<ControlButton
					icon="fas fa-crop-alt"
					:title="$t('visualizer.menu.crop')"
					v-on:click.native="bus.emit(controlEvents.Crop)"
					:class="{ 'button-active': cropActive }"
				/>
				<ControlButton
					icon="fas fa-crosshairs"
					:title="$t('visualizer.menu.center')"
					v-on:click.native="bus.emit(controlEvents.CenterView)"
				/>
				<ControlButton
					icon="fas fa-file-export"
					:title="$t('visualizer.menu.export')"
					data-toggle="exportMenuWrapper"
				/>
				<ControlButton
					icon="fas fa-search"
					:title="$t('visualizer.menu.search')"
					data-toggle="searchGraphBoxWrapper"
				/>
				<ControlButton
					icon="fas fa-ban"
					:title="$t('visualizer.menu.clear')"
					:ishighlighted="true"
					v-on:click.native="bus.emit(controlEvents.ClearView)"
				/>
			</div>
			<div id="progress" :style="{ width: progressWidth }"></div>
		</div>

		<div
			id="exportMenuWrapper"
			class="dropdown-pane top"
			data-dropdown
			data-alignment="right"
			data-auto-focus="true"
			data-close-on-click="true"
			style="max-width: 200px;"
		>
			<ul class="vertical dropdown menu">
				<li>
					<a
						href="#"
						v-on:click="bus.emit(controlEvents.Export)"
					>{{ $t('visualizer.menu.export_report') }}</a>
				</li>
				<li v-if="!isIE">
					<a
						href="#"
						v-on:click="onSvgDownloadClicked()"
						:title="exportSvgTooltip"
					>{{ $t('visualizer.menu.export_svg') }}</a>
				</li>
			</ul>
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
					:placeholder="$t('word_Search')"
					v-model="searchVal"
					@keyup.enter="onSearchClicked"
					@keyup.delete.stop
				/>
				<div class="input-group-button">
					<button
						id="searchGraphBtn"
						class="button"
						:aria-label="$t('word_Search')"
						type="submit"
						:title="$t('word_Search')"
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
					:aria-label="$t('visualizer.menu.close_search')"
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

#exportMenuWrapper {
	padding-top: 0.3em;
	padding-right: 0;
	padding-bottom: 0.3em;
	font-size: 0.8em;
}

#exportMenuWrapper .dropdown.menu > li > a {
	padding: 0.7em;
}

.close-button {
	right: 0.3rem;
	top: 0rem;
}
</style>

<script setup lang="ts">
import webcrap from "@/assets/ts/webcrap/webcrap";
import { bus, events } from "@/bus";
import ControlButton from "./ControlButton.vue";
import EyeControls from "./EyeControls.vue";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { computed, ref } from "vue";
import { useStore } from "@/store";
import { vFoundation } from "@/mixins/foundation";

const store = useStore();
const controlEvents = events.Visualizer.Controls;
let searchVal = ref("");


const isIE = computed<boolean>(() => {
	return webcrap.misc.isIE();
});

const exportSvgTooltip = computed<string>(() => {
	const basemessage = "Export to Scalar Vector Graphics (SVG) file format. Files require FontAwesome Free fonts.";
	return webcrap.misc.isIE()
		? basemessage +
		"\n\nNote that exporting from Internet Explorer may result in some inconsistencies. Edge or Chrome is recommended"
		: basemessage;
});
const simRunning = computed<boolean>(() => {
	return store.state.visualizer.simRunning;
});

const progressWidth = computed<string>(() => {
	return store.state.visualizer.simProgress + "%";
});

const playMode = computed<boolean>({
	get() {
		return store.state.visualizer.playMode;
	},
	set(value) {
		store.commit(VisualizerStorePaths.mutations.Update.PLAY_MODE, value);
	}
});

const selectActive = computed<boolean>(() => {
	return store.state.visualizer.selectModeActive;
});

const cropActive = computed<boolean>(() => {
	return store.state.visualizer.cropModeActive;
});

function onSearchClicked(): void {
	bus.emit(events.Visualizer.Controls.Search, searchVal.value);
}

function applyComputedStyle(element, sourceElement) {
	const computedStyle = window.getComputedStyle(sourceElement);
	Array.from(computedStyle).forEach(key => {
		element.style.setProperty(key, computedStyle.getPropertyValue(key), computedStyle.getPropertyPriority(key));
	});
}

function onSvgDownloadClicked(): void {
	const root = document.getElementById("drawingSvg");
	const rootClone = root.cloneNode(true) as HTMLElement;

	const allRootElements = root.getElementsByTagName("*");
	const allCloneElements = rootClone.getElementsByTagName("*");

	applyComputedStyle(rootClone, root);

	//reset the translation values on top two root svg elements
	rootClone.setAttribute("transform", "translate(0,0)");
	rootClone.querySelector("#zoomlayer").setAttribute("transform", "translate(0,0)");

	for (let i = 0; i < allCloneElements.length; i++) {
		applyComputedStyle(allCloneElements.item(i), allRootElements.item(i));
	}

	const data = new XMLSerializer().serializeToString(rootClone);
	webcrap.misc.download(data, "graph.svg", "image/svg+xml;charset=utf-8");
}
</script>