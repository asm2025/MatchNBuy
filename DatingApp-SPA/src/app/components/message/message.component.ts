import { Component, Input } from "@angular/core";
import { IMessage, MessageType, AlertType, toAlertType } from "@common/Message";

@Component({
	selector: "app-message",
	templateUrl: "./message.component.html",
	styleUrls: ["./message.component.css"]
})

export class MessageComponent {
	@Input() messages: IMessage[];

	protected getMessageType(message: IMessage): string {
		const alertType = toAlertType((message.type || MessageType.Default));
		return AlertType[alertType].toLowerCase();
	}

	protected isDismissible(message: IMessage): boolean {
		return message.dismissable === undefined || message.dismissable;
	}

	protected close(message: IMessage): void {
		if (!message || !this.messages || this.messages.length === 0) return;
		this.messages.splice(this.messages.indexOf(message), 1);
	}
}