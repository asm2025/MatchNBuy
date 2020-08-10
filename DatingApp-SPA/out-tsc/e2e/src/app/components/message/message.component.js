"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MessageComponent = void 0;
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const Message_1 = require("@common/Message");
let MessageComponent = class MessageComponent {
    getMessageType(message) {
        const alertType = Message_1.toAlertType((message.type || Message_1.MessageType.Default));
        return Message_1.AlertType[alertType].toLowerCase();
    }
    isDismissible(message) {
        return message.dismissable === undefined || message.dismissable;
    }
    close(message) {
        if (!message || !this.messages || this.messages.length === 0)
            return;
        this.messages.splice(this.messages.indexOf(message), 1);
    }
};
tslib_1.__decorate([
    core_1.Input()
], MessageComponent.prototype, "messages", void 0);
MessageComponent = tslib_1.__decorate([
    core_1.Component({
        selector: "app-message",
        templateUrl: "./message.component.html",
        styleUrls: ["./message.component.css"]
    })
], MessageComponent);
exports.MessageComponent = MessageComponent;
//# sourceMappingURL=message.component.js.map