import { Injectable } from "@angular/core";
import { CanActivate, Router } from "@angular/router";

import { UserClient } from "@services/web/UserClient";
import { ToastService } from "@services/toast.service";

@Injectable({
	providedIn: "root"
})
export class AuthGuard implements CanActivate {
	constructor(private readonly _router: Router,
		private readonly  _userClient: UserClient,
		private readonly  _toastService: ToastService) {}

	canActivate(): boolean {
		if (this._userClient.isSignedIn()) return true;
		this._toastService.error("You shall not pass!!!");
		this._router.navigate(["/"]);
		return false;
	}
}
