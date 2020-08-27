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
    <div v-if="reportNames.length > 0" class="table-scroll">
        <table id="outputtable" class="hover">
            <thead>
                <tr>
                    <th>{{ pluginName }} Reports</th>
                    <th>Description</th>
                </tr>
            </thead>
            <tbody>
                <PluginReportListRow v-for="(value, name) in reports" :key="name" :report="value" :reportName="name" :pluginName="plugin.name" />
            </tbody>
        </table>
    </div>
</template>

<style scoped>
table {
  table-layout: fixed;
}
</style>
<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import PluginReportListRow from "@/components/reports/PluginReportListRow.vue";
import { Plugin } from "@/assets/ts/dataMap/Plugin";

@Component({
  components: { PluginReportListRow }
})
export default class PluginReportList extends Vue {
    @Prop({ type: Object as () => Plugin, required: true })
    plugin: Plugin;

    pluginName = this.plugin.displayName;
    reports = this.plugin.reports;
    reportNames = Object.keys(this.plugin.reports);
}
</script>