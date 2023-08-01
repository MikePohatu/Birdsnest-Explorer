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
import { Module } from "vuex";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { Search, SearchItem, SearchEdge, SearchNode, Condition, moveCondition, ConditionType, AndOrCondition, ValueCondition, UpdateCondition, DeleteCondition, importNode, importEdge } from "@/assets/ts/visualizer/Search";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { RootState } from '@/store';
import { ResultSet } from '@/assets/ts/dataMap/ResultSet';
import i18n, { i18nGetPhrase, i18nGetWord } from '@/i18n';

const { t } = i18n.global;

export const SearchStorePaths = {
    mutations: {
        CANCEL_ITEM_EDIT: "visualizer/search/cancelItemEdit",
        CANCEL_CONDITION_EDIT: "visualizer/search/cancelCondEdit",
        CANCEL_ANDOR_EDIT: "visualizer/search/cancelAndOrEdit",
        CANCEL_NEW_CONDITION: "visualizer/search/cancelNewCondition",
        RESET: "visualizer/search/reset",
        TOGGLE_SEARCH: "visualizer/search/toggleSearch",
        TOGGLE_SEARCH_MODE: "visualizer/search/toggleSearchMode",

        Add: {
            NEW_NODE: "visualizer/search/newNode",
            NEW_CONDITION: "visualizer/search/newCondition",
            NEW_CONDITION_PARENT: "visualizer/search/newConditionParent"
        },
        Update: {
            SELECTED_ITEM_DOWN: "visualizer/search/itemDown",
            SELECTED_ITEM_UP: "visualizer/search/itemUp",
            SELECTED_ITEM: "visualizer/search/selectedItem",
            SEARCH: "visualizer/search/search",
            EDIT_ITEM: "visualizer/search/setEditItem",

            EDIT_CONDITION: "visualizer/search/setEditCondition",
            INCLUDE_DISABLED: "visualizer/search/includeDisabled",
            SELECTED_CONDITION: "visualizer/search/selectedCondition",
            SELECTED_CONDITION_DOWN: "visualizer/search/conditionDown",
            SELECTED_CONDITION_UP: "visualizer/search/conditionUp",

            SHARE_URL: "visualizer/search/shareUrl",
            SHARE_CYPHER: "visualizer/search/shareCypher",
        },
        Save: {
            EDIT_EDGE: "visualizer/search/saveEditingEdge",
            EDIT_NODE: "visualizer/search/saveEditingNode",
            EDIT_VALUE_CONDITION: "visualizer/search/saveEditingCondition",
            EDIT_ANDOR_CONDITION: "visualizer/search/saveEditingAndOrCondition",
        },
        Delete: {
            SELECTED_ITEM: "visualizer/search/deleteSelectedItem",
            EDIT_NODE: "visualizer/search/deleteEditingNode",
            EDIT_VALUE_CONDITION: "visualizer/search/deleteEditingValueCondition",
            EDIT_ANDOR_CONDITION: "visualizer/search/deleteEditingAndOrCondition",
            SELECTED_CONDITION: "visualizer/search/deleteSelectedCondition",
            RESULTS: "visualizer/search/deleteResults",
        }
    },
    actions: {
        SEARCH: "visualizer/search/search",
        SIMPLE_SEARCH: "visualizer/search/simpleSearch",
        UPDATE_SHARE: "visualizer/search/updateShare",
    }
}

export interface SearchState {
    search: Search;
    results: ResultSet;
    searchEnabled: boolean;
    simpleSearchMode: boolean;

    lastIndex: number;
    selectedItem: SearchItem;
    editNode: SearchNode;
    editEdge: SearchEdge;

    selectedCondition: Condition;
    editAndOrCondition: AndOrCondition;
    editValCondition: ValueCondition;
    newConditionParent: AndOrCondition;
    newCondition: Condition;

    shareCypher: string;
    shareUrl: string;
    statusMessage: string;
    isSearching: boolean;
}

const state: SearchState = {
    search: new Search(),
    results: null,
    
    searchEnabled: false,
    simpleSearchMode: true,

    selectedItem: null,
    editNode: null,
    editEdge: null,
    lastIndex: 0,

    selectedCondition: null,
    editAndOrCondition: null,
    editValCondition: null,
    newConditionParent: null,
    newCondition: null,

    shareCypher: "",
    shareUrl: "",
    statusMessage: null,
    isSearching: false,
}

