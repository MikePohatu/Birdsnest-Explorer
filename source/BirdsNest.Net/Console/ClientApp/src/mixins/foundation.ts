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
import $ from 'jquery';
import { onMounted, onBeforeUnmount } from 'vue'

export function initFoundationMounted(elementRef) {
  onMounted(() => { 
    //console.log({function: "initFoundation-onMounted", el: elementRef.value});
    $(elementRef.value).foundation() }
  );
}

export function initFoundationMountedUnmounted(elementRef) {
  onMounted(() => {
    $(elementRef.value).foundation() 
  });

  onBeforeUnmount(()=> {
    $(elementRef.value).foundation('_destroy');
  });
}
