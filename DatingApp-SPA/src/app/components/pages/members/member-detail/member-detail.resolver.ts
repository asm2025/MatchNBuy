import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IUserForDetails } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Injectable()
export default class MemberDetailResolver implements Resolve<IUserForDetails | null | undefined> {
	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IUserForDetails | null | undefined> {
		const id = route.params["id"];
		return this._userClient
			.get(id)
			.pipe(
				catchError(error => {
					this._alertService.toasts.error(error.toString());
					this._router.navigate(["/members"]);
					return of(null);
				})
			);
	}
}
