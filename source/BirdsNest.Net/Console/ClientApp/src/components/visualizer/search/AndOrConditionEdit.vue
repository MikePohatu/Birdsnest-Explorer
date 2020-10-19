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
	<div class="dialogWrapper">
		<div id="andOrEdit" class="dialog">
			<fieldset class="fieldset">
				<legend>{{ $t('phrase_And_Or') }}</legend>
				<div class="input-group">
					<span class="input-group-label small-4">{{ $tc('word_Type') }}</span>
					<select v-model="type" class="small-8 input-group-field">
						<option value="AND">{{ $t('word_AND') }}</option>
						<option value="OR">{{ $t('word_OR') }}</option>
					</select>
				</div>

				<button
					v-on:click="onCloseClicked"
					class="close-button"
					data-close
					:aria-label="$t('phrase_Close_condition_dialog')"
					type="button"
				>
					<span aria-hidden="true">&times;</span>
				</button>
				<div class="align-middle">
					<div>
						<button
							id="searchAndOrSaveBtn"
							class="button searchbutton-wide small"
							:aria-label="$t('phrase_Save_this_condition')"
							type="button"
							v-on:click="onSaveClicked"
						>{{ $t('word_Save') }}</button>
						<button
							id="searchAndOrDeleteBtn"
							class="alert searchbutton-wide button small"
							:aria-label="$t('phrase_Delete_this_condition')"
							type="button"
							v-on:click="onDeleteClicked"
						>{{ $t('word_Delete') }}</button>
					</div>
				</div>
			</fieldset>
		</div>
	</div>
</template>


<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { AndOrCondition } from "@/assets/ts/visualizer/Search";
import { SearchStorePaths } from "@/store/modules/SearchStore";

@Component
export default class AndOrConditionEdit extends Vue {
	@Prop({ type: Object as () => AndOrCondition, required: true })
	source: AndOrCondition;
	type = "AND";
	
	created(): void {
		this.type = this.source.type;
	}	

	onSaveClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Save.EDIT_ANDOR_CONDITION, this.type);
	}

	onDeleteClicked(): void {
		if (confirm(this.$t('visualizer.search.confirm_andor_condition_delete').toString())) {
			this.$store.commit(SearchStorePaths.mutations.Delete.EDIT_ANDOR_CONDITION);
		}
	}

	onCloseClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.CANCEL_ANDOR_EDIT);
	}
}
</script>