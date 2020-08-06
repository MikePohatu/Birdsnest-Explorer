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
  <div class="page">
    <div class="grid-x grid-margin-x">
      <div class="cell medium-3 large-2">
        <span>
          <button type="button" class="button" v-on:click="onReloadPlugins">Reload Plugins</button>
        </span>
      </div>
      <div class="messages cell medium-9 large-10">
        <span>{{ reloadMessage }}</span>
      </div>
    </div>
  </div>
</template>


<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { api, Request } from "../assets/ts/webcrap/apicrap";
import { auth } from "../assets/ts/webcrap/authcrap";
import { Dictionary } from "vue-router/types/router";
import { rootPaths } from "@/store/index";

@Component
export default class Admin extends Vue {
  reloadMessage = "";

  created(): void {
    auth.getValidationToken();
  }

  onReloadPlugins() {
    const request: Request = {
      url: "/api/admin/reloadplugins",
      postJson: true,
      successCallback: (data: Dictionary<string>) => {
        this.reloadMessage = data.message;
        this.$store.dispatch(rootPaths.actions.UPDATE_PLUGINS);
      },
      errorCallback: (jqXHR: JQueryXHR, status: string, error: string) => {
        this.reloadMessage = error;
      }
    };
    api.post(request);
    return false;
  }
}
</script>

<style scoped>
.messages {
  padding-top: 0.5em;
}
</style>