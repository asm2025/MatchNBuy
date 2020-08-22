export interface IForecast {
	date: Date;
	keyword: string;
	temperatureC: number;
	temperatureF: number;
	summary: string;
}

export interface IForecastResult {
	selectedDate: Date;
	forecasts: IForecast[];
}
