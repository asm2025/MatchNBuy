import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { IUserForLogin } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";

import config from "@/config.json";

@Component({
	selector: "app-nav",
	templateUrl: "./nav.component.html",
	styleUrls: ["./nav.component.scss"]
})
export default class NavComponent implements OnInit {
	loginInfo: IUserForLogin = {
		userName: "",
		password: ""
	};
	photoUrl: string;
	title = config.title;

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _toastService: ToastService) {
	}

	ngOnInit() {
		this._userClient.photoUrl.subscribe(url => this.photoUrl = url);
	}

	login() {
		this._userClient.login(this.loginInfo.userName, this.loginInfo.password)
			.subscribe(() => {
					this._toastService.success("Logged in successfully.");
					this._router.navigate(["/members"]);
				},
				error => this._toastService.error(error.toString()));
	}

	logout() {
		this._userClient.logout();
	}

	isSignedIn(): boolean {
		return this._userClient.isSignedIn();
	}
}
