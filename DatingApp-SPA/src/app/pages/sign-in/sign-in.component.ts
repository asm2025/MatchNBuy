import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { IUserForLogin } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Component({
    selector: "app-sign-in",
    templateUrl: "./sign-in.component.html",
    styleUrls: ["./sign-in.component.scss"]
})
export default class SignInComponent {
	loginInfo: IUserForLogin = {
		userName: "",
		password: ""
	};

	rememberMe = false;

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	login() {
		try {
			this._userClient.login(this.loginInfo.userName, this.loginInfo.password)
				.subscribe((response: boolean) => {
						if (!response) {
							this._alertService.toasts.error("Error occured while logging in.");
							return;
						}

						this._router.navigate(["/members"]);
					},
					error => {
						this._alertService.toasts.error(error.toString());
					});
		} catch (e) {
			this._alertService.toasts.error(e.toString());
		} 
	}
}
