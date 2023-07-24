// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
import $ from "jquery";

import "@fortawesome/fontawesome-free/css/all.min.css";
import "foundation-sites";

import "motion-ui/dist/motion-ui.css";
import "foundation-sites/dist/css/foundation.css";

import i18n from "./i18n";
import CountryFlag from "vue-country-flag-next"
import VueCookies from "vue3-cookies";

import { createApp } from 'vue';
import { store, key, rootPaths } from "./store";
import router from "./router";
import App from './App.vue';

const app = createApp(App);

app.use(CountryFlag)
    .use(VueCookies)
    .use(router)
    .use(i18n)
    .use(store, key)
    .mount('#app');

//Vue.config.productionTip = false;

$.ajaxSetup({ xhrFields: { withCredentials: true }, cache: false });
$(document).foundation();
store.dispatch(rootPaths.actions.INIT);