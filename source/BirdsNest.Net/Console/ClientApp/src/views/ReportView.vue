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
	<div id="reportView">
		<div id="reportHeader" class="grid-x">
			<div class="cell shrink headerItem" id="reportmenu">
				<ul
					class="dropdown menu"
					data-dropdown-menu
					data-close-on-click="false"
					data-close-on-click-inside="false"
				>
					<li>
						<a
							id="reportmenutext"
						>{{ $t('phrase_Query_returned') }} {{ resultCount }} {{ $t('word_records') }}</a>
						<ul class="menu">
							<li id="columnsli">
								<a href="#">{{ $t('word_Columns') }}</a>
								<ul class="vertical menu nested" id="columnstoggles">
									<li v-for="name in propertyNames" :key="name">
										<label class="toggleitem">
											<input
												class="toggleitem"
												type="checkbox"
												v-model="columnStates[name]"
												v-on:click="updateActiveProperties"
											/>
											{{ name }}
										</label>
									</li>
								</ul>
							</li>
							<li id="downloadli">
								<a href="#" v-on:click="onDownloadClicked">{{ $t('word_Download') }}</a>
							</li>
							<li id="visualizerli">
								<a href="#" v-on:click="onVisualizerClicked">{{ $t('phrase_Open_in_Visualizer') }}</a>
							</li>
							<li v-if="query !== ''">
								<a href="#" v-on:click="onShowQueryClicked">{{ $t('phrase_Show_query') }}</a>
							</li>
						</ul>
					</li>
				</ul>
			</div>

			<div class="cell auto" />
			<div class="x-center show-for-medium headerItem" id="reportName">
				<h5>{{ reportName }}</h5>
			</div>

			<div id="pages" class="cell shrink headerItem">
				<span v-if="hasPrevPage" id="prevPageBtn" style="margin:5px;">
					<a href="#" v-on:click="onPageDownClicked">
						<i class="fas fa-caret-left"></i>
					</a>
				</span>
				<span style="margin:5px;" class="text-center">{{ pageNum }} {{ $t('word_of') }} {{ pageCount }}</span>
				<span v-if="hasNextPage" id="nextPageBtn" style="margin:5px;">
					<a href="#" v-on:click="onPageUpClicked">
						<i class="fas fa-caret-right"></i>
					</a>
				</span>
			</div>
		</div>

		<!-- Main output area -->
		<LoadingLogo v-if="!resultsLoaded">Loading</LoadingLogo>
		<div v-else id="output">
			<table id="outputtable" class="hover">
				<thead>
					<tr id="outputHeader">
						<th v-for="name in activePropertyNames" :key="name">{{ name }}</th>
					</tr>
				</thead>
				<tbody id="outputBody">
					<tr v-for="node in nodesPage" :key="node.dbId">
						<td v-for="name in activePropertyNames" :key="name">{{ node.properties[name] }}</td>
					</tr>
				</tbody>
			</table>
		</div>

		<div v-show="showQuery" id="querydialog" class="dialogWrapper">
			<div class="dialog">
				<div>
					<h5>
						<u>Neo4j Cypher Query</u>
					</h5>
					<button
						class="close-button"
						v-on:click="onQueryCloseClicked"
						aria-label="Close query dialog"
						type="button"
					>
						<span aria-hidden="true">&times;</span>
					</button>
				</div>
				<div>
					<p>{{ query }}</p>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
#reportHeader {
	position: -webkit-sticky;
	position: sticky;
	top: 0;
	left: 0;
	width: 100vw;
	background-color: #e7e7e7;
	padding-bottom: 0.5rem;
}

#reportHeader h5 {
	line-height: 1;
	font-size: 1rem;
}

#reportmenutext {
	padding-left: 1rem;
	padding-top: 0;
	padding-bottom: 0;
	padding-right: 1.5rem;
}

#reportView {
	width: max-content;
}

#pages {
	right: 0;
	margin-left: 10px;
	margin-right: 18px;
	line-height: 1;
}

.headerItem {
	margin-top: 0.5rem;
}

.toggleitem {
	margin-top: 5px;
	margin-left: 5px;
	margin-right: 5px;
	margin-bottom: 3px;
	vertical-align: text-bottom;
}
</style>

<script setup lang="ts">
import { routeDefs } from "@/router/index";
import LoadingLogo from "@/components/LoadingLogo.vue";
import { ResultSet } from "../assets/ts/dataMap/ResultSet";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { ApiNode } from "@/assets/ts/dataMap/ApiNode";
import { Report } from "@/assets/ts/dataMap/Report";
import { ConsolePlugin } from "@/assets/ts/dataMap/ConsolePlugin";
import { foundation } from "../mixins/foundation";
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import LStore from "@/assets/ts/LocalStorageManager";
import { bus, events } from "@/bus";
import { useStore } from "@/store";
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";

const store = useStore();
const route = useRoute();
const router = useRouter();

let results = ref<ResultSet>(null);
let statusMessage = ref("");
let maxRecords = ref(100);
let pageNum = ref(1);
let pageCount = ref(0);
let query = ref("");
let showQuery = ref(false);
let plugin: ConsolePlugin;
let report: Report;
let reportName = ref("");
let resultsLoaded = ref(false);
let columnStates: Dictionary<boolean> = {}; //column name as supplied by report property name, and whether enabled
let activePropertyNames: string[] = [];




