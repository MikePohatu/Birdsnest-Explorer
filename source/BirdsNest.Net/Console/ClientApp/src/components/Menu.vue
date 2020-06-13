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
	<div>
		<ul class="dropdown menu align-right" data-dropdown-menu data-click-open="true">
			<li>
				<a href="#">
					<img id="menuicon" src="/img/icons/logo.svg" height="24px" width="24px" />
				</a>
				<ul class="main-menu">
					<li>
						<div v-if="isAuthorized">
							<router-link :to="routeDefs.portal.path">Home</router-link>
							<router-link v-if="isAdmin" :to="routeDefs.admin.path">Admin</router-link>
							<router-link :to="routeDefs.reports.path">Reports</router-link>
							<router-link :to="routeDefs.visualizer.path">Visualizer</router-link>
							<router-link :to="routeDefs.about">About</router-link>
							<a v-on:click="logout">Logout</a>
						</div>
						<div v-else>
							<router-link :to="routeDefs.about">About</router-link>
							<router-link :to="routeDefs.login.path">Login</router-link>
						</div>
					</li>
				</ul>
			</li>
		</ul>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { auth } from "../assets/ts/webcrap/authcrap";
import { routeDefs } from "@/router/index";

@Component
export default class Menu extends Vue {
	routeDefs = routeDefs;

	get isAdmin() {
		return this.$store.state.user.isAdmin;
	}
	get isAuthorized() {
		return this.$store.state.user.isAuthorized;
	}
	logout() {
		auth.logout(() => {
			this.$router.push(routeDefs.login.path);
		});
	}
}
</script>

<style scoped>
.main-menu {
	z-index: 500;
}

.main-menu .menu-text {
	padding: 0.2rem 1rem;
}

.dropdown.menu > li.is-dropdown-submenu-parent > a::after {
	border-color: #53ff67 transparent transparent;
}

#menuicon {
	min-width: 24px;
	min-height: 24px;
}
</style>
