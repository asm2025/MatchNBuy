import { Component, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Subscription } from "rxjs";
import { NgbCarousel, NgbCarouselConfig, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";

import { IAlert, AlertType } from "@common/Alert";
import WeatherClient from "@services/web/WeatherClient";
import { IForecastResult, IForecast } from "@data/model/Forecast";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"]
})
export default class WeatherComponent implements OnInit {
	alerts: IAlert[];
	forecasts: IForecast[];
	selectedDate: Date;
	@ViewChild("carouselControl", { static: true }) carouselControl!: NgbCarousel;

	private _forecastsSubscription: Subscription;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _weatherClient: WeatherClient,
		config: NgbCarouselConfig) {
		config.showNavigationArrows = false;
		config.showNavigationIndicators = false;
	}

	ngOnInit(): void {
		this._forecastsSubscription = this._route.data.subscribe(data => {
			this.selectedDate = data["resolved"].selectedDate;
			this.forecasts = data["resolved"].forecasts || [];
			if (this.forecasts.length > 0) this.select(this.key(this.selectedDate));
		});
	}

	ngOnDestroy() {
		this._forecastsSubscription.unsubscribe();
	}

	onDateChanged($event: any) {
		this.selectedDate = new Date($event);
		this.list();
	}

	list() {
		this.clearMessages();
		this._weatherClient.list(this.selectedDate)
			.subscribe((res: IForecastResult) => {
					this.selectedDate = res.selectedDate;
					this.forecasts = res.forecasts || [];
					if (this.forecasts.length > 0) this.select(this.key(this.selectedDate));
				},
				(error: any) => {
					this.alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	select(id: string) {
		this.carouselControl.select(id, NgbSlideEventSource.INDICATOR);
	}

	key(date: Date | string): string {
		if (typeof date !== "string") return date.toISOString();
		return new Date(date).toISOString();
	}

	icon(forecast: IForecast): string {
		return `assets/images/weather/icons/${forecast.keyword}.svg`;
	}

	image(forecast: IForecast): string {
		return `assets/images/weather/backdrops/${forecast.keyword}.jpg`;
	}

	clearMessages() {
		if (!this.alerts) {
			this.alerts = [];
		} else {
			this.alerts.length = 0;
		}
	}
}
