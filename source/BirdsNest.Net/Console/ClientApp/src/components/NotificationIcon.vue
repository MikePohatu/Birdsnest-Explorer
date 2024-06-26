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
	<div
		id="graphNotification"
		:class="{ disabled: isHidden }"
		:title="tooltipmessage"
	>
		<div style="position: relative">
			<svg viewBox="-32 -32 64 64" width="64" height="64" :class="{ spinner: processing }">
				<g>
					<circle id="notificationCircle" r="24" :class="stateClass" />
					<path v-show="processing" id="spinnerIcon" d="M 0 -18 a 18 18 0 1 1 -12.78 5.22" />
				</g>
			</svg>
			<div id="notificationIcon">
				<span v-html="icon" class="icon noselect"></span>
			</div>
		</div>
				
		<div id="notificationIconClose" v-on:click="onNotificationClicked()">
			<span class="fa-solid fa-circle-xmark clickable"></span>
		</div>
	</div>
</template>



<style scoped>
#notificationIconClose { 
	visibility: hidden;
	color:slategrey;
	font-weight: bold;
	position: absolute; 
	top: 3px; 
	right: 3px;
}
#graphNotification:hover > #notificationIconClose { visibility: visible; }

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
import { NotificationMessage, NotificationMessageLevels, notificationStates } from "@/assets/ts/Notifications";
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
	bus.on(events.ClearNotifications, () => {
		if (state.value < notificationStates.WARN) {
			state.value = notificationStates.HIDDEN;
			message.value = "";
		}

		processing.value = false;
	});

	bus.on(events.Notify, (newmessage?: NotificationMessage) => {
		let isprocessing = false;
		switch (newmessage.level) {
			case NotificationMessageLevels.INFO:
				state.value = notificationStates.INFO;
				break;
			case NotificationMessageLevels.WARN:
				state.value = notificationStates.WARN;
				break;
			case NotificationMessageLevels.ERROR:
				state.value = notificationStates.ERROR;
				break;
			case NotificationMessageLevels.FATAL:
				state.value = notificationStates.FATAL;
				break;	
			case NotificationMessageLevels.PROCESSING:
				state.value = notificationStates.INFO;
				isprocessing = true;
				break;	
		}
		message.value = newmessage.message;
		processing.value = isprocessing;
	});
});

onBeforeUnmount(() => {
	bus.off(events.ClearNotifications);
	bus.off(events.Notify);
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