import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { HttpClientModule } from "@angular/common/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgbPaginationConfig } from "@ng-bootstrap/ng-bootstrap";

/*
 * https://material.io/resources/icons/?style=baseline
 * <mat-icon>location_off</mat-icon>
 */
import { MatIconModule } from "@angular/material/icon";

import { AppComponent } from "@/app.component";
import { MessageComponent } from "@components/message/message.component";
import { NavComponent } from "@components/nav/nav.component";
import { WeatherComponent } from "@components/weather/weather.component";
import { ForecastComponent } from "@components/forecast/forecast.component";

import WeatherClient from "@services/web/WeatherClient";
import UserClient from "@services/web/UserClient";

@NgModule({
	imports: [
		BrowserModule,
		BrowserAnimationsModule,
		HttpClientModule,
		NgbModule,
		MatIconModule
	],
  declarations: [
		AppComponent,
		MessageComponent,
		WeatherComponent,
		ForecastComponent,
		NavComponent
  ],
	providers: [
		NgbPaginationConfig,
		WeatherClient,
		UserClient
  ],
	bootstrap: [
		AppComponent
	]
})
export class AppModule { }
