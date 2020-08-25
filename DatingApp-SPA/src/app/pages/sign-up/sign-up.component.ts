import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { Genders } from "@data/common/Genders";
import { IUserToRegister } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@/services/alert.service";

@Component({
	selector: "app-sign-up",
	templateUrl: "./sign-up.component.html",
	styleUrls: ["./sign-up.component.scss"]
})
export default class SignUpComponent {
	registrationInfo: IUserToRegister = {
		userName: "",
		password: "",
		email: "",
		phoneNumber: "",
		firstName: "",
		lastName: "",
		knownAs: "",
		dateOfBirth: new Date(),
		gender: Genders.NotSpecified,
		cityId: ""
	};

	confirmPassword = "";
	countryId = "";

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	register() {
		try {
			this._userClient.register(this.registrationInfo)
				.subscribe(() => {
						this._alertService.toasts.success("Logged in successfully.");
						this._router.navigate(["/members"]);
					},
					error => {
						this._alertService.alerts.error(error.toString());
					});
		} catch (e) {
			this._alertService.alerts.error(e.toString());
		}
	}

	onNameChange(firstName: string | null | undefined, lastName: string | null | undefined) {
		this.registrationInfo.knownAs = (firstName || "") + (lastName || "");
	}
}
