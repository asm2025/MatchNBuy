import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IPaginated } from "@common/pagination/Paginated";
import { IUserForList, IUserList } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";

@Injectable()
export default class MemberListResolver implements Resolve<IPaginated<IUserForList>> {
	private _pagination: IUserList = {
		page: 1,
		pageSize: 10,
	};

	constructor(private _router: Router,
		private _userClient: UserClient,
		private _toastService: ToastService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IPaginated<IUserForList>> {
		return this._userClient
			.list(this._pagination)
			.pipe(
			catchError(error => {
				this._toastService.error(error.toString());
				this._router.navigate(["/"]);
				return of({
					pagination: this._pagination
				});
			})
		);
	}
}
