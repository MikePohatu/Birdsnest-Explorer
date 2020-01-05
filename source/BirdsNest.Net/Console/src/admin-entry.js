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