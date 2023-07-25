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

