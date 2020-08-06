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
import store, { rootPaths } from "@/store";
import { api, Request } from "./apicrap";
import { bus, events } from "@/bus";

class AuthResults {
    isAuthenticated = false;
    isAuthorized = false;
    isAdmin = false;
    name = "";
    message = "";
}

class AuthCrap {
    authMessage = "";

    login(username: string, password: string, provider: string, successcallback?: () => void) {
        const logindetails = {
            Username: username,
            Password: password,
            Provider: provider
        }

        bus.$emit(events.Notifications.Processing, "Logging in");
        const request: Request = {
            url: "/api/account/login",
            data: logindetails,
            successCallback: (data: AuthResults) => {
                store.commit(rootPaths.mutations.IS_AUTHORIZED, data.isAuthorized);
                store.commit(rootPaths.mutations.IS_ADMIN, data.isAdmin);
                store.commit(rootPaths.mutations.USERNAME, data.name);
                bus.$emit(events.Auth.Message, data.message);

                if (data.isAuthorized) {
                    store.dispatch(rootPaths.actions.UPDATE_AUTHENTICATED_DATA);
                    typeof successcallback === 'function' && successcallback();
                }
                bus.$emit(events.Notifications.Clear);
            },
            errorCallback: (data: JQueryXHR, status: string, error: string) => {
                store.commit(rootPaths.mutations.DEAUTH);
                bus.$emit(events.Auth.Message, error);
                bus.$emit(events.Notifications.Error, "Failed to login: " + error);
            }
        };
        api.post(request);
    }

    getValidationToken(successcallback?: (token?) => void) {
        const request: Request = {
            url: "/api/xsrftoken",
            successCallback: (data) => {
                typeof successcallback === 'function' && successcallback(data.token);
            },
            errorCallback: (jqXHR, status, error) => {
                store.commit(rootPaths.mutations.IS_AUTHORIZED, false);
                store.commit(rootPaths.mutations.SESSION_STATUS, "Error");
                bus.$emit(events.Notifications.Error, "Error downloading validation token: " + error);
            }
        };
        api.get(request);
    }

    logout(callback?: () => void) {
        const request: Request = {
            url: "/api/account/logout",
            successCallback: () => {
                store.commit(rootPaths.mutations.DEAUTH);
                typeof callback === 'function' && callback();
            },
            errorCallback: (data: JQueryXHR, status: string, error: string) => {
                bus.$emit(events.Auth.Message, "Error logging out: " + error);
                store.commit(rootPaths.mutations.SESSION_STATUS, "Error logging out");
                bus.$emit(events.Notifications.Error, "Error logging out: " + error);
            }
        };
        api.get(request);
    }

    ping(callback?: () => void) {
        const request: Request = {
            url: "/api/account/ping",
            successCallback: (data: AuthResults) => {
                store.commit(rootPaths.mutations.IS_AUTHORIZED, data.isAuthorized);
                store.commit(rootPaths.mutations.IS_ADMIN, data.isAdmin);
                store.commit(rootPaths.mutations.USERNAME, data.name);
                bus.$emit(events.Auth.Message, data.message);
                
                if (data.isAuthorized) { 
                    store.commit(rootPaths.mutations.SESSION_STATUS, 'Authorized');
                    store.dispatch(rootPaths.actions.UPDATE_AUTHENTICATED_DATA); 
                }
                typeof callback === 'function' && callback();
            },
            errorCallback: (jqXHR?: JQueryXHR, status?: string, error?: string) => {
                store.commit(rootPaths.mutations.DEAUTH);
                store.commit(rootPaths.mutations.SESSION_STATUS, status);
                bus.$emit(events.Auth.Message, error);
                bus.$emit(events.Notifications.Error, "Pinging server: " + error);
                typeof callback === 'function' && callback();
            }
        };
        api.get(request);
    }
}

export const auth = new AuthCrap();



