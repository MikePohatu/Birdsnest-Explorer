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
}

var domcrap = new DomCrap();
export { domcrap, DomCrap };






