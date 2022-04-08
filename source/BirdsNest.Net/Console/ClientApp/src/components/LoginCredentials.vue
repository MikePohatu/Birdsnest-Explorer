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
	<div class="grid-x align-center-middle">
		<div class="cell"></div>
		<div class="cell loginbox" style="max-width: 350px; min-width: 280px;">
			<fieldset class="fieldset">
				<legend>{{ $t("word_Login") }}</legend>
				<form id="loginForm" class="cell" type="POST" @submit.prevent="login">
					<div class="input-group">
						<input
							required
							ref="usernameEl"
							v-model="username"
							tabindex="1"
							type="text"
							:placeholder="$t('word_Username')"
							class="small-8 input-group-field"
						/>
					</div>

					<div class="input-group">
						<input
							required
							id="password"
							v-model="password"
							tabindex="2"
							type="password"
							:placeholder="$t('word_Password')"
							class="small-8 input-group-field"
						/>
					</div>

					<div class="input-group">
						<select
							id="provider"
							tabindex="3"
							v-model="provider"
							class="small-8 input-group-field"
							required
						>
							<option v-for="prov in providers" :key="prov" :value="prov">{{ prov }}</option>
						</select>
					</div>

					<div class="grid-x">
						<div class="cell small-3 medium-3 large-3">
							<input id="loginbtn" tabindex="3" type="submit" class="button" :value="$t('word_Login')" />
						</div>
					</div>

					<div id="authmessage" class="cell small-9 medium-9 large-9">
						<span>{{ message }}</span>
					</div>
				</form>
			</fieldset>
		</div>
		<div class="cell"></div>
	</div>
</template>


<style scoped>
#authmessage {
	padding: 0;
	font-size: 0.8em;
	min-height: 2em;
}
</style>


<script setup lang="ts">
import { computed, onActivated, onMounted, ref } from "vue";
import { useStore } from "@/store";
import { auth } from "../assets/ts/webcrap/authcrap";
import { rootPaths } from "../store";

const store = useStore();

let username = ref("");
let password = ref("");
let provider = ref("");
const usernameEl = ref(null);


const message = computed((): string => {
	return store.state.auth.message;
});

const providers = computed((): string[] => {
	return store.state.session.providers;
});



onMounted((): void => {
	if (providers.value && providers.value.length > 0) {
		provider.value = providers.value[0];
	}
	
	store.commit(rootPaths.mutations.SESSION_STATUS, "");
});

onActivated(():void => {
	(usernameEl as HTMLElement).focus();
});

function login(): void {
	auth.login(username.value, password.value, provider.value, () => {
		password.value = "";
	});
}
</script>

<style scoped>
.loginbox {
	background-color: white;
	padding-left: 15px;
	padding-right: 15px;
}
</style>