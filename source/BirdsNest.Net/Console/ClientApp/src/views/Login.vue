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
  <div class="login page">
    <LoginCredentials />
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import LoginCredentials from "@/components/LoginCredentials.vue";
import { RawLocation } from "vue-router";
import { routeDefs } from "@/router/index";

@Component({
  components: { LoginCredentials }
})
export default class Login extends Vue {
  created(): void {
    const unwatch = this.$store.watch(
      () => {
        return this.$store.state.user.isAuthorized;
      },
      () => {
        const redirect = {
          path: this.$route.query.redirect || routeDefs.portal.path
        } as RawLocation;
        
        this.$router.replace(redirect);
        unwatch();
      }
    );
  }
}
</script>