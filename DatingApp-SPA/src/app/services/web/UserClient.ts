import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import querystring from "querystring";

import ApiClient from "@common/web/ApiClient";
import { IUser, IUserForList, IUserForSerialization, IUserToRegister, IUserToUpdate } from "@data/model/User";
import { IPagination } from "@common/pagination/Pagination";
import { IPaginated } from "@common/pagination/Paginated";

import config from "@/config.json";

@Injectable({
	providedIn: "root"
})
export default class UserClient extends ApiClient<HttpClient> {
	jwt = new JwtHelperService();
	token?: string | null | undefined;
	user: IUser | null | undefined;
	photo = new BehaviorSubject<string>(config.users.defaultImage);
	photoUrl = this.photo.asObservable();

	constructor(client: HttpClient) {
		super(`${config.backend.url}/users/`, client);
	}

	changeMemberPhoto(photoUrl: string | null | undefined) {
		this.photo.next(photoUrl || config.users.defaultImage);
	}

	list(pagination: IPagination): Observable<IPaginated<IUserForList>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IUserForList>>(`${this.baseUrl}/?${params}`);
	}

	get(id: string): Observable<IUserForList> {
		return this.client.get<IUserForList>(`${this.baseUrl}?${id}`);
	}

	login(userName: string, password: string): Observable<boolean> {
		return this.client.post(`${this.baseUrl}/login`, { userName, password })
			.pipe(map((res: any) => {
				this.user = res.user as IUser;

				if (!this.user) {
					localStorage.removeItem("token");
					localStorage.removeItem("user");
					this.token = null;
					this.changeMemberPhoto(null);
					return false;
				}

				localStorage.setItem("token", res.token);
				localStorage.setItem("user", JSON.stringify(this.user));
				this.token = this.jwt.decodeToken(res.token);
				this.changeMemberPhoto(this.user.photoUrl);
				return true;
			}));
	}

	isSignedIn(): boolean {
		const token = localStorage.getItem("token");
		return !!token && !this.jwt.isTokenExpired(token);
	}

	register(user: IUserToRegister): Observable<string> {
		return this.client.post<string>(`${this.baseUrl}/register`, user);
	}

	update(id: string, user: IUserToUpdate): Observable<IUserForSerialization> {
		return this.client.put<IUserForSerialization>(`${this.baseUrl}/${id}/update`, user);
	}

	delete(id: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${id}/delete`);
	}
}
