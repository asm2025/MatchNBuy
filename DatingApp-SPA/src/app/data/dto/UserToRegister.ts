import { Genders } from "@data/common/Genders";

export default class UserToRegister {
	userName: string;
	password: string;
	firstName: string;
	lastName: string;
	knownAs: string;
	gender: Genders;
	phoneNumber: string;
	email: string;
	dateOfBirth: Date;
	introduction: string;
	lookingFor: string;
	cityId: string;
}
