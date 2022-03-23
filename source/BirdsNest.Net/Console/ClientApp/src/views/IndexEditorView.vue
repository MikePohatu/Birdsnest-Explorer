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
	<div class="page" id="indexEditor">
		<h6>{{ $t('phrase_Indexes_by_Plugin') }}</h6>
		<Loading v-if="!statsDataReady" />
		<div v-else>
			<ul class="accordion" data-accordion data-allow-all-closed="true" data-multi-expand="true">
				<li
					class="accordion-item"
					data-accordion-item
					v-for="(plugin, pluginname) in pluginManager.plugins"
					:key="pluginname"
				>
					<a href="#" class="pluginHeader accordion-title">{{ plugin.displayName }}</a>
					<div v-if="pluginHasProperties(plugin)===false" class="accordion-content" data-tab-content>
						<p class="noPropsMessage">{{ $t('index_editor.plugin_no_datatypes') }}</p>
					</div>
					<div v-else class="accordion-content" data-tab-content>
						<table class="hover">
							<thead>
								<tr>
									<th>{{ $tc('word_Type') }}</th>
									<th>{{ $t('word_Property') }}</th>
									<!-- <th>Index Name</th> -->
									<th></th>
								</tr>
							</thead>

							<tbody v-for="(datatype, label) in plugin.nodeDataTypes" :key="label">
								<tr v-for="(property, propname) in datatype.properties" :key="propname">
									<td>{{ label }}</td>
									<td>{{ propname }}</td>
									<!-- <td>{{ index.indexName }}</td> -->
									<!-- <td>{{ index.state }}</td> -->
									<td>
										<div v-if="propertyHasIndex(label, propname)">
											<span
												v-if="propertyHasConstraint(label, propname)"
												class="inactive"
												:title="$t('index_editor.constraints_not_supported')"
											>{{ $t('word_Constraint') }}</span>
											<span
												v-else-if="propertyIndexIsEnforced(label, propname)"
												class="inactive delete"
												:title="$t('index_editor.index_enforced')"
											>{{ $t('word_Delete') }}</span>
											<a v-else v-on:click="onDeleteIndexClicked(label, propname)" class="delete">{{ $t('word_Delete')}}</a>
										</div>
										<div v-else>
											<a
												v-if="propertyIndexIsEnforced(label, propname)"
												v-on:click="onCreateIndexClicked(label, propname)"
												class="create warning"
												:title="$t('index_editor.enforced_missing')"
											>{{ $t('word_Create') }}</a>
											<a v-else v-on:click="onCreateIndexClicked(label, propname)" class="create">{{ $t('word_Create') }}</a>
										</div>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
				</li>
			</ul>
		</div>
	</div>
</template>

<style scoped>
h6 {
	font-weight: bold;
}

#indexEditor {
	max-width: 1024px;
	margin-left: auto;
	margin-right: auto;
}

.pluginHeader {
	padding: 0.5rem 0.5rem 0.625rem;
	font-size: 0.9rem;
	font-weight: bold;
}

.delete {
	color: red;
}

.create {
	color: green;
}

.create.warning {
	color: orange;
	font-weight: bold;
}

.inactive {
	color: gray;
}

.delete.inactive {
	color: pink;
}

tr, .noPropsMessage {
	font-size: 0.8rem;
}

td {
	width: min-content;
}

p {
	margin: 0;
}
</style>

<script lang="ts">
import { bus, events } from "@/bus";
import Loading from "@/components/Loading.vue";
import { Index } from "@/assets/ts/dataMap/indexes/Index";
import { api, Request } from "@/assets/ts/webcrap/apicrap";
import { Dictionary } from "lodash";
import PluginManager from "@/assets/ts/dataMap/PluginManager";
import { Plugin } from "@/assets/ts/dataMap/Plugin";
import ServerInfo from "@/assets/ts/dataMap/ServerInfo";
import { DataType } from "@/assets/ts/dataMap/DataType";
import { Property } from "@/assets/ts/dataMap/Property";
import { rootPaths } from "@/store/index";
import $ from "jquery";
import "foundation-sites";
import { defineComponent } from "vue";

