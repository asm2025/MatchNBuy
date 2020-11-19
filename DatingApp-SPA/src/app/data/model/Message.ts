import { IUserForLogin } from "./User";

export interface IMessageThread {
	threadId: string;
	participant: IUserForLogin;
	isRead: boolean;
	lastModified: Date;
	count: number;
}

export interface IMessage {
	id: string;
	threadId: string;
	senderId: string;
	recipientId: string;
	subject: string;
	content: string;
	isRead: boolean;
	dateRead?: Date;
	messageSent: Date;
	senderDeleted: boolean;
	recipientDeleted: boolean;
	isArchived: boolean;
}

export interface IMessageToAdd {
	recipientId: string;
	content: string;
}

export interface IMessageToEdit {
	content: string;
}
