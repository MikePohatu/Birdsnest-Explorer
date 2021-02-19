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
		this.contentarea.scrollTop = 0; // For Safari
	}
}
</script>