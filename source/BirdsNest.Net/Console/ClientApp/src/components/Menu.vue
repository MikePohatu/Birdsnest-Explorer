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
		<ul
			class="dropdown menu align-right"
			data-dropdown-menu
			data-click-open="true"
			data-disable-hover="true"
		>
			<li>
				<a href="#">
					<img id="menuicon" src="/img/icons/logo.svg" height="24px" width="24px" />
				</a>
				<ul class="main-menu">
					<li>
						<div v-if="isAuthorized" class="menuwrapper">
							<router-link :to="routeDefs.portal.path">{{ $t('word_Home') }}</router-link>
							<router-link v-if="isAdmin" :to="routeDefs.admin.path">{{ $t('word_Admin') }}</router-link>
							<router-link :to="routeDefs.reports.path">{{ $t('word_Reports') }}</router-link>
							<router-link :to="routeDefs.info.path">{{ $t('phrase_Server_Information') }}</router-link>
							<router-link :to="routeDefs.visualizer.path">{{ $t('word_Visualizer') }}</router-link>
							<hr />
							<router-link :to="routeDefs.about.path">{{ $t('word_About') }}</router-link>
							<router-link v-if="!isIE" :to="routeDefs.docs.path">{{ $t('word_Docs') }}</router-link>
							<!-- <LangMenu /> -->
							<a v-on:click="logout">{{ $t('word_Logout') }}</a>
						</div>
						<div v-else class="menuwrapper">
							<router-link :to="routeDefs.about.path">{{ $t('word_About') }}</router-link>
							<router-link v-if="!isIE" :to="routeDefs.docs.path">{{ $t('word_Docs') }}</router-link>
							<!-- <LangMenu /> -->
							<router-link :to="routeDefs.login.path">{{ $t('word_Login') }}</router-link>
						</div>
					</li>
				</ul>
			</li>
		</ul>
	</div>
</template>

<script setup lang="ts">
import { auth } from "@/assets/ts/webcrap/authcrap";
import { routeDefs } from "@/router/index";
//import LangMenu from "@/components/LangMenu.vue";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import { computed } from "vue";

// @Component({
// 	components: { LangMenu },
// })
const store = useStore();
const router = useRouter();

let isIE = webcrap.misc.isIE();

const isAdmin = computed(() => {
	return store.state.user.isAdmin;
});

const isAuthorized = computed(() => {
	return store.state.user.isAuthorized;
});

function logout() {
	auth.logout(() => {
		router.push(routeDefs.login.path);
	});
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

.menuwrapper {
	width: 100%;
}

hr {
	margin: 1px 1rem;
}
</style>
