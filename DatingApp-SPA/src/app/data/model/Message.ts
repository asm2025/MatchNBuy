import { IUser } from "./User";

export interface IMessageThread {
	threadId: string;
	participant: IUser;
	isRead: boolean;
	lastModified: Date;
	count: number;
}

export interface IMessage {
	threadId: string;
	senderId: string;
	recipientId: string;
	content: string;
	isRead: boolean;
	dateRead?: Date;
	messageSent: Date;
}

export interface IMessageToAdd {
	recipientId: string;
	content: string;
}

export interface IMessageToEdit {
	content: string;
}
