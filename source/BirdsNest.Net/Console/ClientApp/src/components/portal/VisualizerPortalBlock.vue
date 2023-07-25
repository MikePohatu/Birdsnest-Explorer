<!-- Copyright (c) 2019-2023 "20Road"
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
		<div class="portalBoxHeading">
			<router-link to="visualizer">{{ $t('word_Visualizer') }}</router-link>
		</div>
		<div>
			<router-link to="visualizer">
				<div class="portalBox">
					<VisualizerPortalScreeny />
				</div>
			</router-link>
		</div>
		<div
			class="description"
		>{{ $t('portal.visualizer.info') }}</div>
	</div>
</template>

<script setup lang="ts">
import VisualizerPortalScreeny from "@/components/portal/VisualizerPortalScreeny.vue";

</script>