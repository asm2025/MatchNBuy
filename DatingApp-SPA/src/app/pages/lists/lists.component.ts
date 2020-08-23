import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Subscription } from "rxjs";

import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";
import { IUserForList, IUserList } from "@data/model/User";
import { IPaginated } from "@common/pagination/Paginated";

@Component({
	selector: "app-lists",
	templateUrl: "./lists.component.html",
	styleUrls: ["./lists.component.scss"]
})
export default class ListsComponent implements OnInit, OnDestroy {
	users: IUserForList[];
	pagination: IUserList = {
		page: 1,
		pageSize: 10
	};

	private _likesParam = "";
	private _routeSubscription: Subscription;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	ngOnInit() {
		this._routeSubscription = this._route.data.subscribe(data => {
			this.users = data["resolved"].result;
			this.pagination = <IUserList>data["resolved"].pagination;
		});
	}

	ngOnDestroy() {
		this._routeSubscription.unsubscribe();
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
					}, error => this._alertService.alerts.error(error.toString()));
		} catch (e) {
			this._alertService.alerts.error(e.toString());
		} 
	}

	pageChanged(event: any): void {
		this.pagination.page = event.page;
		this.loadUsers();
	}
}
