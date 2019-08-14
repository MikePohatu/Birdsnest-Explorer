var DomCrap = /** @class */ (function () {
    function DomCrap() {
    }
    DomCrap.prototype.ClearOptions = function (selectboxEl) {
        selectboxEl.options.length = 0;
    };
    DomCrap.prototype.AddOption = function (selectbox, text, value) {
        var o = document.createElement("OPTION");
        o.text = text;
        o.value = value;
        selectbox.options.add(o);
        return o;
    };
    return DomCrap;
}());
var domcrap = new DomCrap();
//# sourceMappingURL=domcrap.js.map