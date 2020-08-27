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
	<div>
		<div
			class="title-bar portrait-bar grid-x"
			data-responsive-toggle="full-bar"
			data-hide-for="medium"
			style="display: none;"
		>
			<div class="cell small-2"></div>
			<div class="cell small-8 text-center">
				<router-link :to="routeDefs.portal.path">Birdsnest Explorer</router-link>
			</div>
			<div class="cell small-2">
				<div class="cell auto" />
				<div class="cell shrink">
					<Menu />
				</div>
			</div>
		</div>

		<div class="top-bar landscape-bar" id="full-bar">
			<div class="landscape-bar-left">
				<router-link :to="routeDefs.portal.path">Birdsnest Explorer</router-link>
			</div>
			<div class="landscape-bar-right">
				<Menu />
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import Menu from "@/components/Menu.vue";
import { routeDefs } from "@/router/index";

@Component({
	components: { Menu },
})
export default class TopBar extends Vue {
	routeDefs = routeDefs;
}
</script>

<style>
.menu a {
	padding: 0.1rem;
}

.portrait-bar a,
.portrait-bar a:focus,
.landscape-bar a,
.landscape-bar a:focus {
	color: #fff;
}

.main-bar a:hover {
	color: #ddd;
}

.portrait-bar,
.landscape-bar {
	padding: 0;
}

.portrait-bar,
.landscape-bar,
.landscape-bar ul,
.portrait-bar ul {
	background-color: #606060;
}

.landscape-bar-left {
	font-weight: bold;
	padding: 0.2rem 1rem;
}

.landscape-bar-right .dropdown.menu a {
	padding: 0.2rem 1rem;
}

.landscape-bar-right .dropdown.menu .is-dropdown-submenu a {
	padding: 0.4rem 1rem;
}
</style>