onMounted(() => {
	const contentPane = document.getElementById("contentPane");
	contentPane.style.overflow = "scroll";
	//#region init code
	if (store.state.pluginManager !== null) {
		updateData();
	} else {
		const unwatch = store.watch(
			() => {
				return store.state.pluginManager;
			},
			newval => {
				if (newval !== null) {
					updateData();
					unwatch();
				}
			}
		);
	}
	//#endregion
});

const nodesPage = computed((): ApiNode[] => {
	const toprecord = pageNum.value * maxRecords.value;
	const bottomrecord = (pageNum.value - 1) * maxRecords.value;
	if (results.value === null) {
		return [];
	} else {
		return results.value.nodes.slice(bottomrecord, toprecord);
	}
});

const apiReady = computed((): boolean => {
	return store.state.apiState === api.states.READY;
});

const resultCount = computed((): number => {
	//console.log({source:"resultCount", results: results.value});
	if (results.value === null) {
		return 0;
	} else {
		return results.value.nodes.length;
	}
});

const propertyNames = computed((): string[] => {
	if (results.value !== null) {
		return Object.keys(columnStates);
	} else {
		return null;
	}
});

const hasNextPage = computed((): boolean => {
	return pageNum.value < pageCount.value;
});

const hasPrevPage = computed((): boolean => {
	return pageNum.value > 1;
});

const nextBtnVisibility = computed((): string => {
	return hasNextPage ? "visible" : "hidden";
});

const prevBtnVisibility = computed((): string => {
	return hasPrevPage ? "visible" : "hidden";
});

function updateActiveProperties(): void {
	setTimeout(() => {
		if (results.value !== null) {
			activePropertyNames = Object.keys(columnStates).filter(name => {
				return columnStates[name];
			});
		} else {
			return null;
		}
	}, 1);
}

function updateData() {
	const pluginName = route.query.pluginName as string;
	const reportName = route.query.reportName as string;

	//check if we're looking for a defined report, or importing from the
	//browser local storage
	if (pluginName) {
		updatePluginReportData(reportName, pluginName);
	} else {
		const nodes: ApiNode[] = LStore.popPendingNodeList();
		if (nodes !== null) {
			const nodeids: string[] = [];
			nodes.forEach(node => {
				nodeids.push(node.dbId);
			});
			updateIdsData(nodeids);
		} else {
			resultsLoaded.value = true;
		}
	}
}

function updateIdsData(ids: string[]) {
	const url = "/api/reports/nodes";
	const postData = JSON.stringify({ ids: ids, propertyfilters: ["name", "type", "id"] });

	const request: Request = {
		url: url,
		postJson: true,
		data: postData,
		successCallback: (data: ResultSet) => {
			applyData(data);
		},
		errorCallback: () => {
			resultsLoaded.value = true;
			bus.emit(events.Notifications.Error, "Error downloading report data");
		},
	};
	api.post(request);
}

function updatePluginReportData(reportName: string, pluginName: string) {
	plugin = store.state.pluginManager.plugins[pluginName] as ConsolePlugin;
	report = plugin.reports[reportName] as Report;
	query.value = report.query;

	const url = "/api/reports/report/?pluginname=" + pluginName + "&reportname=" + reportName;

	const request: Request = {
		url: url,
		successCallback: (data: ResultSet) => {
			applyData(data);
		},
		errorCallback: () => {
			// eslint-disable-next-line
			console.error("Error downloading report data");
		},
	};
	api.get(request);
}

function applyData(data: ResultSet) {
	results.value = data;

	if (results.value !== null) {
		pageCount.value = Math.ceil(results.value.nodes.length / maxRecords.value);
	} else {
		pageCount.value = 0;
	}

	// if property filters list is empty, show everything
	if (results.value.propertyFilters.length === 0) {
		results.value.propertyNames.forEach(name => {
			columnStates[name] = true;
		});
	} else {
		results.value.propertyFilters.forEach(name => {
			columnStates[name] = true;
		});

		results.value.propertyNames.forEach(name => {
			if (!columnStates[name]) {
				columnStates[name] = false;
			}
		});
	}

	resultsLoaded.value = true;
	updateActiveProperties();
}

function onVisualizerClicked() {
	LStore.storePendingResultSet(results.value);
	const route = router.resolve({ path: routeDefs.visualizer.path });
	window.open(route.href, "_blank");
}

function onPageUpClicked() {
	if (hasNextPage) {
		pageNum.value++;
	}
}

function onPageDownClicked() {
	if (hasPrevPage) {
		pageNum.value--;
	}
}

function onDownloadClicked() {
	statusMessage.value = "";
	let text = "";
	const props = [];

	function createRowArray(record) {
		const recordrow = [];
		//console.log(node);
		props.forEach(function (prop) {
			let celltext = "";
			if (prop in record.properties) {
				celltext = record.properties[prop];
				celltext = celltext.toString().replace('"', '""');
				celltext = '"' + celltext + '"';
			}

			recordrow.push(celltext);
		});
		return recordrow;
	}

	//header
	activePropertyNames.forEach(name => {
		props.push(name);
	});
	text = text + props.join(",") + "\n";

	//main content
	results.value.nodes.forEach(function (node) {
		const row = createRowArray(node);
		text = text + row.join(",") + "\n";
	});

	results.value.edges.forEach(function (node) {
		const row = createRowArray(node);
		text = text + row.join(",") + "\n";
	});

	webcrap.misc.download(text, "results.value.csv", "text/csv;encoding:utf-8");
}

function onShowQueryClicked(): void {
	showQuery.value = true;
}

function onQueryCloseClicked(): void {
	showQuery.value = false;
}
</script>