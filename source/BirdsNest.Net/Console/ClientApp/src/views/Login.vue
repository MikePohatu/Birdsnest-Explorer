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
    <div v-html="bannerHtml" />
    <LoginCredentials />
    <div v-html="footerHtml" />
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { computed, defineComponent, watch } from "vue";
import LoginCredentials from "@/components/LoginCredentials.vue";
import { routeDefs } from "@/router/index";
import { RouteLocationRaw, RouteRecordRaw, useRoute, useRouter } from "vue-router";

const store = useStore();
const route = useRoute();
const router = useRouter();

const bannerHtml = computed((): string => {
  return store.state.customization.login.banner;
});

const footerHtml = computed((): string => {
  return store.state.customization.login.footer;
});

const unwatch = store.watch(
  () => {
    return store.state.user.isAuthorized;
  },
  () => {
    const redirect: RouteLocationRaw = {
      path: route.query.redirect as string || routeDefs.portal.path
    };

    router.replace(redirect);
    unwatch();
  }
);

</script>