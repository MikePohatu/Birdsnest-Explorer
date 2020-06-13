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
  <div class="portal page">

<p>{{ welcomeMessage }}</p>

<div class="grid-x grid-margin-x">
    <span>
        <router-link to="visualizer" class="button" >Visualizer</router-link>
    </span>
    <span>
        <router-link to="reports" class="button" >Reports</router-link>
    </span>
    <span v-if="isAdmin">
        <router-link to="admin" class="button" >Admin</router-link>
    </span>
    <div class="cell auto"></div>
</div>


  </div>
</template>


<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component
export default class Menu extends Vue {
    name = "Portal";

    get welcomeMessage() {
        const basemessage = "Open Visualizer to get started.";
        if (this.$store.state.user.name !== "") {
            return "Hi " + this.$store.state.user.name + ". " + basemessage;
        }
        else {
            return basemessage;
        }
    }

    get userName() {
        return this.$store.state.user.name;
    }

    get isAdmin() {
      return this.$store.state.user.isAdmin;
    }

    get isAuthorized() {
      return this.$store.state.user.isAuthorized;
    }
}
</script>

<style scoped>
.button {
    width: 100px;
}

span {
    padding-left: 5px;
    padding-right: 5px;
}
</style>
