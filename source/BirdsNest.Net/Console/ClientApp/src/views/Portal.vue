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
	<div class="portal page">
		<div class="watermark">
			<img src="/img/icons/logo.svg" height="512px" width="512px" />
		</div>
		<p>{{ welcomeMessage }} Welcome to BirdsNest</p>
		
		<div class="grid-x grid-margin-x" id="portalBoxes">
			<VisualizerPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
			<ReportsPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
			<ServerInfoPortalBlock class="cell large-4 medium-6 portalBoxWrapper" />
		</div>

		<p>Usage information and links to how-tos videos are available from the BirdsNest online <a href="https://github.com/MikePohatu/BirdsNest" target="_blank">documentation</a>. For support, you can contact us via the <a href="https://www.20road.com/support" target="_blank">20Road support page.</a></p>
		
		<p>For license and attribution information please see the <router-link to="about">About</router-link> page</p>
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
	margin-top: 2rem;
	margin-bottom: 3rem;
}

.portalBoxWrapper {
	text-align: center;
	margin-left: 30px;
	margin-right: 30px;
	margin-bottom: 20px;
	width: 250px;
}

/deep/ .portalBoxWrapper .description {
	margin-top: 5px;
	font-size: 0.8rem;
}

/deep/ .portalBox {
	border-radius: 7px;
	-moz-border-radius: 7px;
	border-width: 2px;
	border-color: lightgray;
	border-style: solid;
	background-color: rgba(235, 235, 235, 0.281);
}

/deep/ table.portalBox {
	border-collapse: separate;
	margin-bottom: 0;
	margin-left: auto;
	margin-right: auto;
	color: black;
}

/deep/ table.portalBox tr:last-child td:first-child {
	-moz-border-radius: 0 0 0 4px;
	border-radius: 0 0 0 4px;
}

/deep/ table.portalBox tr:last-child td:last-child {
	-moz-border-radius: 0 0 4px 0;
	border-radius: 0 0 4px 0;
}

/deep/ table.portalBox tr:first-child td:first-child {
	-moz-border-radius: 4px 0 0 0;
	border-radius: 4px 0 0 0;
}

/deep/ table.portalBox tr:first-child td:last-child {
	-moz-border-radius: 0 4px 0 0;
	border-radius: 0 4px 0 0;
}
</style>


<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import VisualizerPortalBlock from "@/components/portal/VisualizerPortalBlock.vue";
import ReportsPortalBlock from "@/components/portal/ReportsPortalBlock.vue";
import ServerInfoPortalBlock from "@/components/portal/ServerInfoPortalBlock.vue";

@Component({
	components: { VisualizerPortalBlock, ReportsPortalBlock, ServerInfoPortalBlock },
})
export default class Portal extends Vue {
	name = "Portal";

	get welcomeMessage() {
		if (this.$store.state.user.name !== "") {
			return "Hi " + this.$store.state.user.name + ". ";
		} else {
			return "";
		}
	}

	get userName() {
		return this.$store.state.user.name;
	}
}
</script>
