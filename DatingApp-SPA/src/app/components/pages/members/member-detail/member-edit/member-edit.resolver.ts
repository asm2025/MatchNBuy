import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IUserToUpdate } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Injectable()
export default class MemberEditResolver implements Resolve<IUserToUpdate | null | undefined> {
	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IUserToUpdate | null | undefined> {
		const userId = this._userClient.userId;
		if (!userId) return of(null);
		return this._userClient
			.edit(userId)
			.pipe(catchError(error => {
					this._alertService.toasts.error(error.toString());
					this._router.navigate(["/"]);
					return of(null);
				})
			);
	}
}
