﻿// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
import { v4 as uuidv4 } from 'uuid';

class MiscCrap {
    isNullOrWhitespace(input: string) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    }

    isNullOrEmpty(input: string) {
        return input === null || input === "";
    }

    capitalize(s: string) {
        if (typeof s !== 'string') return ''
        return s.charAt(0).toUpperCase() + s.slice(1);
    }

    //decode string from encodeUrlB64 function
    decodeUrlB64(input: string): string {
        const str = decodeURIComponent(input);
        return atob(str);
    }

    //encode to base64 and format for url e.g. query sting
    encodeUrlB64(input: string): string {
        const str = btoa(input);
        return encodeURIComponent(str);
    }

    //return a function that can be called multiple times, and will only run the
    //callback after no duplicate calls for the specified delay period.
    debounce(callback: Function, delay: number): () => void {
        let timer;

        return function () {
            clearTimeout(timer);
            timer = setTimeout(callback, delay);
        }
    }

    generateUID() {
        return uuidv4();
    }

    download(content, fileName, mimeType) {
        if (!mimeType) {
            mimeType = "application/octet-stream";
        }
        const blob = new Blob([content], {
            type: mimeType,
        });

        //IE
        if (navigator.msSaveBlob) {
            navigator.msSaveBlob(blob, fileName);
        }
        //HTML5
        else {
            const downel = document.createElement("a");
            downel.href = URL.createObjectURL(blob);
            downel.setAttribute("download", fileName);
            downel.click();
            downel.remove();
        }
    }

    isIE(): boolean {
        return window.navigator.userAgent.match(/(MSIE|Trident)/) !== null;
    }

    //strip css identifier tags from a string
    cleanCssClassName(className: string): string {
        return className.replace(".", "-").replace("#", "-").replace(":", "-").replace(" ", "-").replace(">", "-").replace("+", "-").replace(",", "-")
            .replace("|=", "-").replace("^=", "-").replace("$=", "-").replace("*=", "-").replace("=", "-").replace("*", "-").replace("~", "-");
    }
}

export default new MiscCrap();





