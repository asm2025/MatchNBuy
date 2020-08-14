import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IPaginated } from "@common/pagination/Paginated";
import { IUserForList, IUserList } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import { RouterStateSnapshot } from "@node/@angular/router/router";

@Injectable()
export default class UsersResolver implements Resolve<IPaginated<IUserForList>> {
	userList = {
		page: 1,
		pageSize: 10,
		minAge: 16,
		maxAge: 99
	} as IUserList;

	constructor(private _userService: UserClient, private _router: Router) {
	}

	resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<IPaginated<IUserForList>> | Promise<IPaginated<IUserForList>> | IPaginated<IUserForList> {
		return this._userService
			.list(this.userList)
			.pipe(
				catchError(error => {
					console.log(error);
					this._router.navigate(["/home"]);
					return of(null);
				}));
	}
}
