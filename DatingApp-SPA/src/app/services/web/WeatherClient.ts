import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import moment from "moment";

import ApiClient from "@common/web/ApiClient";
import { IForecast } from "@data/model/Forecast";

import config from "@/config.json";

@Injectable()
export default class WeatherClient extends ApiClient<HttpClient> {
	constructor(client: HttpClient) {
		super(`${config.backend.url}/Weather`, client);
	}

	list(date: any): Observable<IForecast[]> {
		const d = encodeURIComponent(moment(date).format("yyyy-MM-dd"));
		return this.client.get<IForecast[]>(`${this.baseUrl}/?date=${d}`);
	}

	get(date: any): Observable<IForecast> {
		const d = encodeURIComponent(moment(date).format("yyyy-MM-dd"));
		return this.client.get<IForecast>(`${this.baseUrl}/${d}`);
	}
}
