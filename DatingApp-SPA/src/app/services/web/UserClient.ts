import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import querystring from "querystring";

import ApiClient from "@common/web/ApiClient";
import { ISortablePagination } from "@common/pagination/SortablePagination";
import { IPaginated } from "@common/pagination/Paginated";
import { IUser, IUserForList, IUserForSerialization, IUserToRegister, IUserToUpdate, IUserList } from "@data/model/User";
import { IPhoto, IPhotoToEdit } from "@data/model/Photo";
import { IMessageThread, IMessage, IMessageToAdd, IMessageToEdit } from "@data/model/Message";

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
		super(`${config.backend.url}/Users`, client);
	}

	// #region User
	changeMemberPhoto(photoUrl: string | null | undefined) {
		this.photo.next(photoUrl || config.users.defaultImage);
	}

	list(userList: IUserList): Observable<IPaginated<IUserForList>> {
		const params = querystring.stringify(<any>userList);
		return this.client.get<IPaginated<IUserForList>>(`${this.baseUrl}/?${params}`);
	}

	get(id: string): Observable<IUserForList> {
		return this.client.get<IUserForList>(`${this.baseUrl}/${encodeURIComponent(id)}`);
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
		return this.client.put<IUserForSerialization>(`${this.baseUrl}/${encodeURIComponent(id)}/update`, user);
	}

	delete(id: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${encodeURIComponent(id)}/delete`);
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
	like(userId: string, recipientId: string): Observable<any> {
		return this.client.post(`${this.baseUrl}/${encodeURIComponent(userId)}/Like/${encodeURIComponent(recipientId)}`, null);
	}

	unlike(userId: string, recipientId: string): Observable<any> {
		return this.client.delete(`${this.baseUrl}/${encodeURIComponent(userId)}/Unlike/${encodeURIComponent(recipientId)}`);
	}

	likes(userId: string): Observable<number> {
		return this.client.get<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Likes`);
	}

	likees(userId: string): Observable<number> {
		return this.client.get<number>(`${this.baseUrl}/${encodeURIComponent(userId)}/Likees`);
	}
	// #endregion
}
