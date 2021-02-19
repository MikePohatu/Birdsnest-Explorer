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
			style="display: none"
		>
			<!-- <div class="cell small-2"></div> -->
			<div id="smallcrumbs" class="cell small-9">
				<router-link :to="routeDefs.portal.path">{{ $t("Birdsnest_Explorer") }}</router-link>
				<div>
					<nav aria-label="You are here:" role="navigation">
						<ul class="breadcrumbs">
							<li />
							<li v-for="crumb in breadcrumbs" :key="crumb.name" class="bn-header-crumb">
								<router-link v-if="crumb.link" :to="crumb.link" class="crumb">{{ crumb.name }}</router-link>
								<span v-else class="crumb">{{ crumb.name }}</span>
							</li>
							<li v-for="crumb in pagecrumbs" :key="crumb.link" class="bn-header-crumb">
								<router-link v-if="crumb.link" :to="crumb.link" class="crumb">{{ crumb.name }}</router-link>
								<span v-else class="crumb">{{ crumb.name }}</span>
							</li>
						</ul>
					</nav>
				</div>
			</div>

			<div class="cell auto" />
			<div class="cell shrink">
				<Menu />
			</div>
		</div>

		<div class="top-bar landscape-bar" id="full-bar">
			<div class="landscape-bar-left">
				<nav aria-label="You are here:" role="navigation">
					<ul class="breadcrumbs">
						<li>
							<router-link class="bn-header" :to="routeDefs.portal.path">{{ $t("Birdsnest_Explorer") }}</router-link>
						</li>
						<li v-for="crumb in breadcrumbs" :key="crumb.name" class="bn-header-crumb">
							<router-link v-if="crumb.link" :to="crumb.link" class="crumb">{{ crumb.name }}</router-link>
							<span v-else class="crumb">{{ crumb.name }}</span>
						</li>
						<li v-for="crumb in pagecrumbs" :key="crumb.name" class="bn-header-crumb">
							<router-link v-if="crumb.link" :to="crumb.link" class="crumb">{{ crumb.name }}</router-link>
							<span v-else class="crumb">{{ crumb.name }}</span>
						</li>
					</ul>
				</nav>
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

	get breadcrumbs() {
		return this.$route.meta.breadcrumbs;
	}

	get pagecrumbs() {
		if (Object.prototype.hasOwnProperty.call(this.$route.meta, "pagecrumbs") && this.$route.meta.pagecrumbs !== null) {
			return this.$route.meta.pagecrumbs;
		} else {
			return [];
		}
	}
}
</script>

<style scoped>
#smallcrumbs {
	margin-left: 0.8em;
	margin-top: 0.2em;
	margin-bottom: 0.2em;
}

#smallcrumbs li {
	padding-top: 0;
}

.bn-header {
	font-size: 16px;
	text-transform: none;
}

#smallcrumbs .bn-header {
	font-size: 14px;
}

.crumb {
	color: white;
	font-size: 0.75rem;
	font-weight: 500;
}

.bn-header-crumb {
	padding-top: 0.4em;
}

ul {
	margin-bottom: 0;
}

.breadcrumbs li {
	text-transform: none;
}

.breadcrumbs a:hover {
	text-decoration: none;
}
</style>

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