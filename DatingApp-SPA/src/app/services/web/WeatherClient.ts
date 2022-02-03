import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import moment from "moment";

import ApiClient from "@common/web/ApiClient";
import { IForecastResult, IForecast } from "@data/model/Forecast";

import config from "@app/config.json";

@Injectable()
export default class WeatherClient extends ApiClient<HttpClient> {
	constructor(client: HttpClient) {
		super(`${config.backend.url}/Weather`, client);
	}

	list(date: any): Observable<IForecastResult> {
		const d = encodeURIComponent(moment(date).format("YYYY-MM-DD"));
		return this.client.get<IForecastResult>(`${this.baseUrl}/?date=${d}`);
	}

	get(date: any): Observable<IForecast> {
		const d = encodeURIComponent(moment(date).format("YYYY-MM-DD"));
		return this.client.get<IForecast>(`${this.baseUrl}/${d}`);
	}
}
