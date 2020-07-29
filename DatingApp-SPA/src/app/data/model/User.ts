import { IPhoto } from "./Photo";

export interface IUser {
	id: string;
	userName: string;
	knownAs: string;
	age: number;
	gender: string;
	created: Date;
	lastActive: any;
	photoUrl: string;
	city: string;
	country: string;
	interests?: string;
	introduction?: string;
	lookingFor?: string;
	photos?: IPhoto[];
}