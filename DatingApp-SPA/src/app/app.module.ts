import { NgModule } from "@angular/core";
import { BrowserModule, HammerGestureConfig, HAMMER_GESTURE_CONFIG } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgbPaginationConfig } from "@ng-bootstrap/ng-bootstrap";

/*
 * https://material.io/resources/icons/?style=baseline
 * <mat-icon>location_off</mat-icon>
 */
import { MatIconModule } from "@angular/material/icon";

import { AppComponent } from "@/app.component";
import { AlertComponent } from "@components/alert/alert.component";
import { ToastComponent } from "@components/toast/toast.component";
import { NavComponent } from "@components/nav/nav.component";

import CountriesClient from "@services/web/CountriesClient";
import WeatherClient from "@services/web/WeatherClient";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";

@NgModule({
	imports: [
		BrowserModule,
		BrowserAnimationsModule,
		ReactiveFormsModule,
		HttpClientModule,
		NgbModule,
		MatIconModule
	],
	declarations: [
		AppComponent,
		AlertComponent,
		ToastComponent,
		NavComponent
	],
	providers: [
		NgbPaginationConfig,
		CountriesClient,
		WeatherClient,
		UserClient,
		ToastService
	],
	bootstrap: [
		AppComponent
	]
})
export class AppModule { }
