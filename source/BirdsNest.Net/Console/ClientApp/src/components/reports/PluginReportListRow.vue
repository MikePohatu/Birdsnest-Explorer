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

<script setup lang="ts">import { useRouter } from 'vue-router';
  import { Report } from "@/assets/ts/dataMap/Report";

  const router = useRouter();  
  const props = defineProps({
    report: { type: Object, required: true },
    reportName: { type: String, required: true },
    pluginName: { type: String, required: true }
  });

  const report = props.report as Report;

  function openReport(): void {
    //const reportPath = { path: '/report',  query: this.report } as RawLocation;
    //console.log({source: "openReport", reportName: props.reportName, pluginName:props.pluginName });
    router.push({
      path: '/report',
      query: { reportName:props.reportName, pluginName:props.pluginName }
    });
  }

</script>