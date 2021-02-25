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
		<transition name="scroll-transition">
			<div id="scrolltotop" v-show="show">
				<button v-on:click="topFunction" id="scrollbtn" title="Go to top" class="icon clickable">&#xf102;</button>
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

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import webcrap from "@/assets/ts/webcrap/webcrap";

@Component
export default class ScrollToTop extends Vue {
	debouncedScroll;
	contentarea = document.getElementById("contentPane");
	show = false;

	mounted(): void {
		this.debouncedScroll = webcrap.misc.debounce(this.scrollFunction, 100);
		this.contentarea.addEventListener("scroll", this.debouncedScroll);
	}

	destroyed(): void {
		this.contentarea.removeEventListener("scroll", this.debouncedScroll);
	}

	scrollFunction() {
		if (this.contentarea.scrollTop > 20) {
			this.show = true;
		} else {
			this.show = false;
		}
	}

	// When the user clicks on the button, scroll to the top of the document
	topFunction() {
		this.contentarea.scrollTo({
			top: 0,
			left: 0,
			behavior: "smooth",
		});
	}
}
</script>