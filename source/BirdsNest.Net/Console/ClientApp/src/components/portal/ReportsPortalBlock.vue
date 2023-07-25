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
	<div v-if="pluginManagerReady">
		<div class="portalBoxHeading">
			<router-link to="reports">{{ $t('word_Reports')}}</router-link>
		</div>
		<div>
			<router-link to="reports">
				<div class="portalBox">
					<table>
						<tbody>
							<tr>
								<td class="left">{{ $t('phrase_Available_reports') }}</td>
								<td class="right">{{ reportCount }}</td>
							</tr>
							<tr>
								<td class="left">{{ $t('phrase_Plugins_with_reports') }}</td>
								<td class="right">{{ pluginCount }}</td>
							</tr>
							<tr>
								<td class="separator" colspan="2" />
							</tr>
							<tr v-for="reportName in firstReports" :key="reportName">
								<td class="left small" colspan="2">{{ reportName }}</td>
							</tr>
							<tr>
								<td class="left small" colspan="2">
									<router-link to="reports">{{ $t('word_More') }}...</router-link>
								</td>
							</tr>
						</tbody>
					</table>
				</div>
			</router-link>
		</div>
		<div
			class="description"
		>{{ $t('portal.reports.info') }}</div>
	</div>
</template>

<style scoped>
.separator {
	border-top: 1px solid #d3d3d3;
	padding: 0;
}

td.small {
	font-size: 8px;
	padding: 0.25em 0.75rem;
}
</style>

<script setup lang="ts">
import { api } from "@/assets/ts/webcrap/apicrap";
import PluginManager from "@/assets/ts/dataMap/PluginManager";
import { computed } from "vue";
import { useStore } from "@/store";
	const store = useStore();

	const pluginManager = computed((): PluginManager => {
		return store.state.pluginManager as PluginManager;
	});

	const apiState = computed((): number => {
		return store.state.apiState;
	});

	const pluginManagerReady = computed((): boolean => {
		return apiState.value === api.states.READY && pluginManager.value !== null;
	});

	const firstReports = computed((): string[] => {
		const reportCount = 5;
		const reportNameList = [];
		let count = 0;

		const pluginNames = Object.keys(pluginManager.value.plugins);
		for (let i = 0; count < reportCount && i < pluginNames.length; i++) {
			const pluginName: string = pluginNames[i];
			const reports = pluginManager.value.plugins[pluginName].reports;
			const reportNames = Object.keys(reports);

			for (let j = 0; count < reportCount && j < reportNames.length; j++) {
				const reportName = reportNames[j];
				const report = reports[reportName];
				reportNameList.push(report.displayName);
				count++;
			}
		}

		return reportNameList;
	});

	const reportCount = computed((): number => {
		let count = 0;
		Object.keys(pluginManager.value.plugins).forEach(key => {
			const reports = pluginManager.value.plugins[key].reports;
			count = count + Object.keys(reports).length;
		});

		return count;
	});

	const pluginCount = computed((): number => {
		let count = 0;
		Object.keys(pluginManager.value.plugins).forEach(key => {
			const reports = pluginManager.value.plugins[key].reports;
			if (Object.keys(reports).length > 0) {
				count++;
			}
		});

		return count;
	});
</script>