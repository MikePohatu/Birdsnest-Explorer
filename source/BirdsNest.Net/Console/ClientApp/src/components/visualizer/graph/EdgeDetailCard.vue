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
	<div v-foundation :id="'details-'+edge.DbId" class="detailcard pane">
		<div class="detaillist">
			<div class="grid-x align-middle">
				<div class="cell small-3">
					<u>
						<b>{{ $t('word_Details') }}</b>
					</u>
				</div>
				<div class="cell small-1" :title="$t('visualizer.details.show_hide_relationship')">
					<a v-on:click="onEyeClicked()">
						<span v-show="edge.enabled"><i class="cell fas fa-eye small-2"></i></span>
						<span v-show="!edge.enabled"><i class="cell fas fa-eye-slash small-2"></i></span>
					</a>
				</div>
			</div>
			<b>dbId:</b>
			{{edge.dbId}}
			<br />
			<b>{{ $t('word_Type') }}:</b>
			{{type}}
			<br />
		</div>

		<div>
			<ul class="accordion" data-accordion data-allow-all-closed="true" data-multi-expand="true">
				<!-- Properties -->
				<li class="accordion-item" data-accordion-item>
					<a href="#" class="accordion-title">{{ $t('word_Properties') }} ({{propertyNames.length}}):</a>
					<div class="accordion-content detaillist" data-tab-content>
						<div v-for="name in propertyNames" :key="name">
							<b>{{name}}:</b>
							{{edge.properties[name]}}
							<br />
						</div>
					</div>
				</li>
			</ul>
		</div>
	</div>
</template>


<style scoped>
.pane {
	min-width: 100px;
	background: white;
	border: 2px solid #d6d6d6;
	border-radius: 3px;
	padding: 5px;
	margin-left: 5px;
	margin-right: 5px;
	margin-top: 5px;
	margin-bottom: 5px;
}

.detailcard {
	border: 2px solid black;
	font-size: 12px;
	width: 270px;
	word-wrap: break-word;
	font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;
}

.detailcard .accordion-title {
	padding: 7px;
	/*border: 0px;*/
}

.detailcard .accordion-content {
	padding-left: 5px;
	padding-top: 0px;
	padding-bottom: 0px;
	padding-right: 0px;
	/*border: 0px;*/
}

.detailcard ul {
	margin: 0px;
}

.detaillist {
	padding-left: 7px;
	padding-top: 5px;
	padding-right: 5px;
	padding-bottom: 5px;
}

.currentActiveDetailCard {
	border-color: orange;
}
</style>


<script setup lang="ts">
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { SimLink } from '@/assets/ts/visualizer/SimLink';
import { computed } from "vue";
import { vFoundation } from "@/mixins/foundation";

const props = defineProps({ edge: { type: Object, required: true }});
const edge = props.edge as SimLink<SimNode>;

const propertyNames = computed((): string[] => {
	return Object.keys(props.edge.properties);
});

const type = computed((): string => {
	return props.edge.label;
});

function onEyeClicked() {
	edge.enabled = !edge.enabled;
}
</script>