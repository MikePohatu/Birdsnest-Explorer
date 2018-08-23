$(document).foundation();

function showLoginCurtain() {
    var url = "/Account/LogonForm";
    $.ajax({
        dataType: "html",
        url: url,
        success: function (data) {
            document.getElementById("curtainoverlay").innerHTML = data;
            $("#loginForm").submit(processCurtainLogin);
            $("#curtain").slideToggle();
            $("#curtainoverlaywrapper").slideToggle();
        }
    }); 
}

function processCurtainLogin () {
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
            if (data) {
                document.getElementById("curtainoverlay").innerHTML = data;
                $("#loginForm").submit(processCurtainLogin);
            }
            else {
                $("#curtain").slideToggle();
                $("#curtainoverlaywrapper").slideToggle();
            }
        }
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
