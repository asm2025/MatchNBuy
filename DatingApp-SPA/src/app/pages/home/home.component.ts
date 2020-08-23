import { Component, OnInit } from "@angular/core";

import UserClient from "@services/web/UserClient";

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
