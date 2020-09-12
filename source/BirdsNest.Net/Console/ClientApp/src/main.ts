// Copyright (c) 2019-2020 "20Road"
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
import $ from 'jquery';

require('@fortawesome/fontawesome-free/css/all.min.css');
import 'foundation-sites';

import 'motion-ui/dist/motion-ui.css';
import 'foundation-sites/dist/css/foundation.css';

import Vue from "vue";

import App from "./App.vue";
import "./registerServiceWorker";
import router from "./router";
import store from "./store";
import i18n from './i18n';
import FlagIcon from 'vue-flag-icon';
import VueCookies from 'vue-cookies';

Vue.use(FlagIcon);
Vue.use(VueCookies);

Vue.config.productionTip = false;
new Vue({
  router,
  store,
  i18n,
  render: h => h(App)
}).$mount("#app");

$.ajaxSetup({ xhrFields: { withCredentials: true }, cache: false });
$(document).foundation();
