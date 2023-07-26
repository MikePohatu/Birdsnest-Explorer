// Copyright (c) 2019-2023 "20Road"
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
import { store, rootPaths } from "@/store/index";

let eventCount = 0;
export const notificationStates = {
	HIDDEN: -1,
	TRACE: 0,
	DEBUG: 1,
	INFO: 2,
	WARN: 3,
	ERROR: 4,
	FATAL: 5,
};

export const NotificationMessageTypes = {
	INFO: "INFO",
	WARN: "WARN",
	ERROR: "ERROR",
	FATAL: "FATAL",
	PROCESSING:"PROCESSING",
};

export class NotificationMessage {
    message = "";
    level = "";
    eventNumber: number;

    constructor(level: string, message: string) {
        if (Object.prototype.hasOwnProperty.call(NotificationMessageTypes, level)) {
            this.message = message;
            this.level = level;
            this.eventNumber = eventCount++;
        }
        else {
            console.error(`Invalid event message level: ${level}. Must be INFO, WARN, or ERROR`);
        }
    }
}

export function RegisterForNotificationMessages() {
    bus.on(events.Notifications.Info, (newmessage?: string) => {
        store.commit(rootPaths.mutations.ADD_MESSAGE, new NotificationMessage("INFO", newmessage));
	});

    bus.on(events.Notifications.Warn, (newmessage?: string) => {
        store.commit(rootPaths.mutations.ADD_MESSAGE, new NotificationMessage("WARN", newmessage));
	});

    bus.on(events.Notifications.Error, (newmessage?: string) => {
        store.commit(rootPaths.mutations.ADD_MESSAGE, new NotificationMessage("ERROR", newmessage));
	});

    bus.on(events.Notifications.Fatal, (newmessage?: string) => {
        store.commit(rootPaths.mutations.ADD_MESSAGE, new NotificationMessage("FATAL", newmessage));
	});

    bus.on(events.Notifications.Processing, (newmessage?: string) => {
        store.commit(rootPaths.mutations.ADD_MESSAGE, new NotificationMessage("PROCESSING", newmessage));
	});

    bus.on(events.Notifications.Clear, (newmessage?: string) => {
        store.commit(rootPaths.mutations.CLEAR_PROCESSING_MESSAGEs);
	});
}

//Notify helper class
//Make sure to clear your notifications i.e. clear the icon popup. All messages other that processing will remain in notifications pane
class NotificationHelper {
    Clear():NotificationHelper {
        bus.emit(events.Notifications.Clear);
        return this;
    }

    Info(message: string): NotificationHelper {
        bus.emit(events.Notifications.Info, message);
        return this;
    }

    Warn(message: string): NotificationHelper {
        bus.emit(events.Notifications.Warn, message);
        return this;
    }

    Error(message: string): NotificationHelper {
        bus.emit(events.Notifications.Error, message);
        return this;
    }

    Fatal(message: string): NotificationHelper {
        bus.emit(events.Notifications.Fatal, message);
        return this;
    }

    Processing(message: string): NotificationHelper {
        bus.emit(events.Notifications.Processing, message);
        return this;
    }
}

export const Notify = new NotificationHelper();

