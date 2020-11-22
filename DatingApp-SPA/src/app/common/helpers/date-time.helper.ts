import moment from "moment";

const DATE_FORMAT = "YYYY-MM-DD";
const DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm";
const LONG_DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm:ss";
const FULL_DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm:ss.SSSS";
const KEY_DATE_TIME_FORMAT = "YYYYMMDDHHmmssSSSS";
const TIME_FORMAT = "HH:mm";
const LONG_TIME_FORMAT = "HH:mm:ss";
const FULL_TIME_FORMAT = "HH:mm:ss.SSSS";

export default class DateTimeHelper {
	static formatISODate(date: any): string {
		const result = moment(date).toISOString();
		return result;
	}

	static formatDate(date: any): string {
		const result = moment(date).format(DATE_FORMAT);
		return result;
	}

	static formatDateShortTime(date: any): string {
		const result = moment(date).format(DATE_TIME_FORMAT);
		return result;
	}

	static formatDateTime(date: any): string {
		const result = moment(date).format(LONG_DATE_TIME_FORMAT);
		return result;
	}

	static formatDateFullTime(date: any): string {
		const result = moment(date).format(FULL_DATE_TIME_FORMAT);
		return result;
	}

	static formatKeyDateTime(date: any): string {
		const result = moment(date).format(KEY_DATE_TIME_FORMAT);
		return result;
	}

	static formatShortTime(date: any): string {
		const result = moment(date).format(TIME_FORMAT);
		return result;
	}

	static formatTime(date: any): string {
		const result = moment(date).format(LONG_TIME_FORMAT);
		return result;
	}

	static formatFullTime(date: any): string {
		const result = moment(date).format(FULL_TIME_FORMAT);
		return result;
	}
}
