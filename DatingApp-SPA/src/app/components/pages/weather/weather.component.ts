import { Component, AfterViewInit, OnDestroy, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";

import { IForecastResult, IForecast } from "@data/model/Forecast";
import WeatherClient from "@services/web/WeatherClient";
import AlertService from "@services/alert.service";
import DateTimeHelper from "@common/helpers/date-time.helper";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"]
})
export default class WeatherComponent implements AfterViewInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	forecasts: IForecast[];
	selectedDate: string;

	@ViewChild("weatherCarousel") weatherCarousel!: NgbCarousel;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _weatherClient: WeatherClient,
		private readonly _alertService: AlertService,
		carouselConfig: NgbCarouselConfig) {
		carouselConfig.showNavigationArrows = false;
		carouselConfig.showNavigationIndicators = false;
	}

	ngAfterViewInit(): void {
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => {
				const result = data["resolved"];
				setTimeout(() => this.forecasts = result.forecasts || [], 0);
				this.select(this.key(result.selectedDate));
			});
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	onDateChanged($event: any): void {
		this.list(this.key($event));
	}

	list(date: Date | string): void {
		this._weatherClient
			.list(date)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				return of({
					forecasts: [],
					selectedDate: new Date()
				});
			}))
			.subscribe((res: IForecastResult) => {
				this.forecasts = res.forecasts || [];
				this.select(this.key(res.selectedDate));
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
}
