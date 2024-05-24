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
	<div id="eventspane" ref="eventspane" :class="[{ hidepane: isHidden }]" :title="$t('word_Notifications')">
        <div id="eventspaneHider" class="clickable" v-on:click.native="onHiderClicked()">
            <span class="hiderButton">
                <i :class="['fa', {'fa-angle-up': isHidden, 'fa-angle-down': !isHidden }]"></i>
            </span>
        </div>
        
        <div id="output">
            
            <div id="header">
                <span>{{ $t("word_Notifications") }}</span>
                <!-- <span v-if="isDevMode" class="clickable" v-on:click.native="onTestEventsClicked()"> - TEST MESSAGES</span> -->
                <span id="clear" class="clickable" v-on:click.native="onClearClicked()" :title="$t('phrase_Clear_notifications')">
                    <i class="fa-regular fa-trash-can"></i>
                </span>
            </div>
            <div id="messagelist" ref="messagelist" :style="{height: messageHeight}">
                <ul v-if="messages.length > 0" >
                    <li v-for="item in messages" :key="item.eventNumber" :class="[item.level]">
                        <div v-if="item.level === NotificationMessageLevels.PROCESSING">{{ `** ${item.message}` }}</div> 
                        <div v-else>{{ `${item.level}: ${item.message}` }}</div>
                    </li>
                </ul>
                <ul v-else>
                    <li><i>{{ $t("phrase_Nothing_to_show") }}</i></li>
                </ul>
            </div>
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
    max-height: calc(100% - 60px);
    max-height: -o-calc(100% - 60px); /* opera */
    max-height: -webkit-calc(100% - 60px); /* google, safari */
    max-height: -moz-calc(100% - 60px); /* firefox */
	z-index: 5001 !important;
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
    right: 1px;
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

#output {
    position: relative;
}

#header {
    padding: 2px 5px 0 5px;
    font-weight: bold;
    vertical-align: top;
}

#header span {
    vertical-align: top;
}

#clear {
    position: absolute;
    bottom: 0;
    right: 0;
    font-weight:lighter;
    font-size: 1.1em;
    padding: 0 5px 0 5px;
    margin: 0;
    color: orange;
}

#messagelist{
    overflow: auto;
    scrollbar-width: thin;
    font-size: smaller;
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
import { NotificationMessage, NotificationMessageLevels, Notify, Messages } from '@/assets/ts/Notifications';
import { i18nGetPhrase } from '@/i18n';
import { computed, onUpdated, ref } from 'vue';

    const isHidden = ref(true);
    const messagelist = ref(null);
    const eventspane = ref(null);
    const isDevMode = import.meta.env.DEV;

    const messages = computed<NotificationMessage[]>(() => {
        return Messages;
    });
    const messageHeight = ref("3em");
    
    function UpdateHeight(): void {
        if (isHidden.value) { return; }

        //'no messsages' case also covers component not being mounted,
        //i.e. messagelist.value == null
        if (messages.value.length === 0) { 
            messageHeight.value = '3em'; 
        } 
        else {
            const lineheight = 1.2;
            const fontsize = parseFloat(getComputedStyle(messagelist.value).fontSize);

            //find the new height, and the height available in the parent element. 50px 
            //buffer is to allow for topbar, hiderbutton, and some padding
            const newheight = (messages.value.length+1)*lineheight*fontsize+5;
            const maxheight = eventspane.value.parentElement.offsetHeight - 50;

            if (newheight >= maxheight) {
                messageHeight.value = `${(maxheight).toString()}px`;
            }
            else {
                messageHeight.value = `${(newheight).toString()}px`; 
            }

            setTimeout(() => {
                messagelist.value.scrollTo(0, messagelist.value.scrollHeight);
            }, 50);            
        }
    }

    function onHiderClicked() {
        isHidden.value = !isHidden.value;
        if (!isHidden.value) { UpdateHeight(); }
    }

    function onClearClicked() {
        if (confirm(`${i18nGetPhrase("Clear_notifications")}?`)) {
            Notify.Flush();
		}
    }

    onUpdated(()=> {
        UpdateHeight();
    })
</script>