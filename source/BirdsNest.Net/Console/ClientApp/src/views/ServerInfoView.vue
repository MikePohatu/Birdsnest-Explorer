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
	<div id="infoPageWrapper" class="page">
		<div class="watermark">
			<img src="/img/icons/logo.svg" height="512px" width="512px" />
		</div>

		<div>
			<div id="infoHeader">
				<h6>{{ $t("phrase_Server_Statistics") }}</h6>
				<!-- Refresh button -->
				<div
					id="refreshBtn"
					class="icon clickable"
					:title="$t('word_Refresh')"
					v-on:click="onRefreshClicked"
				>&#xf021;</div>
			</div>

			<div id="statsWrapper">
				<Loading v-if="!statsDataReady" />
				<div v-else>
					<div class="grid-x grid-margin-x">
						<div class="cell shrink medium-cell-block-y large-3">
							<!-- SERVER INFO -->
							<table id="serverInfoTable" class="hover">
								<thead>
									<tr>
										<th>{{ $t("phrase_Server_Information") }}</th>
										<th class="right"></th>
									</tr>
								</thead>
								<tbody>
									<tr>
										<td class="left">{{ $t("phrase_Server_Version") }}</td>
										<td class="right">{{ serverInfo.serverVersion }}</td>
									</tr>
								</tbody>
							</table>

							<!-- DATABASE INFO -->
							<table class="hover">
								<thead>
									<tr>
										<th class="left">{{ $t("phrase_Database_Information") }}</th>
										<th class="right"></th>
									</tr>
								</thead>
								<tbody>
									<tr>
										<td class="left">{{ $t("phrase_Total_Nodes") }}</td>
										<td class="right">{{ serverInfo.dbStats.totals["nodes"] }}</td>
									</tr>
									<tr>
										<td class="left">{{ $t("phrase_Total_Relationships") }}</td>
										<td class="right">{{ serverInfo.dbStats.totals["edges"] }}</td>
									</tr>
									<tr>
										<td class="left">{{ $t("word_Version") }}</td>
										<td class="right">{{ serverInfo.dbStats.version }}</td>
									</tr>
									<tr>
										<td class="left">{{ $t("word_Edition") }}</td>
										<td class="right">{{ serverInfo.dbStats.edition }}</td>
									</tr>
								</tbody>
							</table>

							<!-- PLUGINS -->
							<table id="pluginsTable" class="hover">
								<thead>
									<tr>
										<th>{{ $t("phrase_Active_Plugins") }}</th>
										<th v-if="pluginManager.extensionCount > 0">{{ $t("word_Extensions") }}</th>
									</tr>
								</thead>
								<tbody>
									<tr v-for="(plugin, name) in pluginManager.plugins" :key="name">
										<td v-if="plugin.link">
											<a :href="plugin.link" target="_blank">{{ plugin.displayName }}</a>
										</td>
										<td v-else>{{ plugin.displayName }}</td>
										<td v-if="pluginManager.extensionCount > 0" v-html="plugin.extendedBy.join('<br/>')" />
									</tr>
								</tbody>
							</table>
						</div>

						<!-- INDEXES -->
						<div class="cell shrink medium-cell-block-y large-3">
							<table id="indexesTable" class="hover">
								<thead>
									<tr>
										<th>
											{{ $t("word_Indexes") }}
											<router-link v-if="isAdmin" :to="routeDefs.indexEditor.path" class="sublink">Edit</router-link>
										</th>
										<th>{{ $t("word_Property") }}</th>
										<!-- <th>Index Name</th> -->
										<th>{{ $t("word_Status") }}</th>
									</tr>
								</thead>

								<tbody
									v-for="(value, name) in serverInfo.indexes"
									:key="name"
									name="indexesTransition"
									is="transition-group"
								>
									<tr v-for="index in value" :key="index.propertyName">
										<td>{{ name }}</td>
										<td>{{ index.propertyName }}</td>
										<!-- <td>{{ index.indexName }}</td> -->
										<td>{{ index.state }}</td>
									</tr>
								</tbody>
							</table>
						</div>
						<!-- NODES -->
						<div class="cell shrink medium-cell-block-y large-3">
							<table class="hover">
								<thead>
									<tr>
										<th class="left">{{ $t("phrase_Node_Types") }}</th>
										<th class="right">{{ $t("word_Count") }}</th>
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
										<th class="left">{{ $t("phrase_Relationship_Types") }}</th>
										<th class="right">{{ $t("word_Count") }}</th>
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
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<style>
</style>

<style scoped>
#infoPageWrapper {
	margin-right: 20px;
}

#infoHeader {
	position: relative;
}

#statsWrapper {
	position: relative;
	border-radius: 7px;
	border-width: 2px;
	border-color: lightgray;
	border-style: solid;
	background-color: rgba(235, 235, 235, 0.281);
	padding: 10px;
	min-height: 200px;
}

#refreshBtn {
	position: absolute;
	top: 0;
	right: 0;
	width: 32px;
	height: 32px;
	padding: 2px;
	font-size: 20px;
	color: #1779ba;
	opacity: 0.9;
}

#refreshBtn:hover {
	padding: 0;
	font-size: 24px;
}

.sublink {
	font-weight: normal;
	font-size: 0.7em;
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
import { useStore } from "@/store";
import { computed, defineComponent, watch } from "vue";
import { api } from "@/assets/ts/webcrap/apicrap";
import Loading from "@/components/Loading.vue";
import PluginManager from "@/assets/ts/dataMap/PluginManager";
import ServerInfo from "@/assets/ts/dataMap/ServerInfo";
import { rootPaths } from "@/store/index";

export default defineComponent({
	components: {
		Loading
	},
	setup() {
		const store = useStore();

		const beforeDestroy = computed(() => {
			const contentPane = document.getElementById("contentPane");
			contentPane.style.overflow = null;
		});

		const isAdmin = computed(() => {
			return store.state.user.isAdmin;
		});

		const statsDataReady = computed((): boolean => {
			return (
				serverInfoState.value === api.states.READY &&
				serverInfo.value !== null &&
				apiState.value === api.states.READY &&
				pluginManager.value !== null
			);
		});

		const serverInfoState = computed(():  number => {
			return store.state.serverInfoState;
		});

		const serverInfo = computed(():  ServerInfo => {
			return store.state.serverInfo;
		});

		const apiState = computed(():  number => {
			return store.state.apiState;
		});

		const pluginManager = computed(():  PluginManager => {
			return store.state.pluginManager;
		});

		const onRefreshClicked = computed(():  void => {
			store.dispatch(rootPaths.actions.UPDATE_SERVER_INFO);
		});

		return {
			onRefreshClicked, pluginManager, apiState, serverInfo, serverInfoState, statsDataReady, isAdmin, beforeDestroy
		}
	},
	mounted() {
		const contentPane = document.getElementById("contentPane");
		contentPane.style.overflow = "scroll";
	}
})
</script>
