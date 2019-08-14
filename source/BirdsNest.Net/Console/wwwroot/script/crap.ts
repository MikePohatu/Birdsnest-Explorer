class Crap {
    isNullOrWhitespace(input) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    }
}

var crap = new Crap();