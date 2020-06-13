// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
import { api, Request} from "../assets/ts/webcrap/apicrap";
import { VisualizerStore } from "./modules/VisualizerStore";
import PluginManager from '@/assets/ts/dataMap/PluginManager';
import { bus, events } from '@/bus';
Vue.use(Vuex);

export const rootPaths = {
  mutations: {
    IS_AUTHORIZED: "isAuthorized",
    IS_ADMIN: "isAdmin",
    SESSION_STATUS: "sessionStatus",
    PLUGIN_MANAGER: "pluginManager",
    API_STATE: "apiState",
    DEAUTH: "deAuth",
  },
  actions: {
    UPDATE_PROVIDERS: "updateProviders",
    UPDATE_PLUGINS: "updatePlugins",
    UPDATE_AUTHENTICATED_DATA: "updateAuthedData",
  }
}

export interface RootState {
    user: {
      isAuthorized: boolean;
      isAdmin: boolean;
      name: string;
    };
    session: {
      status: string;
      providers: string[];
    };
    pluginManager: PluginManager;
    apiState: number;
    visualizer?;
}

const state: RootState = {
  user: {
    isAuthorized: false,
    isAdmin: false,
    name: ""
  },
  session: {
    status: "",
    providers: [],
  },
  pluginManager: null,
  apiState: api.states.NOTAUTHORIZED
}

export default new Vuex.Store({
  strict: process.env.NODE_ENV !== 'production',
  modules: {
    visualizer: VisualizerStore
  },
  state: state,
  mutations: {
    isAuthorized(state, isauthenticated: boolean) {
      state.user.isAuthorized = isauthenticated;
    },
    isAdmin(state, isadmin: boolean) {
      state.user.isAdmin = isadmin;
    },
    sessionStatus(state, statusmessage: string) {
      state.session.status = statusmessage;
    },
    pluginManager(state, newManager: PluginManager) {
      state.pluginManager = newManager;
    },
    apiState(state, newstate: number) {
      state.apiState = newstate;
      if (newstate === api.states.READY) {
        bus.$emit(events.Notifications.Clear);
      } else if (newstate === api.states.LOADING) {
        bus.$emit(events.Notifications.Info, "Loading");
        bus.$emit(events.Notifications.Processing);
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
        },
        errorCallback: () => {
          context.commit(rootPaths.mutations.API_STATE, api.states.ERROR);
        }
      } 
      api.get(request);
    },
    updateAuthedData(context) {
      if (this.state.user.isAuthorized) {
        context.dispatch(rootPaths.actions.UPDATE_PLUGINS);
      }
    }
  }
});
