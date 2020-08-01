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
	<div id="report-view">
		<div id="report-menu" class="grid-x">
			<div class="cell shrink" id="reportmenu">
				<ul
					class="dropdown menu"
					data-dropdown-menu
					data-close-on-click="false"
					data-close-on-click-inside="false"
				>
					<li>
						<a id="reportmenutext">Query returned {{ resultCount }} records</a>
						<ul class="menu">
							<li id="columnsli">
								<a href="#">Columns</a>
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
								<a href="#" v-on:click="onDownloadClicked">Download</a>
							</li>
							<li id="visualizerli">
								<a href="#" v-on:click="onVisualizerClicked">Open in Visualizer</a>
							</li>
							<li v-if="query !== ''">
								<a href="#" v-on:click="onShowQueryClicked">Show query</a>
							</li>
						</ul>
					</li>
				</ul>
			</div>

			<div class="cell auto"/>
			<div class="x-center show-for-medium reportHeader" id="status"><h5>{{ reportName }}</h5></div>

			<div id="pages" class="cell shrink">
				<span id="prevPageBtn" style="margin:5px;" :class="{ hidden: !hasPrevPage }">
					<a href="#" v-on:click="onPageDownClicked">
						<i class="fas fa-caret-left"></i>
					</a>
				</span>
				<span
					style="margin:5px;"
					class="text-center"
					id="pageMessage"
				>Page {{ pageNum }} of {{ pageCount }}</span>
				<span id="nextPageBtn" style="margin:5px;" :class="{ hidden: !hasNextPage }">
					<a href="#" v-on:click="onPageUpClicked">
						<i class="fas fa-caret-right"></i>
					</a>
				</span>
			</div>
		</div>

		<!-- Main output area -->
		<Loading v-if="!resultsLoaded">Loading</Loading>
		<div v-else id="output" class="table-scroll">
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
					<h5><u>Neo4j Cypher Query</u></h5>
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
#output {
	position: absolute;
	height: 100%;
	width: 100%;
}

#outputtable thead tr {
	position: sticky;
	top: 0;
}

.toggleitem {
	margin-top: 5px;
	margin-left: 5px;
	margin-right: 5px;
	margin-bottom: 3px;
	vertical-align: text-bottom;
}

.reportHeader {
	padding-top: 5px;
}
</style>

<script lang="ts">
import { routeDefs } from "@/router/index";
import { Component, Vue } from "vue-property-decorator";
import Loading from "@/components/Loading.vue";
import { ResultSet } from "../assets/ts/dataMap/ResultSet";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { ApiNode } from "@/assets/ts/dataMap/ApiNode";
import { Report } from "@/assets/ts/dataMap/Report";
import { Plugin } from "@/assets/ts/dataMap/Plugin";
import { foundation } from "../mixins/foundation";
import { Dictionary } from "vue-router/types/router";
import LStore from "@/assets/ts/LocalStorageManager";
import { bus, events } from "@/bus";

@Component({
	components: { Loading },
	mixins: [foundation],
})
export default class ReportView extends Vue {
	results: ResultSet = null;
	statusMessage = "";
	maxRecords = 100;
	pageNum = 1;
	pageCount = 0;
	query = "";
	showQuery = false;
	plugin: Plugin;
	report: Report;
	reportName = "";
	resultsLoaded = false;
	columnStates: Dictionary<boolean> = {}; //column name as supplied by report property name, and whether enabled
	activePropertyNames: string[] = [];

	get nodesPage(): ApiNode[] {
		const toprecord = this.pageNum * this.maxRecords;
		const bottomrecord = (this.pageNum - 1) * this.maxRecords;
		if (this.results === null) {
			return [];
		} else {
			return this.results.nodes.slice(bottomrecord, toprecord);
		}
	}

	get apiReady(): boolean {
		return this.$store.state.apiState === api.states.READY;
	}

	get resultCount(): number {
		if (this.results === null) {
			return 0;
		} else {
			return this.results.nodes.length;
		}
	}

	get propertyNames(): string[] {
		if (this.results !== null) {
			return Object.keys(this.columnStates);
		} else {
			return null;
		}
	}

	get hasNextPage(): boolean {
		return this.pageNum < this.pageCount;
	}

	get hasPrevPage(): boolean {
		return this.pageNum > 1;
	}

	get nextBtnVisibility(): string {
		return this.hasNextPage ? "visible" : "hidden";
	}

	get prevBtnVisibility(): string {
		return this.hasPrevPage ? "visible" : "hidden";
	}

