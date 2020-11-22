import { Injectable } from "@angular/core";
import { CanActivate, Router } from "@angular/router";

import UserClient from "@services/web/UserClient";

@Injectable({
	providedIn: "root"
})
export default class AuthGuard implements CanActivate {
	constructor(private readonly _router: Router,
		private readonly  _userClient: UserClient) {}

	canActivate(): boolean {
		if (this._userClient.isSignedIn()) return true;
		this._router.navigate(["/"]);
		return false;
	}
}
