import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { ISortablePagination } from "@common/pagination/SortablePagination";
import { SortType } from "@common/sorting/SortType";
import { IPaginated } from "@common/pagination/Paginated";
import { IMessage } from "@data/model/Message";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";

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
		private readonly _toastService: ToastService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IPaginated<IMessage>> {
		const userId = this._userClient.token.nameid;
		const recipientId = route.params["recipientId"];
		return this._userClient
			.thread(userId, recipientId, this._pagination)
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
