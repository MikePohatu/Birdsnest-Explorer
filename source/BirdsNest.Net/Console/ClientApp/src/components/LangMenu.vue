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
		<ul class="dropdown menu" data-dropdown-menu data-click-open="true" data-disable-hover="true">
			<li>
				<a href="#" id="topFlagIcon" >
					{{ $t('word_Language')}} <flag :iso="currentLang.flag" v-bind:squared="false"/>
				</a>
				<ul>
					<li v-for="(lang, name) in languages" :key="name">
						<a href="#" @click="changeLocale(name)">
							<flag :iso="lang.flag" v-bind:squared="false" />
							{{lang.title}}
						</a>
					</li>
				</ul>
			</li>
		</ul>
	</div>
</template>

<style scoped>
#topFlagIcon .flag-icon {
    height: 0.85em;
    vertical-align: middle;
}

.dropdown.menu > li.is-dropdown-submenu-parent > a {
    padding-right: 0.5rem;
}

.dropdown.menu > li.is-active > a {
    color: #ccc;
}

.dropdown.menu > li.is-dropdown-submenu-parent > a::after {
    display: none;
}
</style>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { LanguageSelector, rootPaths } from "@/store";
import { Dictionary } from 'vue-router/types/router';

@Component
export default class LangMenu extends Vue {
	get currentLang(): LanguageSelector {
		return this.languages[this.$i18n.locale];
	}

	get languages(): Dictionary<LanguageSelector> {
		return this.$store.state.languages;
	}

	changeLocale(locale: string) {
		this.$store.commit(rootPaths.mutations.LOCALE, locale);
	}
}
</script>