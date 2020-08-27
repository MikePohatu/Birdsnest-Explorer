// Copyright (c) 2019-2020 "20Road"
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
class ArrayCrap {

    pushItem<T>(arr: T[], item: T): void {
        arr.push(item);
    }

    replaceItem<T>(arr: T[], oldItem: T, newItem: T): boolean {
        for (let i = 0; i < arr.length; i++) {
            if (arr[i] === oldItem) {
                arr.splice(i, 1, newItem);
                return true;
            }
        }
        return false;
    }

    removeItem<T>(arr: T[], item: T): number {
        for (let i = 0; i < arr.length; i++) {
            if (arr[i] === item) {
                arr.splice(i, 1);
                return i;
            }
        }
        return -1;
    }

    moveItemLeft<T>(arr: T[], item: T): boolean {
        for (let i = 0; i < arr.length; i++) {
            if (arr[i] === item) {
                if (i === 0) {
                    return false; //can't move any further
                }
                else {
                    arr.splice(i, 1, arr[i - 1]);
                    arr.splice(i - 1, 1, item);
                    return true;
                }
            }
        }
        return false;
    }

    moveItemRight<T>(arr: T[], item: T): boolean {
        for (let i = 0; i < arr.length; i++) {
            if (arr[i] === item) {
                if (i === arr.length - 1) {
                    return false; //can't move any further
                }
                else {
                    arr.splice(i, 1, arr[i + 1]);
                    arr.splice(i + 1, 1, item);
                    return true;
                }
            }
        }
        return false;
    }
}

export const array = new ArrayCrap();