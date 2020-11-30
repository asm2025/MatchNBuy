import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { of } from "rxjs";
import { catchError } from "rxjs/operators";

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
	submitted = false;
	returnUrl: string;

	constructor(private readonly _router: Router,
		private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _fb: FormBuilder,
		private readonly _alertService: AlertService) {
		this.returnUrl = this._route.snapshot.queryParams["returnUrl"] || "/members";
		if (this._userClient.isSignedIn()) this._router.navigate([this.returnUrl]);
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
		this.submitted = true;
		if (FormHelper.isFormInvalid(this.form$)) return;
		this._userClient.login(loginInfo.userName, loginInfo.password)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				return of(false);
			}))
			.subscribe((response: boolean) => {
				if (!response) return;
				this._router.navigate([this.returnUrl]);
			});
	}
}
