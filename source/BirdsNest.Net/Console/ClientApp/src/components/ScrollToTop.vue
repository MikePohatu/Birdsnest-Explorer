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
		<transition name="scroll-transition">
			<div id="scrolltotop" v-show="show">
				<button
					v-on:click="topFunction"
					id="scrollbtn"
					title="Go to top"
					class="icon clickable"
				>&#xf102;</button>
			</div>
		</transition>
	</div>
</template>

<style scoped>
#scrolltotop {
	border: 3px solid rgb(224, 224, 224);
	border-radius: 18px;
	position: fixed;
	bottom: 15px;
	right: 30px;
	background-color: none;
}

#scrolltotop:hover {
	border-color: rgb(204, 204, 204);
}

#scrollbtn {
	z-index: 99;
	outline: none;
	color: rgb(204, 204, 204);
	padding: 7px 7px 8px 7px;
	font-size: 22px;
}

#scrollbtn:hover {
	color: rgb(194, 194, 194);
}

@media print, screen and (max-width: 800px) {
	#scrolltotop {
		border-radius: 18px;
		bottom: 5px;
		right: 10px;
	}

	#scrollbtn {
		padding: 7px 7px 8px 7px;
		font-size: 22px;
	}
}

.scroll-transition-enter-active,
.scroll-transition-leave-active {
	transition: all 0.3s ease-in-out;
}

.scroll-transition-enter,
.scroll-transition-leave-to {
	transform: translateY(200px);
}
</style>

<script setup lang="ts">
import webcrap from "@/assets/ts/webcrap/webcrap";
import { onMounted, onUnmounted, ref } from "vue";

let debouncedScroll;
let contentarea = document.getElementById("contentPane");
let show = ref(false);

onMounted((): void => {
	debouncedScroll = webcrap.misc.debounce(scrollFunction, 100);
	contentarea.addEventListener("scroll", debouncedScroll);
});

onUnmounted((): void => {
	contentarea.removeEventListener("scroll", debouncedScroll);
});

function scrollFunction() {
	if (contentarea.scrollTop > 20) {
		show.value = true;
	} else {
		show.value = false;
	}
}

// When the user clicks on the button, scroll to the top of the document
function topFunction() {
	contentarea.scrollTo({
		top: 0,
		left: 0,
		behavior: "smooth",
	});
}

</script>