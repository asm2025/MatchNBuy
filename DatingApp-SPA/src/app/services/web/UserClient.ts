import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

import ApiClient from "@common/web/ApiClient";
import { IUser } from "@data/model/User";
import { IPagination } from "@common/pagination/Pagination";
import { IPaginated } from "@common/pagination/Paginated";

import config from "@/config.json";

@Injectable({
	providedIn: "root"
})
export default class UserClient extends ApiClient<HttpClient> {
	jwt = new JwtHelperService();
	token: any;
	user: IUser;
	photo = new BehaviorSubject<string>("@images/user.png");
	photoUrl = this.photo.asObservable();

	constructor(client: HttpClient) {
		super(<string>config.datingApp.url, client);
	}

	list(pagination: IPagination): Observable<IPaginated<IUser>> {
		return this.client.post<IPaginated<IUser>>(`${this.baseUrl}/user/`, pagination);
	}

	login(userName: string, password: string): Observable<boolean> {
		return this.client.post(`${this.baseUrl}/users/login`, { userName, password })
			.pipe(map((res: any) => {
				console.log(res);
				return true;
			}));
	}
}
