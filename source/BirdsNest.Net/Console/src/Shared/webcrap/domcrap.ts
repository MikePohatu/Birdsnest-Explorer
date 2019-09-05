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






