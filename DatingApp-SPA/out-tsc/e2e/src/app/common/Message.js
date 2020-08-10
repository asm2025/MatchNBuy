"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.toMessageType = exports.toAlertType = exports.AlertType = exports.MessageType = void 0;
var MessageType;
(function (MessageType) {
    MessageType[MessageType["Default"] = 0] = "Default";
    MessageType[MessageType["Trace"] = 1] = "Trace";
    MessageType[MessageType["Debug"] = 2] = "Debug";
    MessageType[MessageType["Information"] = 3] = "Information";
    MessageType[MessageType["Success"] = 4] = "Success";
    MessageType[MessageType["Warning"] = 5] = "Warning";
    MessageType[MessageType["Error"] = 6] = "Error";
    MessageType[MessageType["Fatal"] = 7] = "Fatal";
})(MessageType = exports.MessageType || (exports.MessageType = {}));
var AlertType;
(function (AlertType) {
    AlertType[AlertType["Light"] = 0] = "Light";
    AlertType[AlertType["Dark"] = 1] = "Dark";
    AlertType[AlertType["Secondary"] = 2] = "Secondary";
    AlertType[AlertType["Primary"] = 3] = "Primary";
    AlertType[AlertType["Info"] = 4] = "Info";
    AlertType[AlertType["Success"] = 5] = "Success";
    AlertType[AlertType["Warning"] = 6] = "Warning";
    AlertType[AlertType["Danger"] = 7] = "Danger";
})(AlertType = exports.AlertType || (exports.AlertType = {}));
function toAlertType(messageType) {
    let type;
    switch (messageType) {
        case MessageType.Information:
            type = AlertType.Info;
            break;
        case MessageType.Success:
            type = AlertType.Success;
            break;
        case MessageType.Warning:
            type = AlertType.Warning;
            break;
        case MessageType.Error:
        case MessageType.Fatal:
            type = AlertType.Danger;
            break;
        default:
            type = AlertType.Light;
            break;
    }
    return type;
}
exports.toAlertType = toAlertType;
function toMessageType(alertType) {
    let type;
    switch (alertType) {
        case AlertType.Light:
        case AlertType.Dark:
        case AlertType.Secondary:
        case AlertType.Primary:
            type = MessageType.Debug;
            break;
        case AlertType.Info:
            type = MessageType.Information;
            break;
        case AlertType.Success:
            type = MessageType.Success;
            break;
        case AlertType.Warning:
            type = MessageType.Warning;
            break;
        case AlertType.Danger:
            type = MessageType.Error;
            break;
        default:
            type = MessageType.Default;
            break;
    }
    return type;
}
exports.toMessageType = toMessageType;
//# sourceMappingURL=Message.js.map