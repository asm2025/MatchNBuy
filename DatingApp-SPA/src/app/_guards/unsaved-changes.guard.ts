import { Injectable } from "@angular/core";
import { CanDeactivate } from "@angular/router";

export interface IIsDirty {
	isDirty(): boolean;
}

@Injectable()
export default class UnsavedChangesGuard implements CanDeactivate<IIsDirty> {
	canDeactivate(component: IIsDirty) {
		if (component && component.isDirty()) return confirm("Are you sure you want to continue?  Any unsaved changes will be lost.");
		return true;
	}
}
