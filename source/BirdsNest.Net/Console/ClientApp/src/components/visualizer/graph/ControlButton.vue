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
    <div class="control-button">
        <span class="cell has-tip clickable" v-bind:class="{ 'highlighted-button': ishighlighted, 'button-active': isactive }" :title="title">
            <i :class="icon"></i>
        </span>
    </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component({
    props: {
        title: String,
        icon: String,
        ishighlighted: {
            type: Boolean,
            default: false
        },
        isactive: {
            type: Boolean,
            default: false
        }
    }
})
export default class ControlButton extends Vue {

}
</script>

<style>
.control-button {
    /*float: left;*/
    height: 24px;
    width: 16px;
    margin-left: 6px;
    margin-right: 6px;
    margin-top: 6px;
    margin-bottom: 3px;
}

.control-button:hover {
    color: gray;
    transform: scale(1.2);
}

.highlighted-button {
    color: red;
}

.highlighted-button:hover {
    color:hotpink;
}

.button-active {
    color: rgb(39, 71, 255);
}

.button-active:hover {
    color:rgb(78, 104, 255);
}

</style>
