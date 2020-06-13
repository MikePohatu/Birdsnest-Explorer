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
    <div class="xy-center grid-container">
        <div class="grid-x">
            <div class="cell auto" />
            <div class="cell shrink">
                <img class="spinner" src="/img/icons/logo.svg" height="64px" width="64px" />
            </div>
            <div class="cell auto" />
        </div>
        <div class="grid-x" style="width: 100px">
            <p class="cell loading">Loading</p>
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

.loading::after {
  content: '.';
  animation: dots 4s steps(5, end) infinite;
}

@keyframes dots {
  0%, 20% {
    color: rgba(0,0,0,0);
    text-shadow:
      .2em 0 0 rgba(0,0,0,0),
      .4em 0 0 rgba(0,0,0,0);
      }
  40% {
    color: #606060;
    text-shadow:
      .2em 0 0 rgba(0,0,0,0),
      .4em 0 0 rgba(0,0,0,0);}
  60% {
    text-shadow:
      .2em 0 0 #606060,
      .4em 0 0 rgba(0,0,0,0);}
  80%, 100% {
    text-shadow:
      .2em 0 0 #606060,
      .4em 0 0 #606060;}
}
</style>