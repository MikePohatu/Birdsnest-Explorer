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
    <div class="xy-center grid-container">
        <div class="grid-x">
            <div class="cell auto" />
            <div class="cell shrink">
                <img class="spinner" src="/img/icons/logo.svg" height="48px" width="48px" />
            </div>
            <div class="cell auto" />
        </div>
        <div class="grid-x" style="width: 100px">
            <p class="cell loading">{{ $t('word_Loading') }}</p>
        </div>
    </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component
export default class Loading extends Vue { }

</script>

<style scoped>
p {
  color: #606060;
  font-weight: bold;
  font: 300 2em/150% Impact;
  text-align: center;
}
</style>