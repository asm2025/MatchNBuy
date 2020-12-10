import { Injectable } from "@angular/core";
import {
	HttpInterceptor,
	HttpEvent,
	HttpHandler,
	HttpRequest,
	HttpErrorResponse,
	HTTP_INTERCEPTORS
} from "@angular/common/http";
import { Observable, throwError } from "rxjs";
import { catchError } from "rxjs/operators";

import UserClient from "@services/web/UserClient";

@Injectable({
	providedIn: "root"
})
export class ErrorInterceptor implements HttpInterceptor {
	constructor(private readonly _userClient: UserClient) {
	}

	intercept(
		req: HttpRequest<any>,
		next: HttpHandler
	): Observable<HttpEvent<any>> {
		console.log(req);
		return next.handle(req).pipe(
			catchError(error => {
				if (!error) return throwError("Unknown Error.");

				if (error.status === 401 || error.status === 403) {
					// auto logout if 401 or 403 response returned from api
					if (this._userClient.isSignedIn()) this._userClient.logout();
				}

				if (error instanceof HttpErrorResponse) {
					const applicationError = error.headers.get("Application-Error");

					if (applicationError) {
						console.error(applicationError);
						return throwError(applicationError);
					}

					const serverError = error.error;
					let modalStateErrors = "";

					if (serverError && serverError.errors && typeof serverError.errors === "object") {
						for (const key of Object.keys(serverError.errors)) {
							if (!serverError.errors[key]) continue;
							modalStateErrors += serverError.errors[key] + "\n";
						}
					}

					const httpErrorMessage = modalStateErrors ||
						serverError ||
						(error.error && error.error.message) ||
						error.message || error.statusText;
					return throwError(httpErrorMessage);
				}

				const errorMessage = (error && error.error && error.error.message) ||
					(error && error.message) ||
					error.statusText;
				return throwError(errorMessage);
			})
		);
	}
}

const ErrorInterceptorProvider = {
	provide: HTTP_INTERCEPTORS,
	useClass: ErrorInterceptor,
	multi: true
};

export default ErrorInterceptorProvider;
