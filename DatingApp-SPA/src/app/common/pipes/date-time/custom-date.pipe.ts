import { Inject, Pipe, PipeTransform, LOCALE_ID } from "@angular/core";
import { DatePipe } from "@angular/common";

@Pipe({
	name: "customDate",
	pure: true
})
export default class CustomDatePipe extends DatePipe implements PipeTransform {
	private readonly _customFormats = {
		short: "yyyy-MM-dd hh:mm a",
		medium: "yyyy-MM-dd, MMM, hh:mm:ss a",
		long: "yyyy-MM-dd, MMMM, hh:mm:ss a z",
		full: "yyyy-MM-dd, EEEE, MMMM, hh:mm:ss a zzzz",
		shortDate: "yyyy-MM-dd",
		mediumDate: "yyyy-MM-dd, MMM",
		longDate: "yyyy-MM-dd, MMMM",
		fullDate: "yyyy-MM-dd, EEEE, MMMM",
		shortTime: "hh:mm a",
		mediumTime: "hh:mm:ss a",
		longTime: "hh:mm:ss a z",
		fullTime: "hh:mm:ss a zzzz",
		iso8601: "yyyy-MM-ddTHH:mm:ssZ"
	};

	constructor(@Inject(LOCALE_ID) private readonly _locale: string) {
		super(_locale);
	}

	transform(value: Date | string | number | null | undefined, format = "short", timezone: string | undefined = undefined, locale: string | undefined = undefined): string | null {
		return super.transform(value, this._customFormats[format] || format, timezone, locale || this._locale);
	}
}
