import { Injectable } from "@angular/core";
import {
	HttpInterceptor,
	HttpEvent,
	HttpHandler,
	HttpRequest,
	HTTP_INTERCEPTORS
} from "@angular/common/http";
import { Observable } from "rxjs";

import config from "@/config.json";

@Injectable({
	providedIn: "root"
})
export class HTTPHeadersInterceptor implements HttpInterceptor {
	intercept(
		req: HttpRequest<any>,
		next: HttpHandler
	): Observable<HttpEvent<any>> {
		const modifiedRequest = req.clone({
			headers: req.headers
				.set("Access-Control-Allow-Credentials", "true")
				.set("Access-Control-Allow-Origin", config.backend.url || "*")
				.set("Access-Control-Allow-Headers", "*")
				.set("Access-Control-Allow-Methods", "*")
				.set("Access-Control-Max-Age", "86400")
			}
		);
		return next.handle(modifiedRequest);
	}
}

const HTTPHeadersInterceptorProvider = {
	provide: HTTP_INTERCEPTORS,
	useClass: HTTPHeadersInterceptor,
	multi: true
};

export default HTTPHeadersInterceptorProvider;
