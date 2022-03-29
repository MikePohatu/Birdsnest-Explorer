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
	<div id="adminPageWrapper" class="page grid-x grid-margin-x">
		<!-- PLUGINS -->
		<div v-if="pluginManager !== null" id="pluginsWrapper" class="cell shrink medium-6">
			<table id="pluginsTable" class="hover">
				<thead>
					<tr>
						<th>{{ $t('phrase_Active_Plugins') }}</th>
						<th v-if="pluginManager.extensionCount > 0">{{ $t('word_Extensions') }}</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="(plugin, name) in pluginManager.plugins" :key="name">
						<td v-if="plugin.link">
							<a :href="plugin.link" target="_blank">{{ plugin.displayName }}</a>
						</td>
						<td v-else>{{ plugin.displayName}}</td>
						<td v-if="pluginManager.extensionCount > 0" v-html="plugin.extendedBy.join('<br/>')" />
					</tr>
				</tbody>
			</table>
		</div>

		<div class="cell shrink medium-6" id="actionsWrapper">
			<h6>{{ $t('word_Actions') }}</h6>
			<table id="actionsTable">
				<tbody>
					<tr>
						<td width="100">{{ $t('phrase_Reload_Plugins') }}</td>
						<td width="100">
							<button type="button" class="button" v-on:click="onReloadPlugins">{{ $t('word_Go') }}</button>
						</td>
						<td id="reloadMessage">{{ reloadMessage }}</td>
					</tr>
					<tr>
						<td>{{ $t('phrase_Index_Editor') }}</td>
						<td>
							<router-link
								:to="routeDefs.indexEditor.path"
								class="button"
								tag="button"
							>{{ $t('word_Go') }}</router-link>
						</td>
						<td />
					</tr>
				</tbody>
			</table>
		</div>
	</div>
</template>


<style scoped>
#pluginsWrapper,
#actionsWrapper,
#actionsWrapper .button {
	font-size: 0.7rem;
}

#actionsWrapper .button {
	width: 75px;
	margin: 0;
}

#actionsTable {
	table-layout: auto;
}
</style>


<script setup lang="ts">
	import { bus, events } from "@/bus";
	import { api, Request } from "../assets/ts/webcrap/apicrap";
	import { auth } from "../assets/ts/webcrap/authcrap";
	import { Dictionary } from "lodash";
	import { rootPaths } from "@/store/index";
	import PluginManager from "@/assets/ts/dataMap/PluginManager";
	import { routeDefs } from "@/router/index";
	import {computed} from 'vue';
	import { useStore } from "@/store";

	const store = useStore();
	let reloadMessage = "";

	auth.getValidationToken();
	
	const pluginManager = computed((): PluginManager => {
		return store.state.pluginManager;
	});

	function onReloadPlugins() {
		const request: Request = {
			url: "/api/admin/reloadplugins",
			postJson: true,
			successCallback: (data: Dictionary<string>) => {
				this.reloadMessage = data.message;
				store.dispatch(rootPaths.actions.UPDATE_PLUGINS);
			},
			errorCallback: (jqXHR, status: string, error: string) => {
				this.reloadMessage = error;
			},
		};
		bus.emit(events.Notifications.Processing, "Processing");
		api.post(request);
		return false;
	}
</script>
