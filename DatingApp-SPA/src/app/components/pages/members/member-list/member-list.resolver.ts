import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { SortType } from "@common/sorting/SortType";
import { IPaginated } from "@common/pagination/Paginated";
import { Genders } from "@data/common/Genders";
import { IUserForList, IUserList } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Injectable()
export default class MemberListResolver implements Resolve<IPaginated<IUserForList>> {
	private _pagination: IUserList = {
		page: 1,
		pageSize: 12,
		gender: Genders.NotSpecified,
		minAge: 16,
		maxAge: 99,
		likees: false,
		likers: false,
		orderBy: [
			{
				name: "lastActive",
				type: SortType.Descending
			}
		]
	};

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IPaginated<IUserForList>> {
		return this._userClient
			.list(this._pagination)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				this._router.navigate(["/"]);
				return of({
					result: [],
					pagination: {
						...this._pagination,
						gender: Genders.NotSpecified,
						likees: false,
						likers: false,
						page: 1,
						count: 0
					}
				});
			}));
	}
}
