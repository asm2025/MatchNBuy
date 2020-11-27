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
		return next.handle(req).pipe(
			catchError(error => {
				if (error && (error.status === 401 || error.status === 403)) {
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

					if (serverError.errors && typeof serverError.errors === "object") {
						for (const key in serverError.errors) {
							if (!Object.prototype.hasOwnProperty.call(serverError.errors, key)) continue;
							if (serverError.errors[key]) modalStateErrors += serverError.errors[key] + "\n";
						}
					}

					const httpErrorMessage = modalStateErrors ||
						serverError ||
						(error && error.error && error.error.message) ||
						error.statusText;
					return throwError(httpErrorMessage);
				}

				const errorMessage = (error && error.error && error.error.message) ||
					error.statusText || "Unknown Error.";
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
