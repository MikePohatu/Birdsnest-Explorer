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
	<div
		id="graphNotification"
		:class="{ disabled: isHidden }"
		:title="tooltipmessage"
		v-on:click="onNotificationClicked()"
	>
		<div style="position: relative">
			<svg viewBox="-32 -32 64 64" width="64" height="64" :class="{ spinner: processing }">
				<g>
					<circle id="notificationCircle" r="24" :class="stateClass" />
					<path v-show="processing" id="spinnerIcon" d="M 0 -18 a 18 18 0 1 1 -12.78 5.22" />
				</g>
			</svg>
			<div id="notificationIcon">
				<span v-html="icon" font-size class="icon noselect"></span>
			</div>
		</div>
	</div>
</template>



<style scoped>
.disabled {
	display: none;
}

#graphNotification {
	height: 64px;
	width: 64px;
	position: fixed;
	right: 0;
	bottom: 0;
	z-index: 5000;
	padding: 0;
	margin: 10px;
}

#notificationIcon {
	position: absolute;
	top: 20px;
	left: 22px;
	text-align: center;
	height: 20px;
	width: 20px;
	color: white;
}

#spinnerIcon {
	stroke-linecap: round;
	stroke: white;
	stroke-width: 4;
	fill: none;
}

.state0 {
	fill: lightblue;
}
.state0 .icon {
	fill: white;
}

.state1 {
	fill: lightgray;
}
.state0 .icon {
	fill: white;
}

.state2 {
	fill: lightskyblue;
	color: white;
}
.state0 .icon {
	fill: white;
}

.state3 {
	fill: orange;
	color: white;
}
.state0 .icon {
	fill: white;
}

.state4 {
	fill: red;
	color: white;
}
.state0 .icon {
	fill: white;
}

.state5 {
	fill: red;
	color: white;
}
.state0 .icon {
	fill: white;
}
</style>


<script setup lang="ts">
import { notificationStates } from "@/assets/ts/Notifications";
import { bus, events } from "@/bus";
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";



let message = ref("");
let state = ref(notificationStates.HIDDEN);
let processing = ref(false);
const i18n = useI18n();


const tooltipmessage = computed<string>(() => {
	if (processing.value) {
		return message.value;
	} else {
		return message.value + '\n' + i18n.t('notifications.click_to_close');
	}
});

const icon = computed<string>(() => {
	return state.value < notificationStates.WARN ? "&#xf129;" : "&#xf12a;";
});

const isHidden = computed<boolean>(() => {
	return state.value === notificationStates.HIDDEN;
});

const stateClass = computed<string>(() => {
	return "state" + state.value;
});

onMounted((): void => {
	bus.on(events.Notifications.Clear, () => {
		if (state.value < notificationStates.WARN) {
			state.value = notificationStates.HIDDEN;
			message.value = "";
		}

		processing.value = false;
	});

	bus.on(events.Notifications.Processing, (newmessage?: string) => {
		state.value = notificationStates.INFO;
		if (newmessage) {
			message.value = newmessage;
		}

		processing.value = true;
	});

	bus.on(events.Notifications.Info, (newmessage?: string) => {
		state.value = notificationStates.INFO;
		if (newmessage) {
			message.value = newmessage;
		}
		processing.value = false;
	});

	bus.on(events.Notifications.Warn, (newmessage?: string) => {
		state.value = notificationStates.WARN;
		if (newmessage) {
			message.value = newmessage;
		}
		processing.value = false;
	});

	bus.on(events.Notifications.Error, (newmessage?: string) => {
		state.value = notificationStates.ERROR;
		if (newmessage) {
			message.value = newmessage;
		}
		processing.value = false;
	});

	bus.on(events.Notifications.Fatal, (newmessage?: string) => {
		state.value = notificationStates.FATAL;
		if (newmessage) {
			message.value = newmessage;
		}
		processing.value = false;
	});
});

onBeforeUnmount(() => {
	bus.off(events.Notifications.Clear);
	bus.off(events.Notifications.Processing);
	bus.off(events.Notifications.Info);
	bus.off(events.Notifications.Warn);
	bus.off(events.Notifications.Error);
	bus.off(events.Notifications.Fatal);
});

function onNotificationClicked() {
	if (processing.value === false) {
		state.value = notificationStates.HIDDEN;
		message.value = "";
		processing.value = false;
	}
}

</script>
@/assets/ts/Notifications