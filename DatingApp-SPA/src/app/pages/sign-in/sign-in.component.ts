import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
	FormBuilder,
	FormGroup,
	Validators
} from "@angular/forms";

import { IUserForLogin } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";
import FormHelper from "@common/helpers/form.helper";

@Component({
	selector: "app-sign-in",
	templateUrl: "./sign-in.component.html",
	styleUrls: ["./sign-in.component.scss"]
})
export default class SignInComponent implements OnInit {
	private _formHelper: FormHelper;

	form$: FormGroup;

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _fb: FormBuilder,
		private readonly _alertService: AlertService) {
	}

	get formHelper(): FormHelper {
		return this._formHelper;
	}

	ngOnInit(): void {
		this.form$ = this._fb.group({
			userName: [
				null, Validators.compose([
					Validators.required,
					Validators.maxLength(128)
				])
			],
			password: [
				null, Validators.compose([
					Validators.required,
					Validators.minLength(8),
					Validators.maxLength(128)
				])
			],
			rememberMe: [false]
		});
		this._formHelper = new FormHelper(this.form$);
	}

	login(loginInfo: IUserForLogin) {
		if (FormHelper.isFormInvalid(this.form$)) return;

		try {
			this._userClient.login(loginInfo.userName, loginInfo.password)
				.subscribe((response: boolean) => {
					if (!response) {
						this._alertService.toasts.error("Error occured while logging in.");
						return;
					}

					this._router.navigate(["/members"]);
				}, error => this._alertService.toasts.error(error.toString()));
		} catch (e) {
			this._alertService.toasts.error(e.toString());
		}
	}
}
