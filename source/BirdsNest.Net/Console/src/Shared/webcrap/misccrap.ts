class MiscCrap {
    isNullOrWhitespace(input: string ) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    }

    isNullOrEmpty(input:string ) {
        return input === null || input === "";
    }

    //decode string from encodeUrlB64 function
    decodeUrlB64(input: string): string {
        var str = decodeURIComponent(input);
        return atob(str);
    }

    //encode to base64 and format for url e.g. query sting
    encodeUrlB64(input: string): string {
        var str = btoa(input);
        return encodeURIComponent(str);
    }
}

var misccrap = new MiscCrap();

export { misccrap, MiscCrap };




