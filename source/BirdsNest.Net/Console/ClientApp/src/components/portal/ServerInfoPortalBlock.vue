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
	<div v-if="serverInfoReady">
		<div class="portalBoxHeading">
			<router-link to="info">Server Information</router-link>
		</div>
		<div>
			<router-link to="info">
				<div class="portalBox">
					<table>
						<tbody>
							<tr>
								<td class="left">Installed plugins</td>
								<td class="right">{{ Object.keys(pluginManager.plugins).length }}</td>
							</tr>
							<tr>
								<td class="left">Nodes</td>
								<td class="right">{{ serverInfo.dbStats.totals["nodes"] }}</td>
							</tr>
							<tr>
								<td class="left">Relationships</td>
								<td class="right">{{ serverInfo.dbStats.totals["edges"] }}</td>
							</tr>
							<tr>
								<td class="left">Server version</td>
								<td class="right">{{ serverInfo.serverVersion }}</td>
							</tr>
							<tr>
								<td class="left">Database Version</td>
								<td class="right">{{ serverInfo.dbStats.version }}</td>
							</tr>
						</tbody>
					</table>
				</div>
			</router-link>
		</div>
		<div class="description">Click to view server information and data statistics</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { api } from "@/assets/ts/webcrap/apicrap";
import ServerInfo from "@/assets/ts/dataMap/ServerInfo";
import PluginManager from "@/assets/ts/dataMap/PluginManager";

@Component
export default class ServerInfoPortalBlock extends Vue {
	api = api;

	get serverInfoReady(): boolean {
		return this.serverInfoState === api.states.READY && this.serverInfo !== null;
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
}
</script>