import {
	AbstractControl,
	FormGroup,
} from "@angular/forms";
import * as _ from "lodash";

export default class CustomValidators {
	static match(controlName?: string | null | undefined) {
		return (control: AbstractControl) => {
			const parent: FormGroup | null = (control as FormGroup) || control.parent as FormGroup;
			if (!parent) return;

			let controls: AbstractControl[];

			if (!controlName) {
				controls = _.values(parent.controls);
				if (controls.length === 0) return;
			} else {
				const other = parent.get(controlName);
				if (!other) throw new Error(`Could not find form control '${controlName}'.`);
				controls = [control, other];
			}

			const errors = controls[0].errors;
			if (errors && errors.match) return;

			let value: any = null;

			for (let i = 0; i < controls.length; i++) {
				const ctrl = controls[i];

				if (!value) {
					value = ctrl.value;
					continue;
				}
				
				if (value !== ctrl.value)
					ctrl.setErrors({ match: { mismatch: true } });
				else
					ctrl.setErrors(null);
			}
		};
	}

	static compare(first: string, second: string) {
		return (parent: FormGroup) => {
			const firstControl = parent.controls[first];
			if (!firstControl) throw new Error(`Could not find form control '${first}'.`);

			const secondControl = parent.controls[second];
			if (!secondControl) throw new Error(`Could not find form control '${second}'.`);

			if (firstControl.value !== secondControl.value)
				secondControl.setErrors({ compare: { field: first, mismatch: true } });
			else
				secondControl.setErrors(null);
		}
	}
}
