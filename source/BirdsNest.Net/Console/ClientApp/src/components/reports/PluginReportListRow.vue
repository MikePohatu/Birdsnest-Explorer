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
  <tr>
    <td>
      <a v-on:click="openReport">{{ report.displayName }}</a>
    </td>
    <td>{{ report.description }}</td>
  </tr>
</template>

<script setup lang="ts">
  import { Report } from "@/assets/ts/dataMap/Report";
  const props = defineProps({
    report: { type: Report, required: true },
    reportName: { type: String, required: true },
    pluginName: { type: String, required: true }
  });

  function openReport(): void {
    //const reportPath = { path: '/report',  query: this.report } as RawLocation;
    this.$router.push({
      path: '/report',
      query: { reportName:this.reportName, pluginName:this.pluginName }
    });
  }

</script>