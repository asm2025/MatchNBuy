import { Injectable, OnInit, OnDestroy } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable, BehaviorSubject, ReplaySubject, of } from "rxjs";
import { takeUntil, map } from "rxjs/operators";
import querystring from "querystring";

import ApiClient from "@common/web/ApiClient";
import { ISortablePagination } from "@common/pagination/SortablePagination";
import { IPaginated } from "@common/pagination/Paginated";
import { IUser, IUserForList, IUserForDetails, IUserToRegister, IUserToUpdate, IUserList } from "@data/model/User";
import { IPhoto, IPhotoToEdit } from "@data/model/Photo";
import { IMessageThread, IMessage, IMessageToAdd, IMessageToEdit } from "@data/model/Message";

import config from "@/config.json";

export function getToken(): string | null {
	return localStorage.getItem("SESSIONID");
}

function setSession(value: string | null | undefined) {
	if (!value)
		localStorage.removeItem("SESSIONID");
	else
		localStorage.setItem("SESSIONID", value);
}

function setSessionUser(value: IUser | null | undefined) {
	if (!value)
		localStorage.removeItem("USER");
	else
		localStorage.setItem("USER", JSON.stringify(value));
}

@Injectable({
	providedIn: "root"
})
export default class UserClient extends ApiClient<HttpClient> implements OnInit, OnDestroy {
	private readonly _jwt = new JwtHelperService();

	private _token: string | null | undefined = null;
	private _userSubject = new BehaviorSubject<IUser | null | undefined>(null);
	private _user = this._userSubject.asObservable();
	private _refreshTokenTimeout: ReturnType<typeof setTimeout>;

	disposed$ = new ReplaySubject<boolean>();

	constructor(private readonly _router: Router,
		client: HttpClient) {
		super(`${config.backend.url}/Users`, client);
	}

	get token(): string | null | undefined {
		return this._token;
	}

	get user(): Observable<IUser | null | undefined> {
		return this._user;
	}

	get userId(): string | null | undefined {
		const user = this._userSubject.value as IUser;
		return user.id;
	}

	get photoUrl(): string {
		const user = this._userSubject.value as IUser;
		return (user && user.photoUrl) || config.users.defaultImage;
	}

	set photoUrl(url: string) {
		const oldUser = this._userSubject.value as IUser;
		if (!oldUser) return;
		const user: IUser = {
			...oldUser,
			photoUrl: url || config.users.defaultImage
		}
		this._userSubject.next(user);
	}

	ngOnInit(): void {
		this._user
			.pipe(takeUntil(this.disposed$))
			.subscribe(user => {
				setSessionUser(user);
				this._token = getToken();

				if (user)
					this.startRefreshTokenTimer();
				else
					this.stopRefreshTokenTimer();
			});
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	// #region User
	list(userList: IUserList): Observable<IPaginated<IUserForList>> {
		const params = querystring.stringify(<any>userList);
		return this.client.get<IPaginated<IUserForList>>(`${this.baseUrl}/?${params}`);
	}

	get(id: string): Observable<IUserForDetails> {
		return this.client.get<IUserForDetails>(`${this.baseUrl}/${encodeURIComponent(id)}`);
	}

	login(userName: string, password: string): Observable<boolean> {
		this.stopRefreshTokenTimer();
		return this.client.post(`${this.baseUrl}/login`, { userName, password }, { withCredentials: true })
			.pipe(map((res: any) => {
				if (res && res.token && res.user) {
					setSession(res.token);
					this._userSubject.next(res.user);
					return true;
				}

				this.clearLogin();
				return false;
			}));
	}

	refreshToken(): Observable<any> {
		this.stopRefreshTokenTimer();
		if (!getToken()) return of(null);
		return this.client.post(`${this.baseUrl}/refreshToken`, {}, { withCredentials: true })
			.pipe(map((res: any) => {
				if (res && res.token && res.user) {
					setSession(res.token);
					this._userSubject.next(res.user);
					return true;
				}

				this.clearLogin();
				return false;
			}));
	}

	logout() {
		this.stopRefreshTokenTimer();
		this.client.post(`${this.baseUrl}/logout`, {}, { withCredentials: true }).subscribe();
		this.clearLogin();
		this._router.navigate(["/"]);
	}

	clearLogin() {
		setSession(null);
		this._userSubject.next(null);
	}

	isSignedIn(): boolean {
		const token = getToken();
		if (!token) return false;

		try {
			return !this._jwt.isTokenExpired(token);
		} catch (e) {
			return false;
		} 
	}

	register(user: IUserToRegister): Observable<string> {
		return this.client.post<string>(`${this.baseUrl}/register`, user);
	}

	edit(id: string): Observable<IUserToUpdate> {
		return this.client.get<IUserToUpdate>(`${this.baseUrl}/${encodeURIComponent(id)}/Edit`);
	}

	update(id: string, user: IUserToUpdate): Observable<IUserToUpdate> {
		return this.client.put<IUserToUpdate>(`${this.baseUrl}/${encodeURIComponent(id)}/Update`, user);
	}

	delete(id: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${encodeURIComponent(id)}/Delete`);
	}

	private startRefreshTokenTimer() {
		this.stopRefreshTokenTimer();

		const token = getToken();
		if (!token) return;

		let expires: Date | null;

		try {
			expires = this._jwt.getTokenExpirationDate();
		} catch (e) {
			return;
		}

		if (!expires) return;

		const timeout = expires.getTime() - Date.now() - (60 * 1000);
		if (timeout <= 0) return;
		this._refreshTokenTimeout = setTimeout(() => this.refreshToken().subscribe(), timeout);
	}

	private stopRefreshTokenTimer() {
		clearTimeout(this._refreshTokenTimeout);
	}
	// #endregion

	// #region Photos
	photos(userId: string, pagination: ISortablePagination): Observable<IPaginated<IPhoto>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IPhoto>>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/?${params}`);
	}

