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
	<div class="reports page">
		<PluginReportList v-for="(value, name) in plugins" :key="name" :plugin="value" />
	</div>
</template>

<style scoped>
.reports {
	top: 0;
	bottom: 0;
	right: 0;
	left: 0;
	overflow: auto;
}
</style>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import PluginReportList from "@/components/reports/PluginReportList.vue";

@Component({
	components: { PluginReportList },
})
export default class Reports extends Vue {
	get plugins(): Plugin[] {
		return this.$store.state.pluginManager === null ? [] : this.$store.state.pluginManager.plugins;
	}
}
</script>