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
import { Module } from "vuex";
import { SearchStore } from "./SearchStore";
import { RootState } from "../index";
import Mappings from '@/assets/ts/visualizer/Mappings';
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { ResultSet } from '@/assets/ts/dataMap/ResultSet';
import {ApiNode} from "@/assets/ts/dataMap/ApiNode";
//import { Dictionary } from 'vue-router/types/router';

export interface VisualizerState {
  pendingResults: ResultSet[];
  pendingNodes: ApiNode[];

  simRunning: boolean;
  simProgress: number;
  advancedSearchMode: boolean;
  iconMappings: Mappings;
  subtypeMappings: Mappings;
  playMode: boolean;
  perfMode: boolean;
  selectModeActive: boolean;
  cropModeActive: boolean;

  // currentNodeLabelStates: Dictionary<boolean>;
  // currentEdgeLabelStates: Dictionary<boolean>;
}

const state: VisualizerState = {
  pendingResults: [],
  pendingNodes: [],

  simRunning: false,
  simProgress: 0,
  advancedSearchMode: false,
  iconMappings: null,
  subtypeMappings: null,
  playMode: false,
  perfMode: false,
  selectModeActive: false,
  cropModeActive: false,

  // currentNodeLabelStates: {},
  // currentEdgeLabelStates: {},
}

export const VisualizerStorePaths = {
  mutations: {
    

    Add: {
      PENDING_RESULTS: "visualizer/addPendingResults",
      PENDING_NODE: "visualizer/addPendingNode",
      PENDING_NODES: "visualizer/addPendingNodes",
    },
    Update: {
      SIM_RUNNING: "visualizer/simRunning",
      SIM_PROGRESS: "visualizer/simProgress",
      PLAY_MODE: "visualizer/playMode",
      PERF_MODE: "visualizer/perfMode",
      SELECT_ACTIVE: "visualizer/selectModeActive",
      CROP_ACTIVE: "visualizer/cropModeActive",
      NODE_LABEL_STATES: "visualizer/nodeLabelStates",
      EDGE_LABEL_STATES: "visualizer/edgeLabelStates",
      // TOGGLE_NODE_LABEL_STATE: "visualizer/toggleNodeLabelState",
      // TOGGLE_EDGE_LABEL_STATE: "visualizer/toggleEdgeLabelState",
    },
    Save: {
    },
    Delete: {
      PENDING_NODES: "visualizer/flushPendingNodes",
      PENDING_RESULTS: "visualizer/flushPendingResults",
    }
  },
  actions: {
    INIT: "visualizer/init",
  }
}


export const VisualizerStore: Module<VisualizerState, RootState> = {
  namespaced: true as true,
  modules: {
    search: SearchStore
  },
  state: state,
  mutations: {
    iconMappings(state, data: Mappings) {
      state.iconMappings = new Mappings(data, "f128");
    },
    subtypeMappings(state, data: Mappings) {
      state.subtypeMappings = new Mappings(data, "");
    },
    addPendingResults(state, results: ResultSet) {
      state.pendingResults.push(results);
    },
    addPendingNode(state, node: ApiNode) {
      state.pendingNodes.push(node);
    },
    addPendingNodes(state, nodes: ApiNode[]) {
      state.pendingNodes = state.pendingNodes.concat(nodes);
    },
    flushPendingNodes(state) {
      state.pendingNodes = [];
    },
    flushPendingResults(state) {
      state.pendingResults = [];
    },
    simRunning(state, newState) {
      state.simRunning = newState;
    },
    simProgress(state, newVal) {
      state.simProgress = newVal;
    },
    playMode(state, value) {
      state.playMode = value;
    },
    perfMode(state, value) {
      state.perfMode = value;
    },
    selectModeActive(state, value) {
      state.selectModeActive = value;
    },
    cropModeActive(state, value) {
      state.cropModeActive = value;
    },
  },
  actions: {
    init(context) {
      const iconrequest: Request = {
        url: "/api/visualizer/icons",
        successCallback: (data?: Mappings) => {
          //console.log(data);
          context.commit("iconMappings", data);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          console.error(error);
        }
      };
      api.get(iconrequest);

      //SubTypeMappings object to asign the correct font awesome icon to the correct nodes
      const subtyperequest: Request = {
        url: "/api/visualizer/subtypeproperties",
        successCallback: (data?: Mappings) => {
          context.commit("subtypeMappings", data);
        },
        errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
          console.error(error);
        }
      };
      api.get(subtyperequest);
    }
  }
};
