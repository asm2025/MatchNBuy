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
}
