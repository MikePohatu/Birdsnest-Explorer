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
