///// <reference path ="jquery.js"/>

//class DataCrap {
//    apiGetJson(url, callback) {
//        this.apiGet(url, "json", callback);
//    }

//    apiGet(url, datatype, callback) {
//        $.ajax({
//            dataType: datatype,
//            url: url,
//            statusCode: {
//                401: function () {
//                    //console.log("401-unauthorized");
//                    //$("#curtain").slideToggle();
//                    showLoginCurtain(function () {
//                        this.apiGet(url, datatype, callback);
//                    });
//                }
//            },
//            success: callback
//        });
//    }

//    apiPostJson(url, postdata, callback) {
//        this.apiPost(url, postdata, "application/json; charset=utf-8", callback);
//    }

//    apiPost(url, postdata, contenttype, callback) {
//        $.ajax({
//            url: url,
//            method: "POST",
//            data: postdata,
//            contentType: contenttype,
//            headers: {
//                'X-CSRFToken': this.getCookie('csrftoken')
//            },
//            statusCode: {
//                401: function () {
//                    showLoginCurtain(function () {
//                        this.apiPost(url, postdata, contenttype, callback);
//                    });
//                }
//            },
//            success: callback
//        });
//    }

//    //getCookie function from django documentation
//    getCookie(name) {
//        var cookieValue = null;
//        if (document.cookie && document.cookie !== '') {
//            var cookies = document.cookie.split(';');
//            for (var i = 0; i < cookies.length; i++) {
//                var cookie = jQuery.trim(cookies[i]);
//                // Does this cookie string begin with the name we want?
//                if (cookie.substring(0, name.length + 1) === (name + '=')) {
//                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
//                    break;
//                }
//            }
//        }
//        return cookieValue;
//    }
//    }
//}