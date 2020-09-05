import { Component } from "@angular/core";

import UserClient from "@services/web/UserClient";

@Component({
	selector: "app-home",
	templateUrl: "./home.component.html",
	styleUrls: ["./home.component.scss"]
})
export default class HomeComponent {
	constructor(private readonly _userClient: UserClient) {
	}

	protected isSignedIn(): boolean {
		return this._userClient.isSignedIn();
	}
}
