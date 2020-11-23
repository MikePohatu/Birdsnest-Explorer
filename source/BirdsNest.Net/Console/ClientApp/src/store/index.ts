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
import Vue from "vue";
import Vuex from "vuex";
import { api, Request } from "../assets/ts/webcrap/apicrap";
import { VisualizerStore } from "./modules/VisualizerStore";
import PluginManager from '@/assets/ts/dataMap/PluginManager';
import ServerInfo from '@/assets/ts/dataMap/ServerInfo';
import i18n from '@/i18n';

import { bus, events } from '@/bus';
import { Dictionary } from 'vue-router/types/router';
Vue.use(Vuex);

export const rootPaths = {
  mutations: {
    AUTH_MESSAGE: "authMessage",
    IS_AUTHORIZED: "isAuthorized",
    IS_ADMIN: "isAdmin",
    USERNAME: "username",
    SESSION_STATUS: "sessionStatus",
    PLUGIN_MANAGER: "pluginManager",
    API_STATE: "apiState",
    SERVER_INFO_STATE: "serverInfoState",
    DEAUTH: "deAuth",
    SERVER_INFO: "serverInfo",
    LOCALE: "locale"
  },
  actions: {
    UPDATE_PROVIDERS: "updateProviders",
    UPDATE_PLUGINS: "updatePlugins",
    UPDATE_AUTHENTICATED_DATA: "updateAuthedData",
    UPDATE_SERVER_INFO: "updateServerInfo"
  }
}

export interface LanguageSelector {
  flag: string;
  title: string;
}

export interface RootState {
  auth: {
    message: string;
  };
  user: {
    isAuthorized: boolean;
    isAdmin: boolean;
    name: string;
  };
  session: {
    status: string;
    providers: string[];
  };
  locale: string;
  languages: Dictionary<LanguageSelector>;
  pluginManager: PluginManager;
  serverInfo: ServerInfo;
  serverInfoState: number;
  apiState: number;
  visualizer?;
}

const state: RootState = {
  auth: {
    message: ""
  },
  user: {
    isAuthorized: false,
    isAdmin: false,
    name: ""
  },
  session: {
    status: "",
    providers: [],
  },
  locale: "en",
  languages: {
    "en": { flag: "us", title: "English (US)" },
    "mi": { flag: "nz", title: "Māori" }
  },
  pluginManager: null,
  serverInfo: null,
  serverInfoState: api.states.NOTAUTHORIZED,
  apiState: api.states.NOTAUTHORIZED
}

export default new Vuex.Store({
  strict: process.env.NODE_ENV !== 'production',
  modules: {
    visualizer: VisualizerStore
  },
  state: state,
  mutations: {
    authMessage(state, message: string) {
      state.auth.message = message;
    },
    locale(state, locale: string) {
      i18n.locale = locale;
      state.locale = i18n.locale;
      Vue.prototype.$cookies.set("locale", i18n.locale);
    },
    isAuthorized(state, isauthenticated: boolean) {
      state.user.isAuthorized = isauthenticated;
    },
    isAdmin(state, isadmin: boolean) {
      state.user.isAdmin = isadmin;
    },
    username(state, name: string) {
      state.user.name = name;
    },
    sessionStatus(state, statusmessage: string) {
      state.session.status = statusmessage;
    },
    pluginManager(state, newManager: PluginManager) {
      state.pluginManager = newManager;
    },
    serverInfo(state, newStats: ServerInfo) {
      state.serverInfo = newStats;
    },
    serverInfoState(state, newstate: number) {
      state.serverInfoState = newstate;
    },
    apiState(state, newstate: number) {
      state.apiState = newstate;
      if (newstate === api.states.READY) {
        bus.$emit(events.Notifications.Clear);
      }
    },
    deAuth(state) {
      state.user.isAuthorized = false;
      state.user.isAdmin = false;
      state.session.status = "Not authorized";
    },
    providers(state, value) {
      state.session.providers = value;
    }
  },
  actions: {
    updateProviders(context) {
      const request: Request = {
        url: "/api/account/providers",
        successCallback: (data: string[]) => {
          context.commit('providers', data);
          context.commit(rootPaths.mutations.API_STATE, api.states.READY);
        },
        errorCallback: (jqXHR, status, error: string) => {
          // eslint-disable-next-line
          console.error(error);
        },
      };
      api.get(request);
    },

    updatePlugins(context) {
      context.commit(rootPaths.mutations.API_STATE, api.states.LOADING);
      const request: Request = {
        url: "/api/plugins",
        successCallback: (data: PluginManager) => {
          context.commit(rootPaths.mutations.PLUGIN_MANAGER, data);
          context.commit(rootPaths.mutations.API_STATE, api.states.READY);
          bus.$emit(events.Notifications.Clear);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          context.commit(rootPaths.mutations.API_STATE, api.states.ERROR);
          bus.$emit(events.Notifications.Error, "Error updating plugins: " + error);
        }
      }
      api.get(request);
    },

    updateServerInfo(context) {
      // eslint-disable-next-line
      console.log("Refreshing server info");
      context.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.LOADING);
      const request: Request = {
        url: "/api/serverinfo",
        successCallback: (data: PluginManager) => {
          context.commit(rootPaths.mutations.SERVER_INFO, data);
          context.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.READY);
          bus.$emit(events.Notifications.Clear);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          context.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
          bus.$emit(events.Notifications.Error, "Error updating server info: " + error);
        }
      }
      api.get(request);
    },
    updateAuthedData(context) {
      if (this.state.user.isAuthorized) {
        context.dispatch(rootPaths.actions.UPDATE_PLUGINS);
        context.dispatch(rootPaths.actions.UPDATE_SERVER_INFO);
      }
    }
  }
});
