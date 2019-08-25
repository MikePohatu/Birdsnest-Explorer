import $ from 'jquery';
import 'foundation-sites';
import * as log from 'loglevel';
import * as tableview from './reports/js/tableview';

log.setLevel('trace', false);

//$(document).foundation();

//temporary to dealing with legacy crap
global.tableview = tableview;
