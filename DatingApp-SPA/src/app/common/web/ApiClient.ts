import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Injectable()
export abstract default class ApiClient<TClient extends HttpClient> {
    protected constructor(protected baseUrl: string, protected client: TClient) {
    }
}
