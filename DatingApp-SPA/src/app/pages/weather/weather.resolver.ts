import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { IForecastResult } from "@data/model/Forecast";
import WeatherClient from "@services/web/WeatherClient";
import ToastService from "@services/toast.service";

@Injectable()
export default class ListsResolver implements Resolve<IForecastResult> {
	constructor(private readonly _router: Router,
		private readonly _weatherClient: WeatherClient,
		private readonly _toastService: ToastService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<IForecastResult> {
		const date = new Date();
		return this._weatherClient
			.list(date)
			.pipe(
				catchError(error => {
					this._toastService.error(error.toString());
					this._router.navigate(["/"]);
					return of({
						selectedDate: date,
						forecasts: []
					});
				}));
	}
}