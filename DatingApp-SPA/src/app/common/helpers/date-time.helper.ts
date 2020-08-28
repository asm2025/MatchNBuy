import moment from "moment";

const DATE_FORMAT = "YYYY-MM-DD";
const DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm";
const LONG_DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm:ss";
const FULL_DATE_TIME_FORMAT = "YYYY-MM-DD HH:mm:ss.SSSS";
const TIME_FORMAT = "HH:mm";
const LONG_TIME_FORMAT = "HH:mm:ss";
const FULL_TIME_FORMAT = "HH:mm:ss.SSSS";

export default class DateTimeHelper {
	static formatISODate(date: any): string {
		return moment(date).toISOString();
	}

	static formatDate(date: any): string {
		return moment(date).format(DATE_FORMAT);
	}

	static formatDateShortTime(date: any): string {
		return moment(date).format(DATE_TIME_FORMAT);
	}

	static formatDateTime(date: any): string {
		return moment(date).format(LONG_DATE_TIME_FORMAT);
	}

	static formatDateFullTime(date: any): string {
		return moment(date).format(FULL_DATE_TIME_FORMAT);
	}

	static formatShortTime(date: any): string {
		return moment(date).format(TIME_FORMAT);
	}

	static formatTime(date: any): string {
		return moment(date).format(LONG_TIME_FORMAT);
	}

	static formatFullTime(date: any): string {
		return moment(date).format(FULL_TIME_FORMAT);
	}
}
