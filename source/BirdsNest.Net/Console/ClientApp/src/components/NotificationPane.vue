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
	<div id="eventspane" :class="['grid-x', { hidepane: isHidden }]" :title="$t('word_Notifications')">
        <div id="eventspaneHider" class="clickable cell" v-on:click.native="onHiderClicked()">
            <span class="hiderButton">
                <i :class="['fa', {'fa-angle-up': isHidden, 'fa-angle-down': !isHidden }]"></i>
            </span>
        </div>
        <div class="cell">
            <div class="header">{{ $t("word_Notifications") }}</div>
            <ul class="messagelist" >
                <li v-if="messages.length > 0" v-for="item in messages" :key="item.eventNumber" :class="[item.level]">
                    <div v-if="item.level !== NotificationMessageTypes.PROCESSING">{{ `${item.level}: ${item.message}` }}</div> 
                    <div v-else>{{ `** ${item.message}` }}</div>
                </li>
                <li v-else><i>{{ $t("phrase_Nothing_to_show") }}</i></li>
            </ul>
            
        </div>
	</div>
</template>

<style scoped>
#eventspane {
	position: fixed;
	bottom: 0;
    right: 0;
	padding: 0;
	background: white;
	border-top: 1px solid #d6d6d6;
	border-left: 1px solid #d6d6d6;
	margin: 0;
    min-width: 150px;
    font-size: small;
    vertical-align: text-top;
    overflow: visible;
    border-top-left-radius: 3px;
}

#eventspane.hidepane {
    height: 0px;
	border-top: 0;
	border-left: 0;
}

#eventspaneHider {
    border: 0;
    padding: 0;
    position: absolute;
    top: -20px;
    right: 0;
	margin-left: auto; 
    margin-right: 0;
    height: 20px;
    width: 30px;
	background: white;
    border: 1px solid #d6d6d6;
    border-bottom: 0;
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
}

.hiderButton i {
    height: 16px;
    width: 16px;
    padding-top: 5px;
    padding-left: 7px;
}

ul {
    padding: 0 5px 0 5px;
    list-style-type: none;
    margin-left: 0;
}

li {
    margin: 0 0 0 3px;
    line-height: 1.2;
    text-align: left;
    font-size: smaller;
}

.header {
    padding: 2px 5px 0 5px;
    font-weight: bold;
}

.INFO {
    color: lightslategrey;
}

.WARN {
    color: orange;
}

.ERROR {
    color: red;
}
</style>

<script setup lang="ts">
import { NotificationMessage, NotificationMessageTypes } from '@/assets/ts/Notifications';
import { store } from '@/store';
import { computed, ref } from 'vue';

    const isHidden = ref(true);

    const messages = computed<NotificationMessage[]>(() => {
        return store.state.notificationMessages;
    });



    function onHiderClicked() {
        this.isHidden = !this.isHidden;
    }
</script>