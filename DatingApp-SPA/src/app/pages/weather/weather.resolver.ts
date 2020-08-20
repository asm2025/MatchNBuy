import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IForecast } from "@data/model/Forecast";
import WeatherClient from "@services/web/WeatherClient";
import ToastService from "@services/toast.service";

@Injectable()
export default class ListsResolver implements Resolve<IForecast[]> {
	constructor(private readonly _router: Router,
		private readonly _weatherClient: WeatherClient,
		private readonly _toastService: ToastService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IForecast[]> {
		return this._weatherClient
			.list(new Date())
			.pipe(
				catchError(error => {
					this._toastService.error(error.toString());
					this._router.navigate(["/"]);
					return of([]);
				}));
	}
}
