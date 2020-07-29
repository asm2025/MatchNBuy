export enum MessageType {
	Default = 0,
	Trace = 1,
	Debug = 2,
	Information = 3,
	Success = 4,
	Warning = 5,
	Error = 6,
	Fatal = 7
}

export enum AlertType {
	Light = 0,
	Dark = 1,
	Secondary = 2,
	Primary = 3,
	Info = 4,
	Success = 5,
	Warning = 6,
	Danger = 7
}

export interface IMessage {
	type?: MessageType;
	content: string;
	dismissable?: boolean;
}

export function toAlertType(messageType: MessageType): AlertType {
	let type: AlertType;

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

export function toMessageType(alertType: AlertType): MessageType {
	let type: MessageType;

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
