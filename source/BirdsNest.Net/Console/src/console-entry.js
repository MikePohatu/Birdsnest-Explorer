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
//
import $ from 'jquery';

//import '@fortawesome/fontawesome-pro/js/fontawesome';
//import '@fortawesome/fontawesome-pro/js/solid';
//import '@fortawesome/fontawesome-pro/js/regular';
//import '@fortawesome/fontawesome-pro/js/light';

import '@fortawesome/fontawesome-free/js/fontawesome';
import '@fortawesome/fontawesome-free/js/solid';
import '@fortawesome/fontawesome-free/js/regular';

import 'foundation-sites';

import 'jquery-ui/themes/base/core.css';
import 'jquery-ui/themes/base/selectmenu.css';
import 'jquery-ui/themes/base/menu.css';
import 'jquery-ui/themes/base/autocomplete.css';
import 'jquery-ui/themes/base/theme.css';

import 'motion-ui/dist/motion-ui.css';
import 'foundation-sites/dist/css/foundation.css';

import './shared/css/app.css';
import './shared/css/overrides.css';

$(document).foundation();

if (!PRODUCTION) {
    console.log("Birdsnest Console loaded. Mode: " + ENV);
}

//set the mutateApproach to sync. If we do async we can have out of order issues because we might resize the icon
//after adding it. We want to replace with svg first, then resize. 
//https://fontawesome.com/how-to-use/with-the-api/setup/configuration
window.FontAwesome.config.mutateApproach = 'sync';
