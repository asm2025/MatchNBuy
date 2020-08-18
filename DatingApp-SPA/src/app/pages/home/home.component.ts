import { Component, OnInit } from "@angular/core";

import UserClient from "@services/web/UserClient";

@Component({
	selector: "app-home",
	templateUrl: "./home.component.html",
	styleUrls: ["./home.component.scss"]
})
export class HomeComponent implements OnInit {
	registerMode = false;

	constructor(private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		//
	}

	registerToggle() {
		this.registerMode = !this.registerMode;
	}
}
