import { Injectable } from "@angular/core";
import { CanActivate, Router } from "@angular/router";

import UserClient from "@services/web/UserClient";
import AlertService from "@/services/alert.service";

@Injectable({
	providedIn: "root"
})
export default class AuthGuard implements CanActivate {
	constructor(private readonly _router: Router,
		private readonly  _userClient: UserClient,
		private readonly  _alertService: AlertService) {}

	canActivate(): boolean {
		if (this._userClient.isSignedIn()) return true;
		this._alertService.toasts.error("You shall not pass!!!");
		this._router.navigate(["/"]);
		return false;
	}
}
