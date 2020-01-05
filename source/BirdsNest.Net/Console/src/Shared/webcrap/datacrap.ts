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
import { authcrap } from "./authcrap";
import * as $ from 'jquery';

class DataCrap {
    apiGetJson(url, callback) {
        this.apiGet(url, "json", callback);
    }

    apiGet(url, datatype, callback) {
        $.ajax({
            dataType: datatype,
            url: url,
            statusCode: {
                401: function () {
                    //console.log("401-unauthorized");
                    //$("#curtain").slideToggle();
                    authcrap.showLoginCurtain(function () {
                        this.apiGet(url, datatype, callback);
                    });
                }
            },
            success: callback
        });
    }

    apiPostJson(url, postdata, callback) {
        this.apiPost(url, postdata, "application/json; charset=utf-8", callback);
    }

    apiPost(url, postdata, contenttype, callback) {
        $.ajax({
            url: url,
            method: "POST",
            data: postdata,
            contentType: contenttype,
            headers: {
                'X-CSRFToken': this.getCookie('csrftoken')
            },
            statusCode: {
                401: function () {
                    authcrap.showLoginCurtain(function () {
                        this.apiPost(url, postdata, contenttype, callback);
                    });
                }
            },
            success: callback
        });
    }

    //getCookie function from django documentation
    getCookie(name) {
        var cookieValue = null;
        if (document.cookie && document.cookie !== '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                // Does this cookie string begin with the name we want?
                if (cookie.substring(0, name.length + 1) === (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
}
var datacrap = new DataCrap();

export { datacrap, DataCrap };