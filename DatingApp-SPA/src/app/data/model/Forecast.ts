export interface IForecast {
	date: Date;
	temperatureC: number;
	temperatureF: number;
	imageUrl: string;
	summary: string;
}

export default class Forecast implements IForecast {
	constructor(public date: Date, public temperatureC: number, public temperatureF: number, public imageUrl: string, public summary: string) {
	}
}
