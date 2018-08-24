$(document).foundation();

function showLoginCurtain(callback) {
    var url = "/Account/LogonForm";
    $.ajax({
        dataType: "html",
        url: url,
        success: function (data) {
            document.getElementById("curtainoverlay").innerHTML = data;
            $("#loginForm").submit(function () {
                processCurtainLogin(callback);
            });
            $("#curtain").slideToggle();
            $("#curtainoverlaywrapper").slideToggle();
        }
    }); 
}

function processCurtainLogin (callback) {
    //console.log(data);   
    event.preventDefault();
    var url = "/Account/AjaxLogin";
    var token = $("[name='__RequestVerificationToken']").val();
    var u = $('#username').val();
    var p = $('#pw').val();
    var postdata = {
        UserName: u,
        Password: p
    };

    //console.log(token);
    //console.log(url);
    $.ajax({
        url: url,
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
                    processCurtainLogin(callback);
                });
            }
            else {
                $("#curtain").slideToggle();
                $("#curtainoverlaywrapper").slideToggle();
                //console.log("curtainhide, run callback");
                if (callback) { callback(); }
            }
        }
    });
}


function apiGetJson(url, callback) {
    apiGet(url, "json", callback);
}

function apiGet(url, datatype, callback) {
    $.ajax({
        dataType: datatype,
        url: url,
        statusCode: {
            401: function () {
                //console.log("401-unauthorized");
                //$("#curtain").slideToggle();
                showLoginCurtain(function () {
                    apiGet(url, datatype, callback);
                });
            }
        },
        success: callback
    });
}

function apiPostJson(url, postdata, callback) {
    apiPost(url, postdata, "application/json; charset=utf-8", callback);
}

function apiPost(url, postdata, contenttype, callback) {
    $.ajax({
        url: url,
        method: "POST",
        data: postdata,
        contentType: contenttype,
        headers: {
            'X-CSRFToken': getCookie('csrftoken')
        },
        statusCode: {
            401: function () {
                showLoginCurtain(function () {
                    apiPost(url, postdata, contenttype, callback);
                });
            }
        },
        success: callback
    });
}

//getCookie function from django documentation
function getCookie(name) {
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
