import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { IAlert, AlertType } from "@common/Alert";
import WeatherClient from "@services/web/WeatherClient";
import { IForecast } from "@data/model/Forecast";
import { IPagination } from "@common/pagination/Pagination";
import { IPaginated } from "@common/pagination/Paginated";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"]
})
export class WeatherComponent implements OnInit {
	private _alerts: IAlert[];
	private _forecasts: IForecast[];
	private _pagination: IPagination = {
		page: 1,
		pageSize: 7
	};

	constructor(private readonly _rout: ActivatedRoute,
		private readonly _weatherClient: WeatherClient) {
	}

	ngOnInit(): void {
		this._rout.data.subscribe(d => {
			console.log(d);
			this._forecasts = d["result"];
			this._pagination = <IPagination>d["pagination"];
		});
	}

	list() {
		this.clearMessages();
		this._weatherClient.list(this._pagination)
			.subscribe((res: IPaginated<IForecast>) => {
					this._forecasts = res.result;
					this._pagination = res.pagination;
				},
				(error: any) => {
					this._alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	clearMessages() {
		if (!this._alerts) {
			this._alerts = [];
		} else {
			this._alerts.length = 0;
		}
	}
}
