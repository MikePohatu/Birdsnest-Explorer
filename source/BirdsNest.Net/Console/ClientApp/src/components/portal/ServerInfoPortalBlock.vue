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
    <div v-if="serverInfoReady">
        <div>
            <router-link to="info">Server Information</router-link>
        </div>
        <div>
            <router-link to="info">
                <table class="hover portalBox">
                    <tbody>
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
                    </tbody>
                </table>
            </router-link>
        </div>
        <div class="description">Click to view server information and data statistics</div>
    </div>
</template>

<style scoped>
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
import ServerInfo from "@/assets/ts/dataMap/ServerInfo";

@Component
export default class ServerInfoPortalBlock extends Vue {
    api = api;

	get serverInfoReady(): boolean {
		return (
			this.serverInfoState === api.states.READY &&
			this.serverInfo !== null 
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
}
</script>