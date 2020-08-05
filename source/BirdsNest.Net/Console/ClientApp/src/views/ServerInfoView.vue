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
	<div class="page">
		<div class="watermark">
			<img src="/img/icons/logo.svg" height="512px" width="512px" />
		</div>
		
		<div>
			<h6>Server Statistics</h6>
			
			<Loading v-if="!statsDataReady" />
			
			<div v-else id="statsWrapper">
				<div class="grid-x grid-margin-x">
					<!-- PLUGINS -->
					<div class="cell shrink medium-cell-block-y large-3">
						<table id="pluginsTable" class="hover">
							<thead>
								<tr>
									<th>Active Plugins</th>
								</tr>
							</thead>
							<tbody>
								<tr v-for="(plugin, name) in pluginManager.plugins" :key="name">
									<td v-if="plugin.link">
										<a :href="plugin.link" target="_blank">{{ plugin.displayName }}</a>
									</td>
									<td v-else>{{ plugin.displayName}}</td>
								</tr>
							</tbody>
						</table>
					</div>

					<!-- NODES -->
					<div class="cell shrink medium-cell-block-y large-3">
						<table class="hover">
							<thead>
								<tr>
									<th class="left">Node Types</th>
									<th class="right">Count</th>
								</tr>
							</thead>
							<tbody>
								<tr v-for="(value, name) in serverInfo.dbStats.nodeLabelCounts" :key="name">
									<td class="left">{{ name }}</td>
									<td class="right">{{ value }}</td>
								</tr>
							</tbody>
						</table>
					</div>

					<!-- EDGES -->
					<div class="cell shrink medium-cell-block-y large-3">
						<table class="hover">
							<thead>
								<tr>
									<th class="left">Relationship Types</th>
									<th class="right">Count</th>
								</tr>
							</thead>
							<tbody>
								<tr v-for="(value, name) in serverInfo.dbStats.edgeLabelCounts" :key="name">
									<td class="left">{{ name }}</td>
									<td class="right">{{ value }}</td>
								</tr>
							</tbody>
						</table>
					</div>

					<!-- TOTALS -->
					<div class="cell shrink medium-cell-block-y large-3">
						<table class="hover">
							<thead>
								<tr>
									<th class="left">Database Information</th>
									<th class="right"></th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<td class="left">Total Nodes</td>
									<td class="right">{{ serverInfo.dbStats.totals["nodes"] }}</td>
								</tr>
								<tr>
									<td class="left">Total Relationships</td>
									<td class="right">{{ serverInfo.dbStats.totals["edges"] }}</td>
								</tr>
								<tr>
									<td class="left">Version</td>
									<td class="right">{{ serverInfo.dbStats.version }}</td>
								</tr>
								<tr>
									<td class="left">Edition</td>
									<td class="right">{{ serverInfo.dbStats.edition }}</td>
								</tr>
							</tbody>
						</table>
					</div>
				</div>	

				<!-- Refresh button -->
				<div id="refreshBtn" class="icon clickable" title="Refresh" v-on:click="onRefreshClicked">&#xf021;</div>	
			</div>
		</div>
	</div>
</template>

<style scoped>
#statsWrapper {
	position: relative;
	border-radius: 7px;
	border-width: 2px;
	border-color: lightgray;
	border-style: solid;
	background-color: rgba(235, 235, 235, 0.281);
	padding: 10px;
}

#refreshBtn {
	position: absolute;
	bottom: 0;
	right: 0;
	width: 32px;
	height: 32px;
	padding: 2px;
	font-size: 20px;
	color: #1779ba;
}

#refreshBtn:hover {
	padding: 0;
	font-size: 24px;
}

.watermark {
    position: fixed;
    top: 0px;
    right: 0px;
    opacity: 0.03;
    z-index: -10;
}

tr {
	font-size: 12px;
}

td {
	width: min-content;
	font-size: 10px;
}

.left {
	text-align: left;
}

.right {
	text-align: right;
}
</style>


<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { api } from "@/assets/ts/webcrap/apicrap";
import Loading from "@/components/Loading.vue";
import PluginManager from "@/assets/ts/dataMap/PluginManager";
import ServerInfo from "@/assets/ts/dataMap/ServerInfo";
import { rootPaths } from "@/store/index";

@Component({
	components: {
		Loading,
	},
})
export default class ServerInfoView extends Vue {
	api = api;

	get statsDataReady(): boolean {
		return (
			this.serverInfoState === api.states.READY &&
			this.serverInfo !== null &&
			this.apiState === api.states.READY &&
			this.pluginManager !== null
		);
	}

	get serverInfoState(): number {
		return this.$store.state.serverInfoState;
	}

	get serverInfo(): ServerInfo {
		return this.$store.state.serverInfo;
	}

	get apiState(): number {
		return this.$store.state.apiState;
	}

	get pluginManager(): PluginManager {
		return this.$store.state.pluginManager;
	}

	onRefreshClicked(): void {
        this.$store.dispatch(rootPaths.actions.UPDATE_SERVER_INFO);
	}
}
</script>
