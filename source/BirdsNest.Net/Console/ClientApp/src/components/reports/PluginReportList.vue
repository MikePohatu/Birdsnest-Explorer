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
                    <th>{{ pluginName }} {{ $t('word_Reports') }}</th>
                    <th>{{ $t('word_Description') }}</th>
                </tr>
            </thead>
            <tbody>
                <PluginReportListRow v-for="(value, name) in reports" :key="name" :report="value" :reportName="(name as string)" :pluginName="plugin.name" />
            </tbody>
        </table>
    </div>
</template>

<style scoped>
table {
  table-layout: fixed;
}
</style>
<script setup lang="ts">
import PluginReportListRow from "@/components/reports/PluginReportListRow.vue";
import { Plugin } from "@/assets/ts/dataMap/Plugin";

    const props = defineProps({ plugin: { type: Plugin, required: true }})

    const pluginName = props.plugin.displayName;
    const reports = props.plugin.reports;
    const reportNames = Object.keys(props.plugin.reports);

</script>