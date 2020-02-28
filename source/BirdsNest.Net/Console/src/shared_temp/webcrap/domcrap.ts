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
class DomCrap {
    ClearOptions(selectboxEl) {
        selectboxEl.options.length = 0;
    }

    AddOption(selectbox, text, value) {
        var o: HTMLOptionElement = <HTMLOptionElement>document.createElement("OPTION");
        o.text = text;
        o.value = value;
        selectbox.options.add(o);
        return o;
	}

	//Bind the enter key for an element to click a button
	BindEnterToButton(elementid: string, buttonid: string, callback?) {
		document.getElementById(elementid).addEventListener("keydown", function (event) {
			//console.log("keydown listener fired: " + elementid);
			// Number 13 is the "Enter" key on the keyboard
			if (event.keyCode === 13) {
				event.preventDefault();
				document.getElementById(buttonid).click();
				if (typeof callback === "function") {
					callback();
				}
			}
		}
	);
}
}

var domcrap = new DomCrap();
export { domcrap, DomCrap };






