import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import moment from "moment";
import querystring from "querystring";

import ApiClient from "@common/web/ApiClient";
import { IForecast } from "@data/model/Forecast";
import { IPagination } from "@common/pagination/Pagination";
import { IPaginated } from "@common/pagination/Paginated";

import config from "@/config.json";

@Injectable()
export default class WeatherClient extends ApiClient<HttpClient> {
	constructor(client: HttpClient) {
		super(`${config.backend.url}/Weather`, client);
	}

	list(pagination: IPagination): Observable<IPaginated<IForecast>> {
		const params = querystring.stringify(<any>pagination);
		return this.client.get<IPaginated<IForecast>>(`${this.baseUrl}/?${params}`);
	}

	get(date: any): Observable<IForecast> {
		const d = encodeURIComponent(moment(date).format("yyyy-MM-dd"));
		return this.client.get<IForecast>(`${this.baseUrl}/${d}`);
	}
}
