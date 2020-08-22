import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

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
	activeView: ActiveHomeView = ActiveHomeView.None;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		const outlet = this._route.snapshot.outlet.toLowerCase();
		debugger;

		switch (outlet) {
			case "login":
				this.activeView = ActiveHomeView.SignIn;
				break;
			case "register":
				this.activeView = ActiveHomeView.SignUp;
				break;
			default:
				this.activeView = ActiveHomeView.None;
				break;
		}
		console.log(this._userClient.token);
	}

	registerView(show = true) {
		this.activeView = show
			? ActiveHomeView.SignUp
			: ActiveHomeView.None;
	}

	registerToggle() {
		this.registerView(this.activeView !== ActiveHomeView.SignUp);
	}

	loginView(show = true) {
		this.activeView = show
			? ActiveHomeView.SignIn
			: ActiveHomeView.None;
	}

	loginToggle() {
		this.registerView(this.activeView !== ActiveHomeView.SignIn);
	}
}
