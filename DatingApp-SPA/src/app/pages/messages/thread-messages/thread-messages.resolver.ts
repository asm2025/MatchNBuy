import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { ISortablePagination } from "@common/pagination/SortablePagination";
import { SortType } from "@common/sorting/SortType";
import { IPaginated } from "@common/pagination/Paginated";
import { IMessage } from "@data/model/Message";
import { IUser } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@/services/alert.service";

@Injectable()
export default class ThreadMessagesResolver implements Resolve<IPaginated<IMessage>> {
	private _pagination: ISortablePagination = {
		page: 1,
		pageSize: 10,
		orderBy: [
			{
				name: "MessageSent",
				type: SortType.Descending
			}]
	};

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IPaginated<IMessage>> {
		const userId = (<IUser>this._userClient.user).id;
		const recipientId = route.params["recipientId"];
		return this._userClient
			.thread(userId, recipientId, this._pagination)
			.pipe(
				catchError(error => {
					this._alertService.toasts.error(error.toString());
					this._router.navigate(["/"]);
					return of({
						pagination: this._pagination
					});
				})
			);
	}
}
