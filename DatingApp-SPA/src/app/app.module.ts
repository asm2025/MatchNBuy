import { NgModule } from "@angular/core";
import {
	BrowserModule,
	HammerGestureConfig,
	HAMMER_GESTURE_CONFIG
} from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { JwtModule } from "@auth0/angular-jwt";
import {
	NgbModule,
	NgbAlertConfig,
	NgbToastConfig,
	NgbPaginationConfig,
	NgbModalConfig,
	NgbProgressbarConfig
} from "@ng-bootstrap/ng-bootstrap";
import { FileUploadModule } from "ng2-file-upload";
import { TimeAgoPipe } from "time-ago-pipe";

/*
 * https://material.io/resources/icons/?style=baseline
 * <mat-icon>location_off</mat-icon>
 */
import { MatIconModule } from "@angular/material/icon";

import { appRoutes } from "./routes";

import { AppComponent } from "@/app.component";
import { AlertComponent } from "@components/alert/alert.component";
import { ToastComponent } from "@components/toast/toast.component";
import { NavComponent } from "@components/nav/nav.component";

import CountriesClient from "@services/web/CountriesClient";
import WeatherClient from "@services/web/WeatherClient";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";
import { ErrorInterceptorProvider } from "@services/error.service";

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
		ErrorInterceptorProvider,
		NgbAlertConfig,
		NgbToastConfig,
		NgbPaginationConfig,
		NgbModalConfig,
		NgbProgressbarConfig,
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
