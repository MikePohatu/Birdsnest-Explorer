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
	<div class="grid-x grid-margin-x">
		<div class="cell auto"></div>
		<div class="content">
			<h4>{{ $t('word_About') }}</h4>
			<p>
				{{ $t('Birdsnest_Explorer') }} {{ $t('about.is_created_and_maintained_by') }}
				<a href="https://20road.com" target="_blank">{{ $t('20road') }}</a>.
			</p>

			<h5>{{ $t('word_Documentation') }}</h5>
			<p class="justify">
				<router-link v-if="!isIE" to="/docs">{{ $t('word_Documentation') }}</router-link><span v-else style="padding: 0px;">{{ $t('word_Documentation') }}</span> {{ $t('about.documentation_available_from') }} {{ $t('Birdsnest_Explorer') }}
				<a
					href="https://github.com/MikePohatu/Birdsnest-Explorer"
					target="_blank"
				>{{ $t('phrase_source_repository') }}</a>. {{ $t('about.version_documentation_branch') }}
				<b>{{ $t('word_Branch') }}</b> {{ $t('about.dropdown_list') }}
			</p>

			<h5>{{ $t('word_License') }}</h5>
			<p class="justify">
				{{ $t('Birdsnest_Explorer') }} {{ $t('about.is_open_source_software')}} {{ $t('Birdsnest_Explorer') }}
				<a
					href="https://github.com/MikePohatu/Birdsnest-Explorer"
					target="_blank"
				>{{ $t('phrase_source_repository') }}</a>.
			</p>

			<h5>{{ $t('word_Attribution') }}</h5>
			<p
				class="justify"
			>{{ $t('Birdsnest_Explorer') }} {{ $t('about.is_built_upon') }}</p>
			<p>
				.Net Core -
				<a href="https://github.com/dotnet/core" target="_blank">Microsoft</a>
				<br />D3 -
				<a href="https://d3js.org/" target="_blank">d3js.org</a>
				<br />CSharpVitamins -
				<a href="https://github.com/csharpvitamins/CSharpVitamins.ShortGuid" target="_blank">CSharpVitamins</a>
				<br />FontAwesome -
				<a href="https://fontawesome.com/" target="_blank">fontawesome.com</a>
				<br />Foundation 6 -
				<a href="https://get.foundation/index.html" target="_blank">Zurb</a>
				<br />JQuery -
				<a href="https://jquery.com/" target="_blank">jquery.com</a>
				<br />Json.Net -
				<a href="https://www.newtonsoft.com/" target="_blank">NewtonSoft</a>
				<br />neo4j -
				<a href="https://neo4j.com/" target="_blank">neo4j.com</a>
				<br />NLog -
				<a href="https://nlog-project.org/" target="_blank">nlog-project.org</a>
				<br />Novell.Directory.Ldap.NETStandard -
				<a
					href="https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard"
					target="_blank"
				>dsbenghe</a>
				<br />uuid js -
				<a href="https://github.com/uuidjs/uuid" target="_blank">uuidjs</a>
				<br />Vue -
				<a href="https://vuejs.org/" target="_blank">vuejs.org</a>
				<br />vue-cookies - 
				<a href="https://github.com/cmp-cc/vue-cookies" target="_blank">cmp-cc</a>
			</p>
		</div>
		<div class="cell auto"></div>
	</div>
</template>

<style scoped>
p {
	font-size: 0.9rem;
}

.content {
	margin: 2rem;
	max-width: 600px;
	text-align: center;
}

.justify {
	text-align: justify;
}
</style>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import webcrap from "@/assets/ts/webcrap/webcrap";

@Component
export default class About extends Vue {
	isIE = webcrap.misc.isIE();
}
</script>