	getPhoto(userId: string, id: string): Observable<IPhoto> {
		return this.client.get<IPhoto>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/${encodeURIComponent(id)}`);
	}

	addPhoto(userId: string, photoToAdd: FormData): Observable<string> {
		return this.client.post<string>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/Add`, photoToAdd);
	}

	updatePhoto(userId: string, id: string, photoToEdit: IPhotoToEdit): Observable<IPhoto> {
		return this.client.put<IPhoto>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/${encodeURIComponent(id)}/Update`, photoToEdit);
	}

	deletePhoto(userId: string, id: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/${encodeURIComponent(id)}/Delete`);
	}

	defaultPhoto(userId: string): Observable<IPhoto> {
		return this.client.get<IPhoto>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/Default`);
	}

	setDefaultPhoto(userId: string, id: string): Observable<IPhoto> {
		return this.client.put<IPhoto>(`${this.baseUrl}/${encodeURIComponent(userId)}/Photos/${encodeURIComponent(id)}/SetDefault`, null);
	}
	// #endregion

	// #region Messages
	threads(userId: string, pagination: ISortablePagination): Observable<IPaginated<IMessageThread>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IMessageThread>>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/Threads/?${params}`);
	}

	thread(userId: string, recipientId: string, pagination: ISortablePagination): Observable<IPaginated<IMessage>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IMessage>>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/Thread/${encodeURIComponent(recipientId)}/?${params}`);
	}

	messages(userId: string, pagination: ISortablePagination): Observable<IPaginated<IMessage>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IMessage>>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/?${params}`);
	}

	getMessage(userId: string, id: string): Observable<IMessage> {
		return this.client.get<IMessage>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/${encodeURIComponent(id)}`);
	}

	addMessage(userId: string, messageToAdd: IMessageToAdd): Observable<string> {
		return this.client.post<string>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/Add`, messageToAdd);
	}

	updateMessage(userId: string, id: string, messageToEdit: IMessageToEdit): Observable<IMessage> {
		return this.client.put<IMessage>(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/${encodeURIComponent(id)}/Update`, messageToEdit);
	}

	deleteMessage(userId: string, id: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/${encodeURIComponent(id)}/Delete`);
	}

	markMessage(userId: string, id: string, isRead: boolean): Observable<any> {
		return this.client.put(`${this.baseUrl}/${encodeURIComponent(userId)}/Messages/${encodeURIComponent(id)}/?isRead=${isRead}`, null);
	}
	// #endregion

	// #region Likes
	like(userId: string, recipientId: string): Observable<number> {
		return this.client.post<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Like/${encodeURIComponent(recipientId)}`, null);
	}

	dislike(userId: string, recipientId: string): Observable<number> {
		return this.client.delete<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Dislike/${encodeURIComponent(recipientId)}`);
	}

	likes(userId: string): Observable<number> {
		return this.client.get<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Likes`);
	}

	likees(userId: string): Observable<number> {
		return this.client.get<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Likees`);
	}
	// #endregion
}
