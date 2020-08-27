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
	<div
		:class="[isSelected ? 'selected' : '', 'advsearchbutton clickable noselect']"
		v-on:click.stop="onClicked"
		v-on:dblclick.stop="onDblClicked"
	>
		<div>{{condition.name}}.{{condition.property}}</div>
		<div>{{ searchDeets }}</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { ValueCondition, ConditionType } from "@/assets/ts/visualizer/Search";

import { SearchStorePaths } from "@/store/modules/SearchStore";

@Component
export default class ValueConditionIcon extends Vue {
	@Prop({ type: Object as () => ValueCondition, required: true })
	condition: ValueCondition;

	get isSelected(): boolean {
		return this.$store.state.visualizer.search.selectedCondition === this.condition;
	}

	get searchDeets(): string {
		return (this.condition.not ? "Not " : "") + this.condition.operator + " " + this.condition.value + (this.condition.type === ConditionType.String && this.condition.caseSensitive ? "*" : "");
	}

	onClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, this.condition);
	}

	onDblClicked(): void {
		this.$store.commit(SearchStorePaths.mutations.Update.SELECTED_CONDITION, this.condition);
		this.$store.commit(SearchStorePaths.mutations.Update.EDIT_CONDITION);
	}
}
</script>