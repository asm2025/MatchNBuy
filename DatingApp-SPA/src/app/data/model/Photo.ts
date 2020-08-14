export interface IPhoto {
	id: string;
	url: string;
	description: string;
	dateAdded: Date;
	isDefault: boolean;
}

export interface IPhotoToEdit {
	description: string;
	isDefault: boolean;
}
