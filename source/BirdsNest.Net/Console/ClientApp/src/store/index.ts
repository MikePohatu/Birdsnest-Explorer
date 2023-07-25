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
import { InjectionKey } from "vue";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { VisualizerStore } from "./modules/VisualizerStore";
import PluginManager from '@/assets/ts/dataMap/PluginManager';
import ServerInfo from '@/assets/ts/dataMap/ServerInfo';

import { bus, events } from '@/bus';
import { Dictionary } from "@/assets/ts/webcrap/misccrap";
import { createStore, useStore as baseUseStore, Store } from 'vuex';
import i18n from "@/i18n";
import { useCookies } from "vue3-cookies";
import { NotificationMessage } from "@/assets/ts/Notifications";

const { cookies } = useCookies();

export const rootPaths = {
  mutations: {
    AUTH_MESSAGE: "authMessage",
    CUSTOMIZATION: {
      LOGIN_BANNER: "loginBanner",
      LOGIN_FOOTER: "loginFooter",
      PORTAL_BANNER: "portalBanner",
      PORTAL_FOOTER: "portalFooter"
    },
    LOGIN: {
      USERNAME: "loginUsername",
      PROVIDER: "loginProvider"
    },
    ADD_MESSAGE: "addMessage",
    CLEAR_PROCESSING_MESSAGEs: "clearProcessingMessages",
    IS_AUTHORIZED: "isAuthorized",
    IS_ADMIN: "isAdmin",
    USERGN: "usergn",
    USERNAME: "username",
    SESSION_STATUS: "sessionStatus",
    PLUGIN_MANAGER: "pluginManager",
    API_STATE: "apiState",
    SERVER_INFO_STATE: "serverInfoState",
    DEAUTH: "deAuth",
    SERVER_INFO: "serverInfo",
    LOCALE: "locale",
    PROVIDERS: "providers",
    PROVIDER: "provider"
  },
  actions: {
    INIT: "init",
    UPDATE_CUSTOMIZATION: "updateCustomization",
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
  customization: {
    login: {
      banner: string;
      footer: string;
    };
    portal: {
      banner: string;
      footer: string;
    };
  };
  login: {
    username: string;
    provider: string;
  };
  user: {
    isAuthorized: boolean;
    isAdmin: boolean;
    gn: string;
    userName: string;
  };
  session: {
    status: string;
    providers: string[];
    provider: string;
  };
  notificationMessages: NotificationMessage[];
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
  customization: {
    login: {
      banner: "",
      footer: ""
    },
    portal: {
      banner: "",
      footer: ""
    }
  },
  login: {
    username: "",
    provider: ""
  },
  user: {
    isAuthorized: false,
    isAdmin: false,
    gn: "",
    userName: ""
  },
  session: {
    status: "",
    providers: [],
    provider: ""
  },
  notificationMessages: [],
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

export const key: InjectionKey<Store<RootState>> = Symbol();

export function useStore () {
  return baseUseStore(key);
}

export const store = createStore({
  strict: import.meta.env.PROD !== true,
  modules: {
    visualizer: VisualizerStore
  },
  state: state,
  mutations: {
    authMessage(state, message: string) {
      state.auth.message = message;
    },
    addMessage(state, message: NotificationMessage) {
      if (state.notificationMessages.length > 100) {
        state.notificationMessages.pop();
      }
      state.notificationMessages.push(message);
    },
    clearProcessingMessages(state) {
      state.notificationMessages = state.notificationMessages.filter(message => message.level !== "PROCESSING");
    },
    locale(state, locale: string) {
      i18n.global.locale.value = locale;
      state.locale = i18n.global.locale.value;
      cookies.set("locale", i18n.global.locale.value);
    },
    loginBanner(state, bannerHtml: string) {
      state.customization.login.banner = bannerHtml;
    },
    loginFooter(state, footerHtml: string) {
      state.customization.login.footer = footerHtml;
    },
    portalBanner(state, bannerHtml: string) {
      state.customization.portal.banner = bannerHtml;
    },
    portalFooter(state, footerHtml: string) {
      state.customization.portal.footer = footerHtml;
    },
    isAuthorized(state, isauthenticated: boolean) {
      state.user.isAuthorized = isauthenticated;
    },
    isAdmin(state, isadmin: boolean) {
      state.user.isAdmin = isadmin;
    },
    loginUsername(state, name: string){
      state.login.username = name;
    },
    loginProvider(state, prov: string){
      state.login.provider = prov;
    },
    usergn(state, name: string) {
      state.user.gn = name;
    },
    username(state, name: string) {
      state.user.userName = name;
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
        bus.emit(events.Notifications.Clear);
      }
    },
    deAuth(state) {
      state.user.isAuthorized = false;
      state.user.isAdmin = false;
      state.session.status = "Not authorized";
    },
    providers(state, value) {
      state.session.providers = value;
    },
    provider(state, value: string) {
      state.session.provider = value;
    }
  },
  actions: {
    init(context) {
      const storedlocale = cookies.get("locale");
      if (storedlocale !== null) {
        store.commit(rootPaths.mutations.LOCALE, storedlocale);
      }
      context.dispatch(rootPaths.actions.UPDATE_CUSTOMIZATION);
    },

    updateCustomization(context) {
      //login_banner
      api.get({
        url: "/static/customization/login_banner.htm",
        successCallback: (data: string[]) => {
          context.commit(rootPaths.mutations.CUSTOMIZATION.LOGIN_BANNER, data);
        },
        errorCallback: (jqXHR, status, error: string) => {
          // eslint-disable-next-line
          console.error(error);
        },
      });

      //login_footer
      api.get({
        url: "/static/customization/login_footer.htm",
        successCallback: (data: string[]) => {
          context.commit(rootPaths.mutations.CUSTOMIZATION.LOGIN_FOOTER, data);
        },
        errorCallback: (jqXHR, status, error: string) => {
          // eslint-disable-next-line
          console.error(error);
        },
      });

      //portal_banner
      api.get({
        url: "/static/customization/portal_banner.htm",
        successCallback: (data: string[]) => {
          context.commit(rootPaths.mutations.CUSTOMIZATION.PORTAL_BANNER, data);
        },
        errorCallback: (jqXHR, status, error: string) => {
          // eslint-disable-next-line
          console.error(error);
        },
      });

      //portal_footer
      api.get({
        url: "/static/customization/portal_footer.htm",
        successCallback: (data: string[]) => {
          context.commit(rootPaths.mutations.CUSTOMIZATION.PORTAL_FOOTER, data);
        },
        errorCallback: (jqXHR, status, error: string) => {
          // eslint-disable-next-line
          console.error(error);
        },
      });
    },

    updateProviders(context) {
      const request: Request = {
        url: "/api/account/providers",
        successCallback: (data: string[]) => {
          context.commit(rootPaths.mutations.PROVIDERS, data);
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
          bus.emit(events.Notifications.Clear);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          context.commit(rootPaths.mutations.API_STATE, api.states.ERROR);
          bus.emit(events.Notifications.Error, "Error updating plugins: " + error);
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
          bus.emit(events.Notifications.Clear);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          context.commit(rootPaths.mutations.SERVER_INFO_STATE, api.states.ERROR);
          bus.emit(events.Notifications.Error, "Error updating server info: " + error);
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
