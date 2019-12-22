//import "core-js/stable";
//import "regenerator-runtime/runtime";

if (!Element.prototype.matches) {
    Element.prototype.matches = Element.prototype.msMatchesSelector ||
      Element.prototype.webkitMatchesSelector;
}