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
import { bus, events } from "@/bus";
import { store, rootPaths } from "@/store";
import { auth } from "./authcrap";
import $ from 'jquery';

class ApiCrap {
  post(details: Request) {
    auth.getValidationToken((token) => {
      let headers;
      if (details.headers) { headers = details.headers; }
      else { headers = { 'RequestVerificationToken': token }; }

      if (details.postJson) { details.contentType = "application/json; charset=utf-8"; }

      $.ajax(details.url, {
        dataType: details.dataType,
        method: "POST",
        contentType: details.contentType,
        data: details.data,
        headers: headers,
        statusCode: {
          401: function () {
            store.commit(rootPaths.mutations.DEAUTH);
          },
          403: function () {
            bus.emit(events.Notifications.Error, "Access forbidden");
          },
        },
        success: function (data?, status?: string, jqXHR?: JQueryXHR) {
          typeof details.successCallback === 'function' && details.successCallback(data, status, jqXHR);
        },
        error: function (jqXHR?: JQueryXHR, status?: string, error?: string) {
          typeof details.errorCallback === 'function' && details.errorCallback(jqXHR, status, error);
        }
      });
    });
  }

  get(details: Request) {
    $.ajax(
      details.url,
      {
        dataType: details.dataType,
        method: "GET",
        headers: details.headers,
        statusCode: {
          401: function () {
            store.commit(rootPaths.mutations.DEAUTH);
          },
          403: function () {
            bus.emit(events.Notifications.Error, "Access forbidden");
          }
        },
        success: function (data?, status?: string, jqXHR?: JQueryXHR) {
          typeof details.successCallback === 'function' && details.successCallback(data, status, jqXHR);
        },
        error: function (jqXHR?: JQueryXHR, status?: string, error?: string) {
          typeof details.errorCallback === 'function' && details.errorCallback(jqXHR, status, error);
        }
      });
  }

  states = {
    NOTAUTHORIZED: -1,
    INIT: 0,
    LOADING: 1,
    ERROR: 2,
    READY: 3
  }
}

export class Request {
  url: string;
  data?: object | string = null;
  // eslint-disable-next-line
  successCallback?: (data?: any | string, status?: string, jqXHR?: JQueryXHR) => void;
  errorCallback?: (jqXHR?: JQueryXHR, status?: string, error?: string) => void;
  headers?: JQuery.PlainObject;
  contentType?: string = "application/x-www-form-urlencoded; charset=UTF-8";
  dataType?: string = "json";
  postJson?: boolean = false;
}

export const api = new ApiCrap();

