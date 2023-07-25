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
	<div class="portal page">
		<div class="watermark">
			<img src="/img/icons/logo.svg" height="512px" width="512px" />
		</div>
		<div v-html="bannerHtml" />
		<p>{{ $t('word_Hi') }}{{ gn	 }}. {{ $t('portal.welcome') }}</p>

		<div class="grid-x grid-margin-x" id="portalBoxes">
			<VisualizerPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
			<ReportsPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
			<ServerInfoPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
		</div>

		<p class="text-center">
			{{ $t('portal.usage_info_1') }}
			<router-link v-if="!isIE" :to="routeDefs.docs.path">{{ $t('word_documentation') }}</router-link>
			<a
				v-else
				href="https://github.com/MikePohatu/Birdsnest-Explorer"
				target="_blank"
			>{{ $t('phrase_source_repository') }}</a>
			. {{ $t('portal.usage_info_2') }}
			<a
				href="https://support.20road.com"
				target="_blank"
			>{{ $t('twentyroad') }} {{ $t('portal.usage_info_3') }}</a>
		</p>

		<p class="text-center">
			{{ $t('portal.license_attribution') }}
			<router-link to="about">{{ $t('word_About') }}</router-link>
			{{ $t('word_page') }}
		</p>
		<div v-html="footerHtml" />
	</div>
</template>

<style scoped>
.portal {
	max-width: 1024px;
	margin-left: auto;
	margin-right: auto;
}

.button {
	width: 100px;
}

.watermark {
	position: fixed;
	top: 0px;
	right: 0px;
	opacity: 0.03;
	z-index: -10;
}

span {
	padding-left: 5px;
	padding-right: 5px;
}

#portalBoxes {
	margin-top: 1.5rem;
	margin-bottom: 1.5rem;
}

.portalBoxWrapper {
	text-align: center;
	margin-left: 30px;
	margin-right: 30px;
	margin-bottom: 20px;
	width: 250px;
}
:deep(.portalBoxHeading) {
	margin-bottom: 0.8rem;
}

:deep(td) {
	width: min-content;
	font-size: 0.8em;
	padding: 0.625em 0.625em;
}

:deep(.left) {
	text-align: left;
}

:deep(.right) {
	text-align: right;
}

:deep(.portalBoxWrapper .description) {
	margin-top: 0.8rem;
	margin-bottom: 1.5rem;
	font-size: 0.8em;
}

:deep(.portalBox) {
	border-radius: 7px;
	-moz-border-radius: 7px;
	border-width: 2px;
	border-color: lightgray;
	border-style: solid;
	background-color: rgba(235, 235, 235, 0.281);
	padding: 3px;
	height: 195px;
	color: black;
}

:deep(.portalBox table) {
	border-collapse: separate;
	margin-bottom: 0;
	margin-left: auto;
	margin-right: auto;
}

:deep(.portalBox table tr:last-child td:first-child) {
	-moz-border-radius: 0 0 0 4px;
	border-radius: 0 0 0 4px;
}

:deep(.portalBox table tr:last-child td:last-child) {
	-moz-border-radius: 0 0 4px 0;
	border-radius: 0 0 4px 0;
}

:deep(.portalBox table tr:first-child td:first-child) {
	-moz-border-radius: 4px 0 0 0;
	border-radius: 4px 0 0 0;
}

:deep(.portalBox table tr:first-child td:last-child) {
	-moz-border-radius: 0 4px 0 0;
	border-radius: 0 4px 0 0;
}
</style>


<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "@/store";
import VisualizerPortalBlock from "@/components/portal/VisualizerPortalBlock.vue";
import ReportsPortalBlock from "@/components/portal/ReportsPortalBlock.vue";
import ServerInfoPortalBlock from "@/components/portal/ServerInfoPortalBlock.vue";
import { routeDefs } from "@/router/index";
import webcrap from "@/assets/ts/webcrap/webcrap";


const isIE = webcrap.misc.isIE();
const store = useStore();

const gn = computed(() => {
	if (store.state.user.gn !== "") {
		return " " + store.state.user.gn;
	} else {
		return "";
	}
});

const bannerHtml = computed((): string => {
	return store.state.customization.portal.banner;
});

const footerHtml = computed((): string => {
	return store.state.customization.portal.footer;
});
</script>