export const SearchStore: Module<SearchState, RootState> = {
    namespaced: true,
    state: state,
    mutations: {
        //main bits
        search(state, newSeach: Search) {
            state.search = newSeach;
            state.lastIndex = newSeach.nodes.length;
        },
        results(state, newResults: ResultSet) {
            state.results = newResults;
        },
        reset(state): void {
            state.selectedItem = null;
            state.selectedCondition = null;
            state.editAndOrCondition = null;
            state.editEdge = null;
            state.editNode = null;
            state.shareCypher = "";
            state.shareUrl = "";
            state.lastIndex = 0;
            state.search = new Search();
        },
        toggleSearch(state): void {
            state.searchEnabled = !state.searchEnabled;
        },
        toggleSearchMode(state): void {
            state.simpleSearchMode = !state.simpleSearchMode;
        },
        deleteResults(state): void {
            state.results = null;
        },

        //status message mutations
        setStatusMessage(state, newMessage): void {
            state.statusMessage = newMessage;
        },
        clearStatusMessage(state): void {
            state.statusMessage = null;
        },
        setIsSearching(state, isseaching: boolean): void {
            state.isSearching = isseaching;
        },


        //#region Path update mutations
        selectedItem(state, newItem: SearchItem) {
            state.selectedItem = newItem;
        },
        newNode(state): void {
            const newNode = new SearchNode();
            state.lastIndex++;
            newNode.name = "node" + state.lastIndex;
            webcrap.array.pushItem<SearchNode>(state.search.nodes, newNode);
            if (state.search.nodes.length > 1) {
                const newEdge = new SearchEdge();
                newEdge.name = "hop" + (state.lastIndex - 1);
                webcrap.array.pushItem<SearchEdge>(state.search.edges, newEdge);
            }
        },
        saveEditingNode(state, value: SearchNode) {
            if (state.editNode === null) {
                webcrap.array.pushItem<SearchNode>(state.search.nodes, value);
            } else {
                importNode(value, state.editNode);
                state.editNode = null;
            }
        },
        saveEditingEdge(state, value: SearchEdge) {
            if (state.editEdge === null) {
                webcrap.array.pushItem<SearchEdge>(state.search.edges, value);
            } else {
                //value.id = state.editEdge.id;
                //webcrap.array.replaceItem<SearchEdge>(state.search.edges, state.editEdge, value);
                importEdge(value, state.editEdge);
                state.editEdge = null;
            }
        },
        itemUp(state): void {
            if (state.selectedItem.type === "SearchNode") {
                webcrap.array.moveItemLeft<SearchNode>(state.search.nodes, state.selectedItem as SearchNode);
            } else {
                webcrap.array.moveItemLeft<SearchEdge>(state.search.edges, state.selectedItem as SearchEdge);
            }
        },
        itemDown(state): void {
            if (state.selectedItem.type === "SearchNode") {
                webcrap.array.moveItemRight<SearchNode>(state.search.nodes, state.selectedItem as SearchNode);
            } else {
                webcrap.array.moveItemRight<SearchEdge>(state.search.edges, state.selectedItem as SearchEdge);
            }
        },
        deleteSelectedItem(state): void {
            if (state.selectedItem.type === "SearchNode") {
                const topIndex = state.search.nodes.length - 1;
                const index = webcrap.array.removeItem<SearchNode>(state.search.nodes, state.selectedItem as SearchNode);
                if (index === topIndex) {
                    webcrap.array.removeItem<SearchEdge>(state.search.edges, state.search.edges[index - 1]);
                } else if (topIndex !== 0) {
                    webcrap.array.removeItem<SearchEdge>(state.search.edges, state.search.edges[index]);
                }
                state.selectedItem = null;
            } else {
                // eslint-disable-next-line
                console.error("Can't delete an edge");
            }
        },
        deleteEditingNode(state): void {
            const topIndex = state.search.nodes.length - 1;
            const index = webcrap.array.removeItem<SearchNode>(state.search.nodes, state.editNode);
            if (index === topIndex) {
                webcrap.array.removeItem<SearchEdge>(state.search.edges, state.search.edges[index - 1]);
            } else if (topIndex !== 0) {
                webcrap.array.removeItem<SearchEdge>(state.search.edges, state.search.edges[index]);
            }
            state.editNode = null;
            state.selectedItem = null;
        },
        setEditItem(state): void {
            if (state.selectedItem.type === "SearchNode") {
                state.editNode = state.selectedItem as SearchNode;
            } else {
                state.editEdge = state.selectedItem as SearchEdge;
            }
        },
        cancelItemEdit(state): void {
            state.editNode = null;
            state.editEdge = null;
        },
        //#end region



        //#region condition update mutations
        setEditCondition(state): void {
            if (state.selectedCondition.type === ConditionType.And || state.selectedCondition.type === ConditionType.Or) {
                state.editAndOrCondition = state.selectedCondition as AndOrCondition;
            } else {
                state.editValCondition = state.selectedCondition as ValueCondition;
            }
        },
        selectedCondition(state, value: Condition) {
            state.selectedCondition = value;
        },
        cancelCondEdit(state) {
            if (state.editValCondition === state.newCondition) {
                DeleteCondition(state.search.condition, state.newCondition);
            }
            state.editValCondition = null;
            state.newCondition = null;
        },
        cancelAndOrEdit(state) {
            if (state.editAndOrCondition === state.newCondition) {
                DeleteCondition(state.search.condition, state.newCondition);
            }
            state.editAndOrCondition = null;
            state.newCondition = null;
        },
        saveEditingAndOrCondition(state, newType: string) {
            state.editAndOrCondition.type = newType;
            state.editAndOrCondition = null;
            state.newCondition = null;
        },
        saveEditingCondition(state, condition: ValueCondition) {
            UpdateCondition(state.search.condition, state.editValCondition, condition);
            state.newCondition = null;
            state.editValCondition = null;
        },
        deleteEditingValueCondition(state) {
            DeleteCondition(state.search.condition, state.editValCondition);
            state.newCondition = null;
            state.editValCondition = null;
        },
        deleteEditingAndOrCondition(state) {
            if (state.search.condition === state.editAndOrCondition) {
                state.search.condition = new AndOrCondition(ConditionType.And);
            } else {
                DeleteCondition(state.search.condition, state.editAndOrCondition);
            }
            state.newCondition = null;
            state.editAndOrCondition = null;
        },
        deleteSelectedCondition(state) {
            if (state.search.condition === state.selectedCondition) {
                state.search.condition = new AndOrCondition(ConditionType.And);
            } else {
                DeleteCondition(state.search.condition, state.selectedCondition);
            }
        },
        includeDisabled(state, newState: boolean):void {
            state.search.includeDisabled = newState;
        },
        //#endregion


        //#region new conditions
        newCondition(state, newCondition: Condition) {
            let parent: AndOrCondition;
            if (state.newConditionParent === null) {
                parent = state.search.condition;
            } else {
                parent = state.newConditionParent;
            }

            state.newCondition = newCondition;
            webcrap.array.pushItem<Condition>(parent.conditions, newCondition);
            if (newCondition.type !== ConditionType.And && newCondition.type !== ConditionType.Or) {
                state.editValCondition = newCondition as ValueCondition;
            }
            state.newConditionParent = null;
        },
        newConditionParent(state, root: AndOrCondition) {
            if (state.search.condition === null) {
                state.search.condition = new AndOrCondition(ConditionType.And);
                state.newConditionParent = state.search.condition;
            } else if (root === null) {
                state.newConditionParent = state.search.condition;
            } else {
                state.newConditionParent = root;
            }
        },
        cancelNewCondition(state) {
            state.newConditionParent = null;
        },
        conditionDown(state) {
            moveCondition(true, state.search.condition, null, state.selectedCondition);
        },
        conditionUp(state) {
            moveCondition(false, state.search.condition, null, state.selectedCondition);
        },

        //#endregion

        //#region share mutations
        shareUrl(state, value) {
            state.shareUrl = value;
        },
        shareCypher(state, value) {
            state.shareCypher = value;
        }
    },
    actions: {
        search(context): void {
            context.commit('results', null);
            context.commit('setStatusMessage', i18nGetWord('Searching'));
            context.commit('setIsSearching', true);

            const postdata = JSON.stringify(context.state.search);
            //const postdata = context.state.search;

            const request: Request = {
                url: "/api/search/advancedsearch",
                data: postdata,
                postJson: true,
                successCallback: (data?: ResultSet) => {
                    if (data.nodes.length === 0) {
                        context.commit('setStatusMessage', i18nGetPhrase('Found_no_results'));
                    } else {
                        context.commit('clearStatusMessage');
                    }
                    context.commit('setIsSearching', false);
                    context.commit('results', data);
                },
                errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
                    context.commit('setStatusMessage', i18nGetWord('Error'));
                    context.commit('setIsSearching', false);
                    // eslint-disable-next-line
                    console.error(error);
                }
            };
            api.post(request);
        },
        simpleSearch(context, term): void {
            context.commit('results', null);
            context.commit('setIsSearching', true);
            context.commit('setStatusMessage', i18nGetWord('Searching'));

            const request: Request = {
                url: "/api/search/?searchterm=" + term,
                postJson: true,
                successCallback: (data?: ResultSet) => {
                    if (data.nodes.length === 0) {
                        context.commit('setStatusMessage', i18nGetPhrase('Found_no_results'));
                    } else {
                        context.commit('clearStatusMessage');
                    }
                    context.commit('setIsSearching', false);
                    context.commit('results', data);
                },
                errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
                    context.commit('setIsSearching', false);
                    context.commit('setStatusMessage', i18nGetWord('Error'));
                    // eslint-disable-next-line
                    console.error(error);
                }
            };
            api.get(request);
        },
        updateShare(context): void {
            const search = context.state.search;

            if (search.nodes.length > 0) {
                const encodedData = webcrap.misc.encodeUrlB64(JSON.stringify(search));
                context.commit("shareUrl", "?sharedsearch=" + encodedData);

                const url = "/api/search/advancedsearchcypher";
                const postdata = JSON.stringify(search);

                const newrequest: Request = {
                    url: url,
                    data: postdata,
                    postJson: true,
                    dataType: "text",
                    successCallback: data => {
                        context.commit("shareCypher", data);
                    },
                    errorCallback: (jqXHR: JQueryXHR, status: string, error: string) => {
                        // eslint-disable-next-line
                        console.error("Error retrieving cypher query: " + error);
                    },
                };
                api.post(newrequest);
            }
        }
    }
};
