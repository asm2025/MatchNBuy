import { Component, Input } from "@angular/core";

import { IForecast } from "@data/model/Forecast";

@Component({
    selector: "app-forecast",
    templateUrl: "./forecast.component.html",
    styleUrls: ["./forecast.component.css"]
})
export class ForecastComponent {
	@Input()
	forecast: IForecast;

	getIcon(): string {
		if (!this.forecast) return "";
		return `@images/weather/icons/${this.forecast.keyword}.svg`;
	}

	getImage(): string {
		if (!this.forecast) return "";
		return `@images/weather/backdrops/${this.forecast.keyword}.jpg`;
	}
}
