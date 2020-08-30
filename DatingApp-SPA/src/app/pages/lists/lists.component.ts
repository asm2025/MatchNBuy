import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ReplaySubject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";
import { IPaginated } from "@common/pagination/Paginated";
import { IUserForList, IUserList } from "@data/model/User";
import { Genders } from "@data/common/Genders";

@Component({
	selector: "app-lists",
	templateUrl: "./lists.component.html",
	styleUrls: ["./lists.component.scss"]
})
export default class ListsComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	users: IUserForList[];
	pagination: IUserList = {
		page: 1,
		pageSize: 10,
		genders: Genders.NotSpecified,
		minAge: 16,
		maxAge: 99,
		likees: false,
		likers: false
	};

	private _likesParam = "";

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	ngOnInit() {
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => {
				this.users = data["resolved"].result;
				this.pagination = <IUserList>data["resolved"].pagination;
			});
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	loadUsers() {
		switch (this._likesParam) {
			case "Likers":
				this.pagination.likers = true;
				this.pagination.likees = false;
				break;
			case "Likees":
				this.pagination.likers = false;
				this.pagination.likees = true;
				break;
			default:
				this.pagination.likers = false;
				this.pagination.likees = false;
				break;
		}

		try {
			this._userClient.list(this.pagination)
				.subscribe((res: IPaginated<IUserForList>) => {
						this.users = res.result || [];
						this.pagination = res.pagination;
					}, error => this._alertService.toasts.error(error.toString()));
		} catch (e) {
			this._alertService.toasts.error(e.toString());
		} 
	}

	pageChanged(event: any): void {
		this.pagination.page = event.page;
		this.loadUsers();
	}
}
