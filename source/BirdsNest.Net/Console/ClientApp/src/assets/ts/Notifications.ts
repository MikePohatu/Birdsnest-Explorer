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

import { bus, events } from "@/bus";
import { reactive } from "vue";

// along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

export const NotificationMessageLevels = {
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
        if (Object.prototype.hasOwnProperty.call(NotificationMessageLevels, level)) {
            this.message = message;
            this.level = level;
            this.eventNumber = eventCount++;
        }
        else {
            console.error(`Invalid event message level: ${level}. Must be INFO, WARN, or ERROR`);
        }
    }
}

//reactive array for Vue
export const Messages: NotificationMessage[] = reactive([]);

//Notify helper class
//Make sure to clear your notifications i.e. clear the icon popup. All messages other that processing will remain in notifications pane
class NotificationHelper {
    MaxMessages = 500;

    Clear():NotificationHelper {
        this.ClearProcessingMessages();
        bus.emit(events.ClearNotifications);
        return this;
    }

    Info(message: string): NotificationHelper {
        this.AddMessage(new NotificationMessage(NotificationMessageLevels.INFO, message));
        return this;
    }

    Warn(message: string): NotificationHelper {
        this.AddMessage(new NotificationMessage(NotificationMessageLevels.WARN, message));
        return this;
    }

    Error(message: string): NotificationHelper {
        this.AddMessage(new NotificationMessage(NotificationMessageLevels.ERROR, message));
        return this;
    }

    Fatal(message: string): NotificationHelper {
        this.AddMessage(new NotificationMessage(NotificationMessageLevels.FATAL, message));
        return this;
    }

    Processing(message: string): NotificationHelper {
        this.AddMessage(new NotificationMessage(NotificationMessageLevels.PROCESSING, message));
        return this;
    }

    Flush():NotificationHelper {
        Messages.splice(0, Infinity);
        return this;
    }

    
    private AddMessage(message: NotificationMessage) {
        Messages.push(message);
        bus.emit(events.Notify, message);
        this.Trim();
    }

      
    private ClearProcessingMessages() {
        for (let i=0; i< Messages.length; i++) {
            if (Messages[i].level === NotificationMessageLevels.PROCESSING) {
                Messages.splice(i,1);
            }
        }
    }

    //debounce, don't use webcrap becauase webcrap uses notify. circular dependency
    private timer;
    private Trim = ()=> {
        clearTimeout(this.timer);
        this.timer = setTimeout(()=>{
            if (Messages.length > this.MaxMessages) {
                Messages.splice(0,Messages.length - this.MaxMessages);
            } 
        }), 500 }
}

export const Notify = new NotificationHelper();

