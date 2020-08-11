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
	<div id="graphNotification" :class="{disabled: isHidden}" :title="tooltipmessage" v-on:click="onNotificationClicked()">
		<div style="position: relative">
			<svg viewBox="-32 -32 64 64" width="64" height="64" :class="{spinner: processing}">
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


<script lang="ts">
import { bus, events } from "@/bus";
import { Component, Vue } from "vue-property-decorator";

const notificationStates = {
	HIDDEN: -1,
	TRACE: 0,
	DEBUG: 1,
	INFO: 2,
	WARN: 3,
	ERROR: 4,
	FATAL: 5,
};

@Component
export default class NotificationIcon extends Vue {
	message = "";
	state = notificationStates.HIDDEN;
	states = notificationStates;
	processing = false;

	get tooltipmessage(): string {
		if (this.processing) {
			return this.message;
		} else {
			return this.message + '\nClick icon to close notification';
		}
	}
	get icon(): string {
		return this.state < notificationStates.WARN ? "&#xf129;" : "&#xf12a;";
	}
	get iconSize(): number {
		if (this.processing) {
			return 20;
		} else {
			return 22;
		}
	}

	get iconXY(): number {
		if (this.processing) {
			return 12;
		} else {
			return 10;
		}
	}

	get isHidden(): boolean {
		return this.state === notificationStates.HIDDEN;
	}

	get stateClass(): string {
		return "state" + this.state;
	}

	created() {
		bus.$on(events.Notifications.Clear, () => {
			if (this.state < notificationStates.WARN) {
				this.state = notificationStates.HIDDEN;
				this.message = "";
			}

			this.processing = false;
		});

		bus.$on(events.Notifications.Processing, (message?: string) => {
			this.state = notificationStates.INFO;
			if (message) {
				this.message = message;
			}

			this.processing = true;
		});

		bus.$on(events.Notifications.Info, (message?: string) => {
			this.state = notificationStates.INFO;
			if (message) {
				this.message = message;
			}
			this.processing = false;
		});

		bus.$on(events.Notifications.Warn, (message?: string) => {
			this.state = notificationStates.WARN;
			if (message) {
				this.message = message;
			}
			this.processing = false;
		});

		bus.$on(events.Notifications.Error, (message?: string) => {
			this.state = notificationStates.ERROR;
			if (message) {
				this.message = message;
			}
			this.processing = false;
		});

		bus.$on(events.Notifications.Fatal, (message?: string) => {
			this.state = notificationStates.FATAL;
			if (message) {
				this.message = message;
			}
			this.processing = false;
		});
	}

	beforeDestroy() {
		bus.$off(events.Notifications.Clear);
		bus.$off(events.Notifications.Processing);
		bus.$off(events.Notifications.Info);
		bus.$off(events.Notifications.Warn);
		bus.$off(events.Notifications.Error);
		bus.$off(events.Notifications.Fatal);
	}

	onNotificationClicked() {
		if (this.processing === false) {
			this.state = notificationStates.HIDDEN;
		this.message = "";
		this.processing = false;
		}
	}
}
</script>