	created(): void {
		if (this.$store.state.pluginManager !== null) {
			this.updateData();
		} else {
			const unwatch = this.$store.watch(
				() => {
					return this.$store.state.pluginManager;
				},
				newval => {
					if (newval !== null) {
						this.updateData();
						unwatch();
					}
				}
			);
		}
	}

	updateActiveProperties(): void {
		setTimeout(() => {
			if (this.results !== null) {
				this.activePropertyNames = Object.keys(this.columnStates).filter(name => {
					return this.columnStates[name];
				});
			} else {
				return null;
			}
		}, 1);
	}

	updateData() {
		const pluginName = this.$route.query.pluginName as string;
		const reportName = this.$route.query.reportName as string;

		//check if we're looking for a defined report, or importing from the
		//browser local storage
		if (pluginName) {
			this.updatePluginReportData(reportName, pluginName);
		} else {
			const nodes: ApiNode[] = LStore.popPendingNodeList();
			if (nodes !== null) {
				const nodeids: string[] = [];
				nodes.forEach(node => {
					nodeids.push(node.dbId);
				});
				this.updateIdsData(nodeids);
			} else {
				this.resultsLoaded = true;
			}
		}
	}

	updateIdsData(ids: string[]) {
		const url = "/api/reports/nodes";
		const postData = JSON.stringify({ ids: ids, propertyfilters: ["name", "type", "id"] });

		const request: Request = {
			url: url,
			postJson: true,
			data: postData,
			successCallback: (data: ResultSet) => {
				this.applyData(data);
			},
			errorCallback: () => {
				this.resultsLoaded = true;
				bus.$emit(events.Notifications.Error, "Error downloading report data");
			},
		};
		api.post(request);
	}

	updatePluginReportData(reportName: string, pluginName: string) {
		this.plugin = this.$store.state.pluginManager.plugins[pluginName] as Plugin;
		this.report = this.plugin.reports[reportName] as Report;
		this.query = this.report.query
		this.reportName = this.report.displayName;

		const url = "/api/reports/report/?pluginname=" + pluginName + "&reportname=" + reportName;

		const request: Request = {
			url: url,
			successCallback: (data: ResultSet) => {
				this.applyData(data);
			},
			errorCallback: () => {
				console.error("Error downloading report data");
			},
		};
		api.get(request);
	}

	applyData(data: ResultSet) {
		this.results = data;

		if (data !== null) {
			this.pageCount = Math.ceil(this.results.nodes.length / this.maxRecords);
		} else {
			this.pageCount = 0;
		}

		// if property filters list is empty, show everything
		if (this.results.propertyFilters.length === 0) {
			this.results.propertyNames.forEach(name => {
				this.columnStates[name] = true;
			});
		} else {
			this.results.propertyFilters.forEach(name => {
				this.columnStates[name] = true;
			});

			this.results.propertyNames.forEach(name => {
				if (!this.columnStates[name]) {
					this.columnStates[name] = false;
				}
			});
		}
		

		this.resultsLoaded = true;
		this.updateActiveProperties();
	}

	onVisualizerClicked() {
		LStore.storePendingResultSet(this.results);
		const route = this.$router.resolve({ path: routeDefs.visualizer.path });
		window.open(route.href, "_blank");
	}

	onPageUpClicked() {
		if (this.hasNextPage) {
			this.pageNum++;
		}
	}

	onPageDownClicked() {
		if (this.hasPrevPage) {
			this.pageNum--;
		}
	}

	onDownloadClicked() {
		this.statusMessage = "";
		let text = "";
		const props = [];

		function createRowArray(record) {
			const recordrow = [];
			//console.log(node);
			props.forEach(function(prop) {
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
		this.activePropertyNames.forEach(name => {
			props.push(name);
		});
		text = text + props.join(",") + "\n";

		//main content
		this.results.nodes.forEach(function(node) {
			const row = createRowArray(node);
			text = text + row.join(",") + "\n";
		});

		this.results.edges.forEach(function(node) {
			const row = createRowArray(node);
			text = text + row.join(",") + "\n";
		});

		this.download(text, "results.csv", "text/csv;encoding:utf-8");
	}

	onShowQueryClicked(): void {
		this.showQuery = true;
	}

	onQueryCloseClicked(): void {
		this.showQuery = false;
	}

	download(content, fileName, mimeType) {
		if (!mimeType) {
			mimeType = "application/octet-stream";
		}
		const blob = new Blob([content], {
			type: mimeType,
		});

		//IE
		if (navigator.msSaveBlob) {
			navigator.msSaveBlob(blob, fileName);
		}
		//HTML5
		else {
			const downel = document.createElement("a");
			downel.href = URL.createObjectURL(blob);
			downel.setAttribute("download", fileName);
			downel.click();
			downel.remove();
		}
	}
}
</script>