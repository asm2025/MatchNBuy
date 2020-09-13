import { Genders } from "@data/common/Genders";
import { ISortablePagination } from "@common/pagination/SortablePagination";

export interface IUserForLogin {
	userName: string;
	password: string;
}

export interface IUser {
	id: string;
	userName: string;
	email: string;
	knownAs: string;
	gender: Genders;
	dateOfBirth: Date;
	age: number;
	photoUrl?: string;
	countryCode?: string;
	country?: string;
	cityId?: string;
	city?: string;
	created: Date;
	lastActive: Date;
}

export interface IUserForList extends IUser {
	firstName: string;
	lastName?: string;
	introduction?: string;
	lookingFor?: string;
	canBeLiked: boolean;
	canBeDisliked: boolean;
	likes: number;
}

export interface IUserForDetails extends IUserForList {
	phoneNumber: string;
	interests?: Array<string>;
	roles: Array<string>;
}

export interface IUserForSerialization extends IUserForList {
	phoneNumber: string;
	modified: Date;
}

export interface IUserToRegister {
	userName: string | null | undefined;
	password: string | null | undefined;
	email: string | null | undefined;
	phoneNumber: string | null | undefined;
	firstName: string | null | undefined;
	lastName: string | null | undefined;
	knownAs: string | null | undefined;
	gender: Genders | null | undefined;
	dateOfBirth: Date | null | undefined;
	cityId: string | null | undefined;
	introduction: string | null | undefined;
	lookingFor: string | null | undefined;
}

export interface IUserToUpdate {
	firstName: string;
	lastName?: string;
	knownAs: string;
	gender: Genders;
	dateOfBirth: Date;
	phoneNumber: string;
	cityId: string;
	introduction?: string;
	lookingFor?: string;
	interests?: Array<string>;
	roles?: Array<string>;
}

export interface IUserList extends ISortablePagination {
	gender?: Genders;
	minAge?: number;
	maxAge?: number;
	likees?: boolean;
	likers?: boolean;
}
