import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { IAlert, AlertType } from "@common/Alert";
import UserClient from "@services/web/UserClient";
import { IUserForList, IUserList } from "@data/model/User";
import { IPaginated } from "@common/pagination/Paginated";

@Component({
	selector: "app-lists",
	templateUrl: "./lists.component.html",
	styleUrls: ["./lists.component.scss"]
})
export class ListsComponent implements OnInit {
	private _alerts: IAlert[];
	private _users: IUserForList[];
	private _pagination: IUserList = {
		page: 1,
		pageSize: 10
	};
	private _likesParam = "";

	constructor(private readonly _rout: ActivatedRoute,
		private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		this._rout.data.subscribe(d => {
			console.log(d);
			this._users = d["result"];
			this._pagination = <IUserList>d["pagination"];
		});
	}

	loadUsers() {
		this.clearMessages();

		switch (this._likesParam) {
			case "Likers":
				this._pagination.likers = true;
				this._pagination.likees = false;
				break;
			case "Likees":
				this._pagination.likers = false;
				this._pagination.likees = true;
				break;
			default:
				this._pagination.likers = false;
				this._pagination.likees = false;
				break;
		}

		this._userClient.list(this._pagination)
			.subscribe((res: IPaginated<IUserForList>) => {
					this._users = res.result || [];
					this._pagination = res.pagination;
				},
				error => {
					this._alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	pageChanged(event: any): void {
		this._pagination.page = event.page;
		this.loadUsers();
	}

	clearMessages() {
		if (!this._alerts) {
			this._alerts = [];
		} else {
			this._alerts.length = 0;
		}
	}
}
