import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

export interface IApiClient<TClient extends HttpClient> {
	client: TClient;
    baseUrl: string;
}

@Injectable()
export abstract default class ApiClient<TClient extends HttpClient> implements IApiClient<TClient> {
    constructor(public baseUrl: string, public client: TClient) {
    }
}
