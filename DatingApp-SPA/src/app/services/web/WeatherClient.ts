import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import moment from "moment";

import ApiClient from "@common/web/ApiClient";
import { IForecast } from "@data/model/Forecast";
import { IPagination } from "@common/pagination/Pagination";
import { IPaginated } from "@common/pagination/Paginated";

import config from "@/config.json";

@Injectable()
export default class WeatherClient extends ApiClient<HttpClient> {
    constructor(client: HttpClient) {
        super(<string>config.datingApp.url, client);
    }

    list(pagination: IPagination): Observable<IPaginated<IForecast>> {
        return this.client.post<IPaginated<IForecast>>(`${this.baseUrl}/weather/`, pagination);
    }

    get(date: any): Observable<IForecast> {
		const d = encodeURIComponent(moment(date).format("yyyy-MM-dd"));
        return this.client.get<IForecast>(`${this.baseUrl}/weather/date=${d}`);
    }
}
