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
  <tr>
    <td>
      <a v-on:click="openReport">{{ report.displayName }}</a>
    </td>
    <td>{{ report.description }}</td>
  </tr>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import { Report } from "@/assets/ts/dataMap/Report";

@Component
export default class PluginReportListRow extends Vue {
  @Prop({ type: Object as () => Report, required: true })
  report: Report;

  @Prop({ type: String, required: true })
  reportName: string;

  @Prop({ type: String, required: true })
  pluginName: string;

  openReport(): void {
    //const reportPath = { path: '/report',  query: this.report } as RawLocation;
    this.$router.push({
      path: '/report',
      query: { reportName:this.reportName, pluginName:this.pluginName }
    });
  }
}
</script>