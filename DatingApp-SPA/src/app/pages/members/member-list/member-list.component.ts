import { Component, AfterViewInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";

import { SortType } from "@common/sorting/SortType";
import { IPaginated } from "@common/pagination/Paginated";
import { Genders } from "@data/common/Genders";
import { IUserForList, IUserList } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Component({
    selector: "app-member-list",
    templateUrl: "./member-list.component.html",
    styleUrls: ["./member-list.component.scss"]
})
export default class MemberListComponent implements AfterViewInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	users: IUserForList[];
	pagination: IUserList = {
		page: 1,
		pageSize: 12,
		gender: Genders.NotSpecified,
		minAge: 16,
		maxAge: 99,
		orderBy: [
			{
				name: "lastActive",
				type: SortType.Descending
			}
		]
	};

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	ngAfterViewInit(): void {
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => {
				setTimeout(() => {
					const resolved = data["resolved"];
					this.users = resolved.result || [];
					this.pagination = <IUserList>resolved.pagination;
				}, 0);
			});
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	loadUsers() {
		this._userClient.list(this.pagination)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				return of({
					result: [],
					pagination: {
						...this.pagination,
						page: 1,
						count: 0,
						gender: Genders.NotSpecified
					}
				});
			}))
			.subscribe((res: IPaginated<IUserForList>) => {
				this.users = res.result || [];
				this.pagination = res.pagination;
			});
	}

	pageChanged(page: number): void {
		this.pagination.page = page;
		this.loadUsers();
	}
}
