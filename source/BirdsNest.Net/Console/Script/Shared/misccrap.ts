class MiscCrap {
    isNullOrWhitespace(input) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    }
}

var misccrap = new MiscCrap();

export { misccrap, MiscCrap };




