import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
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

function setToken(value: string | null | undefined) {
	if (!value)
		localStorage.removeItem("SESSIONID");
	else
		localStorage.setItem("SESSIONID", value);
}

function getUser(): IUser | null | undefined {
	if (!getToken()) return null;
	const jsonUser = localStorage.getItem("USER");
	if (!jsonUser) return null;
	return JSON.parse(jsonUser) as IUser;
}

function setUser(value: IUser | null | undefined) {
	if (!value)
		localStorage.removeItem("USER");
	else
		localStorage.setItem("USER", JSON.stringify(value));
}

@Injectable({
	providedIn: "root"
})
export default class UserClient extends ApiClient<HttpClient> {
	private readonly _jwt = new JwtHelperService();

	private _token: string | null | undefined = null;
	private _user: IUser | null | undefined = null;
	private _photo = new BehaviorSubject<string>(config.users.defaultImage);
	private _photoUrl = this._photo.asObservable();

	constructor(client: HttpClient) {
		super(`${config.backend.url}/Users`, client);
	}

	get token(): string | null | undefined {
		return this._token;
	}

	get user(): IUser | null | undefined {
		return this._user;
	}

	get photoUrl(): Observable<string> {
		return this._photoUrl;
	}

	setPhotoUrl(url: string | null | undefined) {
		this._photo.next(url || config.users.defaultImage);
	}

	// #region User
	init() {
		this._token = getToken();
		this._user = getUser();

		const photoUrl = this._user ? this._user.photoUrl : null;
		this._photo.next(photoUrl || config.users.defaultImage);
	}

	list(userList: IUserList): Observable<IPaginated<IUserForList>> {
		const params = querystring.stringify(<any>userList);
		return this.client.get<IPaginated<IUserForList>>(`${this.baseUrl}/?${params}`);
	}

	get(id: string): Observable<IUserForDetails> {
		return this.client.get<IUserForDetails>(`${this.baseUrl}/${encodeURIComponent(id)}`);
	}

	login(userName: string, password: string): Observable<boolean> {
		return this.client.post(`${this.baseUrl}/login`, { userName, password })
			.pipe(map((res: any) => {
				this._user = res.user as IUser;

				if (!this._user) {
					this.logout();
					return false;
				}

				setToken(res.token);
				setUser(this._user);
				this.init();
				return Boolean(this._user);
			}));
	}

	logout() {
		setToken(null);
		setUser(null);
		this.init();
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
