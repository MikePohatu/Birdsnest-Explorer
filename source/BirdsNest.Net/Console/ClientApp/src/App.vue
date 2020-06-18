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
	<div id="app" class="fillAvailable">
		<Curtain />
		<TopBar />
		<div id="contentPane" class="fillAvailable">
			<router-view />
		</div>
		<NotificationIcon />
	</div>
</template>


<script lang="ts">
import TopBar from "@/components/TopBar.vue";
import Curtain from "@/components/Curtain.vue";
import { Component, Vue } from "vue-property-decorator";
import { rootPaths } from "./store";
import NotificationIcon from "@/components/NotificationIcon.vue";

@Component({
	components: {
		TopBar,
		Curtain,
		NotificationIcon,
	},
})
export default class App extends Vue {
	created() {
		this.$store.dispatch(rootPaths.actions.UPDATE_PROVIDERS);
	}
}
</script>

<style lang="scss">
body {
	position: relative;
}

#contentPane {
	position: fixed;
	overflow: auto;
}

.fillAvailable,
body,
html {
	margin: 0;
	padding: 0;
	height: 100%;
	width: 100%;
}

#app {
	-webkit-font-smoothing: antialiased;
	-moz-osx-font-smoothing: grayscale;
}

.page {
	padding-top: 10px;
	padding-bottom: 10px;
	padding-right: 20px;
	padding-left: 20px;
}

.clickable:hover {
	cursor: pointer;
}

.has-tip {
	border-bottom: none;
	cursor: auto;
}

.hidden {
	display: none;
}

.xy-center {
	position: absolute;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
}

.absolute-top-left {
	position: absolute;
	top: 0;
	left: 0;
}

.menu {
	z-index: 500;
}

.scrollable {
	overflow: auto;
}

.scrollable::-webkit-scrollbar {
	width: 10px;
	background: transparent;
}

.scrollable::-webkit-scrollbar-thumb {
	background: #d6d6d6;
}

.spinner {
	-webkit-animation: spin 4s linear infinite;
	-moz-animation: spin 4s linear infinite;
	animation: spin 4s linear infinite;
}

@-moz-keyframes spin {
	100% {
		-moz-transform: rotate(360deg);
	}
}

@-webkit-keyframes spin {
	100% {
		-webkit-transform: rotate(360deg);
	}
}

@keyframes spin {
	100% {
		-webkit-transform: rotate(360deg);
		transform: rotate(360deg);
		-ms-transform-origin: 50%;
	}
}

.dialog {
	padding: 5px 15px;
	background: rgba(255, 255, 255, 1);
	opacity: 1;	
}

.dialogWrapper {
	padding: 5px;
	background-color: rgba(220, 220, 220, 0.8);
  z-index: 4000;
  position: fixed;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
}

@media all and (max-width: 767px) {
	.dialogWrapper {
		position: fixed;
		top: 50%;
    left: 0;
    right: 0;
    transform: translate(0, -50%);
	}
}



.icon {
	font-family: "Font Awesome 5 Free";
	font-weight: 900;
	text-anchor: middle;
	stroke-width: 0;
	line-height: 1;
}

.noselect {
	-webkit-touch-callout: none;
	-webkit-user-select: none;
	-khtml-user-select: none;
	-moz-user-select: none;
	-ms-user-select: none;
	user-select: none; 
}
</style>