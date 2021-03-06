import { Component, Input } from "@angular/core";

@Component({
    selector: "app-member-messages",
    templateUrl: "./member-messages.component.html",
    styleUrls: ["./member-messages.component.scss"]
})
export default class MemberMessagesComponent {
	@Input() recipientId: string;
}
