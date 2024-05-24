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
	<ul ref="templateRoot" id="listWrapper" class="vertical menu accordion-menu scrollable" data-accordion-menu>
        <li v-for="(nodeList, property) in labelledNodes">
            <a href="#">{{ property }} ({{ nodeList.length }})</a>
            <ul class="vertical menu nested">
                <li class="menu nested add-all">
                    <a v-on:click="onAddLabelClicked(nodeList)" class="">{{ $t('visualizer.search.Add_all') }}</a>
                </li>
                <li v-for="node in nodeList" :key="node.dbId" :title="node.labels.join(', ')" class="menu nested">
                    <a v-on:click="onAddClicked(node)">(+)</a><div>{{ node.name }}</div>
                </li>
            </ul>
        </li>
    </ul>
</template>


<style scoped>
#listWrapper {
    max-height: 450px;
    max-width: 600px;
}

.accordion-menu .is-accordion-submenu {
	margin-left: 0;
    padding-bottom: 0;
    padding-top: 0.2em;
}

.accordion-menu .is-accordion-submenu a {
	padding-right: 0.5em;
    padding-bottom: 0;
    padding-top: 0.2em;
    padding-left: 0;
}

.vertical.menu.nested .add-all a {
    position: relative;
    left: -0.4rem;
    padding-left: 0;
    padding-right: 0;
}

.accordion-menu a {
	padding: 0.3rem;
}

.menu.nested {
    font-size: small;
    width: max-content;
}
</style>

<script setup lang="ts">
import { SearchNode } from "@/assets/ts/visualizer/Search";
import { VisualizerStorePaths } from "@/store/modules/VisualizerStore";
import { computed, ref } from "vue";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import { initFoundationMounted } from "@/mixins/foundation";
import { Dictionary } from "@/assets/ts/webcrap/misccrap";

const templateRoot = ref(null);
initFoundationMounted(templateRoot);

const store = useStore();
const { t } = useI18n();


const labelledNodes = computed<Dictionary<SearchNode[]>>(() => {
	const results = store.state.visualizer.search.results;
	const labelledResults: Dictionary<SearchNode[]> = {};

	if (results !== null) {
		labelledResults[t('visualizer.search.ALL')] = [];

		if (results.nodes) {
			results.nodes.forEach(node => {
                binaryInsertion(labelledResults[t('visualizer.search.ALL')],node);
                
				node.labels.forEach(label => {
					if (labelledResults[label] === undefined) {
						labelledResults[label] = [];
					}
                    binaryInsertion(labelledResults[label],node);

					//labelledResults[label].push(node);
				});
			});
		}
	}
	else {
		labelledResults[t('visualizer.search.ALL')] = [];
	}
	return labelledResults;
});

function onAddClicked(node: SearchNode): void {
	store.commit(VisualizerStorePaths.mutations.Add.PENDING_NODE, node);
};


function onAddLabelClicked(nodelist: SearchNode[]): void {
	store.commit(VisualizerStorePaths.mutations.Add.PENDING_NODES, nodelist);
};

//https://stackoverflow.com/a/60702475
function binaryInsertion(arr: SearchNode[], element: SearchNode) {
    if (arr.length === 0) { 
        arr.push(element); 
    }
    else {
        binaryHelper(arr, element, 0, arr.length - 1);
    }
}
    
function binaryHelper(arr: SearchNode[], element: SearchNode, lBound, uBound) {
    if (uBound - lBound <= 1) {
        // binary search ends, we need to insert the element around here       
        if (element.name.localeCompare(arr[lBound].name) === -1) {
            arr.splice(lBound, 0, element);
        }
        else if (element.name.localeCompare(arr[uBound].name) === 1) {
            arr.splice(uBound+1, 0, element);
        }
        else {
            arr.splice(uBound, 0, element);
        }
    } 
    else {       
        // we look for the middle point
        const midPoint = Math.floor((uBound - lBound) / 2) + lBound;
        // depending on the value in the middle, we repeat the operation only on one slice of the array, halving it each time
        element.name.localeCompare(arr[midPoint].name) === -1
            ? binaryHelper(arr, element, lBound, midPoint)
            : binaryHelper(arr, element, midPoint, uBound);
    }
}
</script>