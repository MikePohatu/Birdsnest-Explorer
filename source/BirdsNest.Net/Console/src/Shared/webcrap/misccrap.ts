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

    //https://stackoverflow.com/a/6248722
    generateUID():string {
        // I generate the UID from two parts here 
        // to ensure the random number provide enough bits.
        var firstPart: number = (Math.random() * 46656) | 0;
        var secondPart: number = (Math.random() * 46656) | 0;
        return ("000" + firstPart.toString(36)).slice(-3) + ("000" + secondPart.toString(36)).slice(-3);
    }

}

var misccrap = new MiscCrap();

export { misccrap, MiscCrap };




