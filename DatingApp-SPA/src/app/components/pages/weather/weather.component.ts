import { Component, AfterViewInit, OnDestroy, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import {
	fadeInOnEnterAnimation,
	fadeOutOnLeaveAnimation,
} from "angular-animations";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";

import { IForecastResult, IForecast, IKeyedForecast } from "@data/model/Forecast";
import WeatherClient from "@services/web/WeatherClient";
import AlertService from "@services/alert.service";
import DateTimeHelper from "@common/helpers/date-time.helper";

@Component({
	selector: "app-weather",
	templateUrl: "./weather.component.html",
	styleUrls: ["./weather.component.scss"],
	animations: [
		fadeInOnEnterAnimation(),
		fadeOutOnLeaveAnimation()
	]
})
export default class WeatherComponent implements AfterViewInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	forecasts: IKeyedForecast[];
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
				setTimeout(() => {
					this.forecasts = result.forecasts.map((e: IForecast) => Object.assign(e, { key: this.key(e.date) })) || [];
					this.select(this.key(result.selectedDate));
				}, 0);
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
				this.forecasts = res.forecasts.map((e: IForecast) => Object.assign(e, { key: this.key(e.date) })) || [];
				this.select(this.key(res.selectedDate));
			});
	}

	select(key: string): void {
		setTimeout(() => {
			this.selectedDate = key;

			if (!this.weatherCarousel.slides) {
				this.weatherCarousel.activeId = this.selectedDate;
				return;
			}

			this.weatherCarousel.select(this.selectedDate, NgbSlideEventSource.INDICATOR);
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
