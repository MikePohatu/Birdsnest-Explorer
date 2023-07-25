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
	<div id="docowrapper" class="page">
		<div v-if="markdown !== defaultmd" class="page" v-html="markdown" />

		<ScrollToTop />
	</div>
</template>

<style scoped>
@media print, screen and (min-width: 40em) {
	:deep(h1, .h1) {
		font-size: 2rem;
	}

	:deep(h2, .h2) {
		font-size: 1.7rem;
	}

	:deep(h3, .h3) {
		font-size: 1.3rem;
	}
}
</style>

<script setup lang="ts">
import { onBeforeRouteLeave, useRoute, useRouter } from "vue-router";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { bus, events } from "@/bus";
import ScrollToTop from "@/components/ScrollToTop.vue";
import { onBeforeMount, onUpdated, watch, ref } from "vue";
import { useI18n } from 'vue-i18n';
import MarkdownIt from 'markdown-it';

const { t } = useI18n();
const md = new MarkdownIt();

const router = useRouter();
const route = useRoute();

let defaultmd = ref("Loading");
let markdown = ref(defaultmd.value);

onBeforeMount((): void => {
	updateMarkdown();
	moveToAnchor();
});

onUpdated(()=> {
	updateLinks();
});

onBeforeRouteLeave(()=>{
	unwatch();
})

const unwatch = watch(
	()=>route.path,
	()=>{
		updateMarkdown();	
	}
);

function updateMarkdown(): void {
	bus.emit(events.Notifications.Processing);
	markdown.value = defaultmd.value;
	const docurl = route.path.replace("/docs/", "/");

	const request: Request = {
		url: docurl,
		postJson: false,
		successCallback: (data?: string) => {
			markdown.value = md.render(data);
			bus.emit(events.Notifications.Clear);
		},
		errorCallback: (jqXHR?, status?: string, error?: string) => {
			// eslint-disable-next-line
			console.error(error);
			bus.emit(events.Notifications.Error, t("word_Error") + ": " + error);
		},
	};

	api.get(request);
}

function updateLinks(): void {
	//update the links with corrections for birdsnest setup
	const docowrapper = document.querySelector("#docowrapper");

	const links: Array<HTMLAnchorElement> = Array.from(docowrapper.querySelectorAll("a"));
	links.forEach((link) => {
		if (link.href.startsWith(location.origin + "/documentation")) {
			const newpath = link.href.replace(location.origin + "/documentation", "/docs/static/documentation");
			link.addEventListener("click", (e) => {
				e.preventDefault();
				router.push({ path: newpath });
			});
			link.href = newpath;
		} else if (link.href.startsWith(location.origin) === false) {
			link.target = "_blank";
		}
	});

	const imgs: Array<HTMLImageElement> = Array.from(docowrapper.querySelectorAll("img"));
	imgs.forEach((img) => {
		if (img.src.startsWith(location.origin + "/documentation")) {
			img.src = img.src.replace("/documentation", "/static/documentation");
		}
	});

	const headings: Array<HTMLHeadingElement> = Array.from(docowrapper.querySelectorAll("h1, h2, h3, h4, h5, h6"));
	headings.forEach((h) => {
		const id = h.innerText.replace(/ /g, "-").replace(/\./g, "").toLowerCase();
		h.id = id;
	});
}

//Separate movetoanchor function is required because the links won't be created at page load. 
//This is requried to run separately after content has finished loading
function moveToAnchor(): void {
	if (route.hash) {
		setTimeout(() => { //setTimeout so this will run after page has loaded and created id
			const pathsplit = route.hash.split("#");
			if (pathsplit.length > 1) {
				const id = document.getElementById(pathsplit[1]);
				if (id !== null) { id.scrollIntoView({ behavior: "smooth" }); } //check in case there is an invalid link
			}
		}, 500);
	}
}
</script>