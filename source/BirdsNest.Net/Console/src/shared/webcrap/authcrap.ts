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
import * as $ from 'jquery';

class AuthCrap {
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


    showCurtain() {
        //console.log("showCurtain: " + $('#curtain').css('display'));
        if ($('#curtain').css('display') === "none") {
            //console.log("toggle");
            $("#curtain").slideToggle();
            $("#curtainoverlay").slideToggle();
        }
    }

    hideCurtain() {
        //console.log("hideCurtain: " + $('#curtain').css('display'));

        if ($('#curtain').css('display') !== "none") {
            //console.log("toggle");
            $("#curtain").slideToggle();
            $("#curtainoverlay").slideToggle();
        }
    }

    showLoginCurtain(callback) {
        var url = "/Account/LogonExpiredForm";
        var me = this;
        $.ajax({
            dataType: "html",
            url: url,
            success: function (data) {
                document.getElementById("curtainoverlay").innerHTML = data;
                $("#loginForm").submit(function () {
                    me.processCurtainLogin(callback);
                });
                me.showCurtain();
                $("#username").focus();
            }
        });
    }

    processCurtainLogin(callback) {
        //console.log(data);   
        event.preventDefault();
        var url = "/Account/PartialLogin";
        var token: string = $("[name='__RequestVerificationToken']").val() as string;
        var u: string = $('#username').val() as string;
        var p: string = $('#pw').val() as string;
        var prov: string = $('#provider').val() as string;
        var me = this;
        //console.log(prov);
        var postdata = {
            UserName: u,
            Password: p,
            Provider: prov
        };

        //console.log(postdata);
        //console.log(token);
        //console.log(url);
        $.ajax(
            url,
            {
                method: "POST",
                data: postdata,
                headers: {
                    'RequestVerificationToken': token
                },
                success: function (data) {
                    //console.log(data);
                    //if data comes back, display it on the curtain, otherwise hide the curtain
                    if (data) {
                        document.getElementById("curtainoverlay").innerHTML = data;
                        $("#loginForm").submit(function () {
                            me.processCurtainLogin(callback);
                        });
                    }
                    else {
                        me.hideCurtain();
                        if (callback) { callback(); }
                    }
                }
            });
    }
}
var authcrap = new AuthCrap();

export { authcrap, AuthCrap };