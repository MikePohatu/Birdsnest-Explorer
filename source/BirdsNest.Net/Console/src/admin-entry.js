import { webcrap } from "./shared/webcrap/webcrap";

document.getElementById("updatePropsGoBtn").addEventListener("click", UpdateProperties);
document.getElementById("pluginReloadBtn").addEventListener("click", ReloadPlugins);

function ReloadPlugins() {
    webcrap.data.apiGetJson("/admin/reloadplugins", function (data) {
        //console.log(data);
        document.getElementById("reloadmessage").innerHTML = data.message;
    });
}

function UpdateProperties() {
    var label = document.getElementById("label").value;
    //console.log(label);

    webcrap.data.apiGetJson("/admin/updateproperties?label=" + label, function (data) {
        //console.log(data);
        document.getElementById("reloadmessage").innerHTML = data.message;
    });
}