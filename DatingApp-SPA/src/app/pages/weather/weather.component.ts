import { Component, AfterViewInit, OnDestroy, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Subscription } from "rxjs";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";

import { IAlert, AlertType } from "@common/Alert";
import { IForecastResult, IForecast } from "@data/model/Forecast";
import WeatherClient from "@services/web/WeatherClient";
import DateTimeHelper from "@common/helpers/DateTimeHelper";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"]
})
export default class WeatherComponent implements AfterViewInit, OnDestroy {
	alerts: IAlert[];
	forecasts: IForecast[];
	selectedDate: string;
	@ViewChild("weatherCarousel") weatherCarousel!: NgbCarousel;

	private _forecastsSubscription: Subscription;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _weatherClient: WeatherClient,
		config: NgbCarouselConfig) {
		config.showNavigationArrows = false;
		config.showNavigationIndicators = false;
	}

	ngAfterViewInit(): void {
		this._forecastsSubscription = this._route.data.subscribe(data => {
			setTimeout(() => this.forecasts = data["resolved"].forecasts || [], 0);
			this.select(this.key(data["resolved"].selectedDate));
		});
	}

	ngOnDestroy() {
		this._forecastsSubscription.unsubscribe();
	}

	onDateChanged($event: any): void {
		this.list(this.key($event));
	}

	list(date: Date | string): void {
		this.clearMessages();
		this._weatherClient.list(date)
			.subscribe((res: IForecastResult) => {
					this.forecasts = res.forecasts || [];
					this.select(this.key(res.selectedDate));
				},
				(error: any) => {
					this.alerts.push({
						type: AlertType.Error,
						content: error.toString()
					});
				});
	}

	select(id: string): void {
		setTimeout(() => {
			this.selectedDate = id;

			if (!this.weatherCarousel.slides) {
				this.weatherCarousel.activeId = id;
				return;
			}

			this.weatherCarousel.select(id, NgbSlideEventSource.INDICATOR);
		}, 0);
	}

	key(date: Date | string): string {
		return DateTimeHelper.formatDateTime(date);
	}

	icon(forecast: IForecast): string {
		return `assets/images/weather/icons/${forecast.keyword}.svg`;
	}

	image(forecast: IForecast): string {
		return `assets/images/weather/backdrops/${forecast.keyword}.jpg`;
	}

	clearMessages(): void {
		if (!this.alerts) {
			this.alerts = [];
		} else {
			this.alerts.length = 0;
		}
	}
}
