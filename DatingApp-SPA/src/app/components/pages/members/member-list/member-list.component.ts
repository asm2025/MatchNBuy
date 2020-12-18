import { Component, OnInit, OnDestroy } from "@angular/core";
import { ReplaySubject, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { SortType } from "@common/sorting/SortType";
import { IPaginated } from "@common/pagination/Paginated";
import { Genders } from "@data/common/Genders";
import { IOptionItem } from "@common/data/option-item"
import { IUserForList, IUserList } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-member-list",
	templateUrl: "./member-list.component.html",
	styleUrls: ["./member-list.component.scss"]
})
export default class MemberListComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	users: IUserForList[];
	pagination: IUserList = {
		page: 1,
		pageSize: 12,
		gender: 0,
		minAge: 16,
		maxAge: 99,
		likees: false,
		likers: false,
		orderBy: [{
			name: "likes",
			type: SortType.Descending
		},
		{
			name: "lastActive",
			type: SortType.Descending
		},
		{
			name: "knownAs"
		}]
	};

	constructor(private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	ngOnInit(): void {
		this.loadUsers();
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	pageChanged(page: number): void {
		this.pagination.page = page;
		this.loadUsers();
	}

	loadUsers() {
		this._userClient.list(this.pagination)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				return of({
					result: [],
					pagination: {
						...this.pagination,
						gender: Genders.Unspecified,
						likees: false,
						likers: false,
						page: 1,
						count: 0
					}
				});
			}))
			.subscribe((res: IPaginated<IUserForList>) => {
				this.users = res.result || [];
				this.pagination = res.pagination;
			});
	}

	liked(id: string, value: number): void {
		const index = this.users.findIndex(e => e.id === id);
		if (index < 0) return;
		this.users[index] = {
			...this.users[index],
			likes: value,
			canBeDisliked: true,
			canBeLiked: false
		};
	}

	disliked(id: string, value: number): void {
		const index = this.users.findIndex(e => e.id === id);
		if (index < 0) return;
		this.users[index] = {
			...this.users[index],
			likes: value,
			canBeDisliked: false,
			canBeLiked: true
		};
	}

	genders(): IOptionItem<number>[] {
		return Object.keys(Genders)
			.filter(e => isNaN(Number(e)))
			.map(e => {
				return {
					text: e,
					value: Genders[e],
					selected: this.pagination.gender === Genders[e]
				}
			});
	}
}
