var Crap = /** @class */ (function () {
    function Crap() {
    }
    Crap.prototype.isNullOrWhitespace = function (input) {
        if (typeof input === 'undefined' || input == null)
            return true;
        return input.replace(/\s/g, '').length < 1;
    };
    return Crap;
}());
var crap = new Crap();
//# sourceMappingURL=crap.js.map