export default defineComponent({
	components: {
		Loading
	},
	data() {
		let indexes: Dictionary<Index[]> = null;
		return {
			indexes
		};	
	},
	computed: {
		apiState (): number {
			return this.$store.state.apiState;
		},

		pluginManager (): PluginManager {
			return this.$store.state.pluginManager;
		},

		serverInfoState (): number {
			return this.$store.state.serverInfoState;
		},

		serverInfo (): ServerInfo {
			return this.$store.state.serverInfo;
		},

		statsDataReady (): boolean {
			return this.serverInfo !== null && this.pluginManager !== null;
		}
	},
	methods: {
		pluginHasProperties (plugin: Plugin) {
			const dataTypeNames = Object.keys(plugin.nodeDataTypes);
			for (let i = 0; i < dataTypeNames.length; i++) {
				const datatype = plugin.nodeDataTypes[dataTypeNames[i]];
				if (datatype.propertyNames.length > 0) {
					return true;
				}
			}

			return false;
		},

		propertyHasIndex(label: string, propertyName: string): boolean  {
			if (Object.prototype.hasOwnProperty.call(this.serverInfo.indexes, label)) {
				if (Object.prototype.hasOwnProperty.call(this.serverInfo.indexes[label], propertyName)) {
					return true;
				}
			}
			return false;
		},

		propertyHasConstraint(label: string, propertyName: string): boolean {
			if (Object.prototype.hasOwnProperty.call(this.serverInfo.indexes, label)) {
				const indexes = this.serverInfo.indexes[label];

				if (Object.prototype.hasOwnProperty.call(indexes, propertyName)) {
					const index: Index = indexes[propertyName];
					if (index.isConstraint) {
						return true;
					}
				}
			}
			return false;
		},

		propertyIndexIsEnforced(label: string, propertyName: string): boolean {
			if (Object.prototype.hasOwnProperty.call(this.pluginManager.nodeDataTypes, label)) {
				const datatype: DataType = this.pluginManager.nodeDataTypes[label];

				if (Object.prototype.hasOwnProperty.call(datatype.properties, propertyName)) {
					const propertydef: Property = datatype.properties[propertyName];
					if (propertydef.indexEnforced) {
						return true;
					}
				}
			}

			if (Object.prototype.hasOwnProperty.call(this.pluginManager.edgeDataTypes, label)) {
				const datatype: DataType = this.pluginManager.edgeDataTypes[label];

				if (Object.prototype.hasOwnProperty.call(datatype.properties, propertyName)) {
					const propertydef: Property = datatype.properties[propertyName];
					if (propertydef.indexEnforced) {
						return true;
					}
				}
			}
			return false;
		},

		onDeleteIndexClicked(label: string, property: string): void {
			const message = this.$t('index_editor.confirm_index_delete', {label: label, property: property}).toString();
			if (confirm(message)) {
				const indexes: Dictionary<Index> = this.serverInfo.indexes[label];
				const index = indexes[property];
				const postdata = JSON.stringify(index);

				const request: Request = {
					url: "/api/admin/indexes/drop",
					data: postdata,
					postJson: true,
					successCallback: () => {
						this.$store.dispatch(rootPaths.actions.UPDATE_SERVER_INFO);
					},
					errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
						// eslint-disable-next-line
						console.error(error);
						bus.$emit(events.Notifications.Error, this.$t('index_editor.error_delete', {label: label, property: property}).toString() + "\n" + error);
					},
				};

				bus.$emit(events.Notifications.Processing, this.$t('word_Processing'));
				api.post(request);
			}
		},

		onCreateIndexClicked(label, property) {
			const message = this.$t('index_editor.confirm_index_create', {label: label, property: property}).toString();
			if (confirm(message)) {
				const postdata = JSON.stringify({ label: label, property: property });

				const request: Request = {
					url: "/api/admin/indexes/create",
					data: postdata,
					postJson: true,
					successCallback: () => {
						this.$store.dispatch(rootPaths.actions.UPDATE_SERVER_INFO);
					},
					errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
						// eslint-disable-next-line
						console.error(error);
						bus.$emit(events.Notifications.Error, this.$t('index_editor.error_create', {label: label, property:property}) + "\n" + error);
					},
				};

				bus.$emit(events.Notifications.Processing, this.$t('word_Processing'));
				api.post(request);
			}
		}
	},
	mounted() {
		if (this.statsDataReady) {
			//double requestAnimationFrame required primarily for IE
			requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					$(this.$el).foundation();
				});
			});
		} else {
			const unwatch = this.$store.watch(
				() => {
					return this.$store.state.serverInfo;
				},
				() => {
					requestAnimationFrame(() => {
						requestAnimationFrame(() => {
							unwatch();
							$(this.$el).foundation();
						});
					});
				}
			);
		}
	}
})
</script>