import { Component, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgbCarousel, NgbCarouselConfig, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";
import moment from "moment";

import { IAlert, AlertType } from "@common/Alert";
import WeatherClient from "@services/web/WeatherClient";
import { IForecast } from "@data/model/Forecast";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"]
})
export default class WeatherComponent implements OnInit {
	alerts: IAlert[];
	forecasts: IForecast[];
	selectedDate = new Date();
	@ViewChild("carouselControl", { static: true }) carouselControl: NgbCarousel;

	constructor(private readonly _rout: ActivatedRoute,
		private readonly _weatherClient: WeatherClient,
		config: NgbCarouselConfig) {
		config.showNavigationArrows = false;
		config.showNavigationIndicators = false;
	}

	ngOnInit(): void {
		this._rout.data.subscribe(data => {
			this.forecasts = data["resolved"];
		});
	}

	onDateChanged($event: any) {
		console.log($event);
		this.selectedDate = moment($event).toDate();
		this.list();
	}

	list() {
		this.clearMessages();
		this._weatherClient.list(this.selectedDate)
			.subscribe((res: IForecast[]) => {
					this.forecasts = res || [];
				},
				(error: any) => {
					this.alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	select(forecast: IForecast) {
		const id = this.key(forecast);
		this.carouselControl.select(id, NgbSlideEventSource.INDICATOR);
	}

	key(forecast: IForecast): string {
		return forecast.date.toISOString();
	}

	icon(forecast: IForecast): string {
		return `@images/weather/icons/${forecast.keyword}.svg`;
	}

	image(forecast: IForecast): string {
		return `@images/weather/backdrops/${forecast.keyword}.jpg`;
	}

	clearMessages() {
		if (!this.alerts) {
			this.alerts = [];
		} else {
			this.alerts.length = 0;
		}
	}
}
