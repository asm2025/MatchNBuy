import {
	AbstractControl,
	FormGroup,
	FormControl,
	ValidationErrors
} from "@angular/forms";

import ValidationMessages from "@common/globalization/validation-messages";

export default class FormHelper {
	constructor(private readonly _form: FormGroup) {
	}

	getControl<TControl extends AbstractControl>(name: string): TControl {
		return FormHelper.getControl(this._form, name);
	}

	static getControl<TControl extends AbstractControl>(form: FormGroup, name: string): TControl {
		const control = form.get(name) as TControl;
		if (!control) throw new Error(`Could not find control '${name}'.`);
		return control;
	}

	isControlValid(name: string): boolean {
		return FormHelper.isControlValid(this._form, name);
	}

	static isControlValid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.valid;
	}

	isControlInvalid(name: string): boolean {
		return FormHelper.isControlInvalid(this._form, name);
	}

	static isControlInvalid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.invalid;
	}

	isControlDirtyInvalid(name: string): boolean {
		return FormHelper.isControlDirtyInvalid(this._form, name);
	}

	static isControlDirtyInvalid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.invalid && (control.dirty || control.touched);
	}

	getControlErrors(name: string): string[] | null | undefined {
		return FormHelper.getControlErrors(this._form, name);
	}

	static getControlErrors(form: FormGroup, name: string): string[] | null | undefined {
		const control = this.getControl(form, name);
		if (!control || !control.errors || control.pristine) return null;

		const errors: ValidationErrors | null = control.errors;
		if (!errors) return null;

		const keys = Object.keys(errors);
		const messages: string[] = [];

		for (const key of keys) {
			const message = ValidationMessages.getMessage(key, name, errors[key]);
			if (!message) {
				console.log(key, errors[key]);
				continue;
			}
			messages.push(message);
		}

		return messages;
	}

	isFormTouched(): boolean {
		return FormHelper.isFormTouched(this._form);
	}

	static isFormTouched(form: FormGroup): boolean {
		return form.dirty || form.touched;
	}

	isFormValid(): boolean {
		return FormHelper.isFormValid(this._form);
	}

	static isFormValid(form: FormGroup): boolean {
		return form.valid;
	}

	isFormInvalid(): boolean {
		return FormHelper.isFormInvalid(this._form);
	}

	static isFormInvalid(form: FormGroup): boolean {
		return form.invalid;
	}

	isFormDirtyInvalid(): boolean {
		return FormHelper.isFormDirtyInvalid(this._form);
	}

	static isFormDirtyInvalid(form: FormGroup): boolean {
		return form.invalid && (form.dirty || form.touched);
	}
}
