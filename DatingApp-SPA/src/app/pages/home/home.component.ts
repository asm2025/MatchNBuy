import { Component, OnInit } from "@angular/core";

import UserClient from "@services/web/UserClient";

export enum ActiveHomeView {
	None = 0,
	SignIn = 1,
	SignUp = 2
}

@Component({
	selector: "app-home",
	templateUrl: "./home.component.html",
	styleUrls: ["./home.component.scss"]
})
export default class HomeComponent implements OnInit {
	constructor(private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		console.log(this._userClient.token);
	}
}
