var webcrap;
(function (webcrap) {
    function isNullOrWhitespace(input) {
        if (typeof input === 'undefined' || input == null)
            return true;
        return input.replace(/\s/g, '').length < 1;
    }
    webcrap.isNullOrWhitespace = isNullOrWhitespace;
    class DomCrap {
        ClearOptions(selectboxEl) {
            selectboxEl.options.length = 0;
        }
        AddOption(selectbox, text, value) {
            var o = document.createElement("OPTION");
            o.text = text;
            o.value = value;
            selectbox.options.add(o);
            return o;
        }
    }
    webcrap.DomCrap = DomCrap;
    webcrap.dom = new DomCrap();
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
            var token = $("[name='__RequestVerificationToken']").val();
            var u = $('#username').val();
            var p = $('#pw').val();
            var prov = $('#provider').val();
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
            $.ajax(url, {
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
                        if (callback) {
                            callback();
                        }
                    }
                }
            });
        }
    }
    webcrap.AuthCrap = AuthCrap;
    webcrap.auth = new AuthCrap();
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
                        webcrap.auth.showLoginCurtain(function () {
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
                        webcrap.auth.showLoginCurtain(function () {
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
    webcrap.DataCrap = DataCrap;
    webcrap.data = new DataCrap();
})(webcrap || (webcrap = {}));
//# sourceMappingURL=webcrap.js.map