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
	<div>
		<div class="portalBoxHeading">
			<router-link to="info">{{ $t('phrase_Server_Information') }}</router-link>
		</div>
		<div>
			<router-link to="info">
				<div class="portalBox">
					<table>
						<tbody>
							<tr>
								<td class="left">{{ $t('phrase_Installed_Plugins') }}</td>
								<td v-if="serverInfoReady" class="right">{{ Object.keys(pluginManager.plugins).length }}</td>
								<td v-else class="loading"></td>
							</tr>
							<tr>
								<td class="left">{{ $t('word_Nodes') }}</td>
								<td v-if="serverInfoReady" class="right">{{ serverInfo.dbStats.totals["nodes"] }}</td>
								<td v-else class="loading"></td>
							</tr>
							<tr>
								<td class="left">{{ $t('word_Relationships') }}</td>
								<td v-if="serverInfoReady" class="right">{{ serverInfo.dbStats.totals["edges"] }}</td>
								<td v-else class="loading"></td>
							</tr>
							<tr>
								<td class="left">{{ $t('phrase_Server_Version') }}</td>
								<td v-if="serverInfoReady" class="right">{{ serverInfo.serverVersion }}</td>
								<td v-else class="loading"></td>
							</tr>
							<tr>
								<td class="left">{{ $t('phrase_Database_Version') }}</td>
								<td v-if="serverInfoReady" class="right">{{ serverInfo.dbStats.version }}</td>
								<td v-else class="loading"></td>
							</tr>
						</tbody>
					</table>
				</div>
			</router-link>
		</div>
		<div class="description">{{ $t('portal.server_info.info') }}</div>
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