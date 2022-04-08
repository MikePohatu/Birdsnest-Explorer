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
						<a href="#" @click="changeLocale(name as string)">
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

<script setup lang="ts">
	import { LanguageSelector, rootPaths, useStore } from "@/store";
	import { Dictionary } from "@/assets/ts/webcrap/misccrap";
	import { computed, defineComponent } from "vue";
	import { useI18n } from "vue-i18n";

	const i18n = useI18n();
	const store = useStore();

	const currentLang = computed((): LanguageSelector => {
		return languages[i18n.locale.value];
	})

	const languages = computed((): Dictionary<LanguageSelector> => {
		return store.state.languages;
	})

	function changeLocale(locale: string) {
		store.commit(rootPaths.mutations.LOCALE, locale);
	}

</script>