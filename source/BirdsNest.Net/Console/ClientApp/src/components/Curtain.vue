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
	<!-- no curtain on allowanonymous pages please. -->
	<div v-if="authorizationRequired">
		<div id="curtain" />
		<div id="curtainoverlay">
      <!-- Avoid conflicts with ids. -->
			<LoginCredentials v-if="!isPageAuthorized" class="xy-center" />
		</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import LoginCredentials from "@/components/LoginCredentials.vue";
import $ from "jquery";
import { routeDefs } from "@/router/index";

@Component({
	components: { LoginCredentials },
})
export default class Curtain extends Vue {
	get authorizationRequired(): boolean {
		if (this.$route.meta && this.$route.meta.allowAnonymous) {
			return false;
		} else {
			return true;
		}
	}

	get notLoginPage(): boolean {
		return this.$route.path !== routeDefs.login.path;
	}

	get isPageAuthorized(): boolean {
		if (this.$route.meta.allowAnonymous === true) {
			return true;
		} else {
			return this.$store.state.user.isAuthorized;
		}
	}

	created(): void {
		this.$store.watch(
			() => {
				return this.$store.state.user.isAuthorized;
			},
			() => {
        //add a setTimeout in case the change is due to a 'softPing', in which case
        //a route change might be coming. The setTimeout lets the route change happen
        //first so this can be evaluated correctly
				setTimeout(() => {
					if (this.isPageAuthorized) {
						this.hideCurtain();
					} else {
						this.showCurtain();
					}
				}, 500);
			}
		);
	}

	showCurtain() {
		if ($("#curtain").css("display") === "none") {
			$("#curtain").slideToggle();
			$("#curtainoverlay").slideToggle();
		}
	}

	hideCurtain() {
		if ($("#curtain").css("display") !== "none") {
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