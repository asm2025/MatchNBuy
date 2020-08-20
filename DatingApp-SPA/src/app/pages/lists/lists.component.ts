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
export default class ListsComponent implements OnInit {
	alerts: IAlert[];
	users: IUserForList[];
	pagination: IUserList = {
		page: 1,
		pageSize: 10
	};
	private _likesParam = "";

	constructor(private readonly _rout: ActivatedRoute,
		private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		this._rout.data.subscribe(data => {
			this.users = data["resolved"].result;
			this.pagination = <IUserList>data["resolved"].pagination;
		});
	}

	loadUsers() {
		this.clearMessages();

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

		this._userClient.list(this.pagination)
			.subscribe((res: IPaginated<IUserForList>) => {
					this.users = res.result || [];
					this.pagination = res.pagination;
				},
				error => {
					this.alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	pageChanged(event: any): void {
		this.pagination.page = event.page;
		this.loadUsers();
	}

	clearMessages() {
		if (!this.alerts) {
			this.alerts = [];
		} else {
			this.alerts.length = 0;
		}
	}
}
