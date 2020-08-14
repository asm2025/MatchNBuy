import { Genders } from "@data/common/Genders";

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
	firstName?: string;
	lastName?: string;
	introduction?: string;
	lookingFor?: string;
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
	userName: string;
	password: string;
	email: string;
	phoneNumber: string;
	firstName: string;
	lastName?: string;
	knownAs: string;
	gender: Genders;
	dateOfBirth: Date;
	cityId: string;
	introduction?: string;
	lookingFor?: string;
}

export interface IUserToUpdate {
	phoneNumber: string;
	firstName: string;
	lastName?: string;
	knownAs: string;
	gender: Genders;
	dateOfBirth: Date;
	cityId: string;
	introduction?: string;
	lookingFor?: string;
	interests?: Array<string>;
	roles?: Array<string>;
}
