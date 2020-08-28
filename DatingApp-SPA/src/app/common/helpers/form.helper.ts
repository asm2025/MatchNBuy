import {
	AbstractControl,
	FormGroup,
	FormControl,
	ValidationErrors
} from "@angular/forms";

import ValidationMessages from "@common/globalization/validation-messages";

export default class FormHelper {
	static getControl<TControl extends AbstractControl>(form: FormGroup, name: string): TControl {
		const control = form.get(name) as TControl;
		if (!control) throw new Error(`Could not find control '${name}'.`);
		return control;
	}

	static isControlValid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.valid;
	}

	static isControlInvalid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.invalid;
	}

	static isControlDirtyInvalid(form: FormGroup, name: string): boolean {
		const control = this.getControl<FormControl>(form, name);
		return control.invalid && (control.dirty || control.touched);
	}

	static getControlErrors(form: FormGroup, name: string): string[] | null | undefined {
		const control = this.getControl(form, name);
		if (!control || (control.invalid && (control.dirty || control.touched))) return null;

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

	static isFormTouched(form: FormGroup): boolean {
		return form.touched;
	}

	static isFormInvalid(form: FormGroup): boolean {
		return form.invalid && (form.dirty || form.touched);
	}

	static isFormValid(form: FormGroup): boolean {
		return form.valid;
	}
}
