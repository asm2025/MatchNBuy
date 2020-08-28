import * as _ from "lodash";
import { sprintf } from "sprintf-js";

import { IDictionary } from "@common/collections/Dictionary";
import ResourceHelper from "./resource-helper";

type FormatterDelegate = (message: string, field: string, error: any) => string | null | undefined

interface IError {
	message: IDictionary<string>;
	formatter: FormatterDelegate;
}

const __errors: IDictionary<IError> = {
	["required"]: {
		message: {
			en: "The %1$s field is required."
		},
		formatter: (message: string, field: string) => {
			return sprintf(message, field);
		}
	},
	["requiredtrue"]: {
		message: {
			en: "The %1$s field is required to have a value evaluates to true."
		},
		formatter: (message: string, field: string) => {
			return sprintf(message, field);
		}
	},
	["min"]: {
		message: {
			en: "The %1$s field's value must be greater than or equal to %2$s."
		},
		formatter: (message: string, field: string, error: any) => {
			return sprintf(message, field, error.min);
		}
	},
	["max"]: {
		message: {
			en: "The %1$s field's value must be less than or equal to %2$s."
		},
		formatter: (message: string, field: string, error: any) => {
			return sprintf(message, field, error.max);
		}
	},
	["minlength"]: {
		message: {
			en: "The %1$s field's value must be longer than or equal to %2$s characters."
		},
		formatter: (message: string, field: string, error: any) => {
			return sprintf(message, field, error.requiredLength);
		}
	},
	["maxlength"]: {
		message: {
			en: "The %1$s field's value must be shorter than or equal to %2$s characters."
		},
		formatter: (message: string, field: string, error: any) => {
			return sprintf(message, field, error.requiredLength);
		}
	},
	["email"]: {
		message: {
			en: "The %1$s field is not a valid e-mail."
		},
		formatter: (message: string, field: string) => {
			return sprintf(message, field);
		}
	},
	["pattern"]: {
		message: {
			en: "The %1$s field value is invalid."
		},
		formatter: (message: string, field: string) => {
			return sprintf(message, field);
		}
	},
	/**
	 * This is the beginning of the custom validation
	 */
	["compare"]: {
		message: {
			en: "The %1$s and %2$s field values don't match."
		},
		formatter: (message: string, field: string, error: any) => {
			return sprintf(message, field, error.field);
		}
	},
};

export default class ValidationMessageHelper {
	static getMessage(rule: string, field: string, error: any, language: string | null | undefined = null): string | null | undefined {
		if (!rule) return null;

		const formatter = _.property(`${rule}.formatter`)(__errors) as FormatterDelegate;
		if (!formatter) return null;
		if (!language) language = ResourceHelper.language;

		const message = _.property(`${rule}.message.${language}`)(__errors) as string;
		if (!message) return null;

		return formatter(message, field, error);
	}
};
