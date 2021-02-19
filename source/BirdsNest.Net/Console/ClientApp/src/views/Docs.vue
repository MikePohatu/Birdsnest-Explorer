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
	<div id="docowrapper" class="page">
		<div v-if="markdown !== defaultmd" class="page">
			<vue-markdown :source="markdown" />
		</div>

		<ScrollToTop />
	</div>
</template>

<style scoped>

@media print, screen and (min-width: 40em) {
	/deep/ h1, .h1 {
		font-size: 2rem;
	}

	/deep/ h2, .h2 {
		font-size: 1.7rem;
	}

	/deep/ h3, .h3 {
		font-size: 1.3rem;
	}
}
</style>

<script lang="ts">
import { Component, Vue, Watch } from "vue-property-decorator";
import { Route } from "vue-router";
import VueMarkdown from "@adapttive/vue-markdown";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { bus, events } from "@/bus";
import i18n from "@/i18n";
import ScrollToTop from "@/components/ScrollToTop.vue";

@Component({
	components: { VueMarkdown, ScrollToTop }
})
export default class Docs extends Vue {
	defaultmd = "Loading";
	markdown = this.defaultmd;

	updateMarkdown(route: Route): void {
		bus.$emit(events.Notifications.Processing);
		this.markdown = this.defaultmd;
		const docurl = route.path.replace("/docs/", "/");

		const request: Request = {
			url: docurl,
			postJson: false,
			successCallback: (data?: string) => {
				this.markdown = data;
				bus.$emit(events.Notifications.Clear);
			},
			errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
				// eslint-disable-next-line
				console.error(error);

				bus.$emit(events.Notifications.Error, i18n.t("word_Error") + ": " + error);
			},
		};

		api.get(request);
	}

	updateLinks(): void {
		//update the links with corrections for birdsnest setup
		const docowrapper = document.querySelector("#docowrapper");
		const links: NodeListOf<HTMLAnchorElement> = docowrapper.querySelectorAll("a");
		links.forEach((link) => {
			if (link.href.startsWith(location.origin + "/documentation")) {
				const newpath = link.href.replace(location.origin + "/documentation", "/docs/static/documentation");
				link.addEventListener("click", (e) => {
					e.preventDefault();
					this.$router.push({ path: newpath });
				});
				link.href = newpath;
			} else if (link.href.startsWith(location.origin) === false) {
				link.target = "_blank";
			}
		});

		const imgs: NodeListOf<HTMLImageElement> = docowrapper.querySelectorAll("img");
		imgs.forEach((img) => {
			if (img.src.startsWith(location.origin + "/documentation")) {
				img.src = img.src.replace("/documentation", "/static/documentation");
			}
		});

		const headings: NodeListOf<HTMLHeadingElement> = docowrapper.querySelectorAll("h1, h2, h3, h4, h5, h6");
		headings.forEach((h) => {
			const id = h.innerText.replace(/ /g, "-").replace(/\./g, "").toLowerCase();

			if (h.previousElementSibling === null || h.previousElementSibling.id !== id) {
				const anchor: HTMLAnchorElement = document.createElement("a");
				anchor.id = id;
				h.parentElement.insertBefore(anchor, h);
			}
		});
	}

	beforeMount(): void {
		this.updateMarkdown(this.$route);
	}

	updated(): void {
		this.updateLinks();
	}

	@Watch("$route", { immediate: true, deep: true })
	onUrlChange(to: Route, from: Route) {
		const topathsplit = to.path.split("#");

		if (from) {
			const frompathsplit = from.path.split("#");

			if (topathsplit[0] !== frompathsplit[0]) {
				this.updateMarkdown(to);
			}
		} else {
			this.updateMarkdown(to);
		}
	}
}
</script>