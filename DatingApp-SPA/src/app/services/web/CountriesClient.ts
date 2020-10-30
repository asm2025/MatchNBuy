import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import ApiClient from "@common/web/ApiClient";
import { ICountry, ICity } from "@data/common/Country";

import config from "@/config.json";

@Injectable()
export default class CountriesClient extends ApiClient<HttpClient> {
	constructor(client: HttpClient) {
		super(`${config.backend.url}/Countries`, client);
	}

	list(): Observable<ICountry[]> {
		return this.client.get<ICountry[]>(this.baseUrl);
	}

	get(code: string): Observable<ICountry> {
		return this.client.get<ICountry>(`${this.baseUrl}/${encodeURIComponent(code)}`);
	}

	cities(code: string): Observable<ICity[]> {
		return this.client.get<ICity[]>(`${this.baseUrl}/${encodeURIComponent(code)}/Cities`);
	}

	city(id: string): Observable<ICity> {
		return this.client.get<ICity>(`${this.baseUrl}/Cities/${encodeURIComponent(id)}`);
	}
}
