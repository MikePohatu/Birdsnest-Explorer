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
  <!-- no curtain on login page please. Also avoid conflicts with ids. -->
  <div v-if="notLoginPage" >
    <div id="curtain" />
    <div id="curtainoverlay">
      <LoginCredentials v-if="!isAuthorized" class="xy-center" />
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import LoginCredentials from "@/components/LoginCredentials.vue";
import $ from "jquery";
import { routeDefs } from "@/router/index";

@Component({
  components: { LoginCredentials }
})
export default class Curtain extends Vue {

  get notLoginPage(): boolean {
    return this.$route.path !== routeDefs.login.path;
  }

  get isAuthorized(): boolean {
    return this.$store.state.user.isAuthorized;
  }

  created(): void {
    this.$store.watch(
      () => {
        return this.$store.state.user.isAuthorized;
      },
      (newval) => {
        if (this.$route.path !== routeDefs.login.path) {
          if (!newval) { this.showCurtain(); }
          else { this.hideCurtain(); }
        }
      }
    );
  }

  showCurtain() {
    //console.log("showCurtain: " + $('#curtain').css('display'));
    if ($("#curtain").css("display") === "none") {
      //console.log("toggle");
      $("#curtain").slideToggle();
      $("#curtainoverlay").slideToggle();
    }
  }

  hideCurtain() {
    //console.log("hideCurtain: " + $('#curtain').css('display'));

    if ($("#curtain").css("display") !== "none") {
      //console.log("toggle");
      $("#curtain").slideToggle();
      $("#curtainoverlay").slideToggle();
    }
  }
}
</script>

<style scoped>
#curtain {
  position: fixed;
  background-color: lightgray;
  opacity: 0.8;
  top: 0;
  bottom: 0;
  right: 0;
  left: 0;
  display: none;
  z-index: 4999 !important;
}

#curtainoverlay {
  position: fixed;
  opacity: 1;
  top: 0;
  bottom: 0;
  right: 0;
  left: 0;
  display: none;
  z-index: 5000 !important;
}
</style>