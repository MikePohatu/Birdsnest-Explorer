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
	<g ref="root" :id="'nodebg_' + node.dbId" :transform="translate">
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

<script setup lang="ts">
import { d3 } from "@/assets/ts/visualizer/d3";
import { SimNode } from "@/assets/ts/visualizer/SimNode";
import { computed, onMounted, ref } from "vue";

	const props = defineProps({ node: { type: Object, required: true }});
	const root = ref(null);
	const node = props.node as SimNode;

	onMounted(() => {
		d3.select(root).datum(node);
	});

	const translate = computed((): string => {
		return "translate(" + node.currentX + "," + node.currentY + ")";
	});

	const pinned = computed((): boolean => {
		return node.pinned;
	});

	const size = computed(():  number => {
		return node.currentSize;
	});

	const radius = computed((): number => {
		return node.currentSize / 2;
	});
</script>