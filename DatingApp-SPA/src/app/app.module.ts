import { NgModule } from "@angular/core";
import {
	BrowserModule,
	Title,
} from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";

import { JwtModule } from "@auth0/angular-jwt";

import {
	NgbModule,
	NgbAlertConfig,
	NgbToastConfig,
	NgbTooltipConfig,
	NgbPaginationConfig,
	NgbModalConfig,
	NgbProgressbarConfig
} from "@ng-bootstrap/ng-bootstrap";
import { LazyLoadImageModule } from "ng-lazyload-image";

import { FileUploadModule } from "ng2-file-upload";
import { NgSelectModule } from "@ng-select/ng-select";
import { NgOptionHighlightModule } from "@ng-select/ng-option-highlight";
/*
 * https://material.io/resources/icons/?style=baseline
 * <mat-icon>location_off</mat-icon>
 */
//import { MatIconModule } from "@angular/material/icon";
import { FontAwesomeModule, FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { fab } from "@fortawesome/free-brands-svg-icons";

import { TimeAgoPipe } from "time-ago-pipe";

import AppRoutingModule from "./app-routing.module";

import TrimPipe from "@common/pipes/string/trim.pipe";
import TrimLeftPipe from "@common/pipes/string/trim-left.pipe";
import TrimRightPipe from "@common/pipes/string/trim-right.pipe";
import CustomDatePipe from "@common/pipes/date-time/custom-date.pipe";

import { NgControlStatus } from "@common/directives/ng_control_status";

import HammerConfigProvider from "@/config/hammer.config";

import HTTPHeadersInterceptorProvider from "@/interceptors/http-headers.interceptor";
import ErrorInterceptorProvider from "@/interceptors/error.interceptor";

import CountriesClient from "@services/web/CountriesClient";
import WeatherClient from "@services/web/WeatherClient";
import UserClient, { getToken } from "@services/web/UserClient";
import AlertService from "@services/alert.service";

import AuthGuard from "@common/guards/auth.guard";
import UnsavedChangesGuard from "@common/guards/unsaved-changes.guard";

import AppComponent from "./app.component";
import SpinnerComponent from "@components/spinner/spinner.component";
import AlertsComponent from "@components/alert/alerts/alerts.component";
import NavComponent from "@components/nav/nav.component";
import LazyImageComponent from "@components/lazy-image/lazy-image.component";

import HomeComponent from "@components/pages/home/home.component";

import MemberListComponent from "@components/pages/members/member-list/member-list.component";
import MemberCardComponent from "@components/pages/members/member-card/member-card.component";
import MemberDetailComponent from "@components/pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@components/pages/members/member-detail/member-detail.resolver";
import MemberEditComponent from "@components/pages/members/member-detail/member-edit/member-edit.component";
import MemberEditResolver from "@components/pages/members/member-detail/member-edit/member-edit.resolver";
import MemberGalleryComponent from "@components/pages/members/member-detail/member-gallery/member-gallery.component";
import MemberMessagesComponent from "@components/pages/members/member-detail/member-messages/member-messages.component";
import MemberPhotoEditorComponent from "@components/pages/members/member-detail/member-photo-editor/member-photo-editor.component";
import MemberPhotoEditorResolver from "@components/pages/members/member-detail/member-photo-editor/member-photo-editor.resolver";

import MessagesComponent from "@components/pages/messages/messages.component";
import ThreadsComponent from "@components/pages/messages/threads/threads.component";
import ThreadMessagesComponent from "@components/pages/messages/thread-messages/thread-messages.component";

import SignInComponent from "@components/pages/sign-in/sign-in.component";
import SignUpComponent from "@components/pages/sign-up/sign-up.component";

import WeatherComponent from "@components/pages/weather/weather.component";

@NgModule({
	imports: [
		BrowserModule,
		BrowserAnimationsModule,
		FormsModule,
		ReactiveFormsModule,
		HttpClientModule,
		JwtModule.forRoot({
			config: {
				tokenGetter: getToken,
				allowedDomains: ["localhost:8000"],
				disallowedRoutes: ["localhost:8000/Users/Login"]
			}
		}),
		FontAwesomeModule,
		LazyLoadImageModule,
		//MatIconModule,
		NgSelectModule,
		NgOptionHighlightModule,
		FileUploadModule,
		NgbModule,
		AppRoutingModule
	],
	exports: [
		NgControlStatus
	],
	declarations: [
		TimeAgoPipe,
		TrimPipe,
		TrimLeftPipe,
		TrimRightPipe,
		CustomDatePipe,
		NgControlStatus,
		AppComponent,
		SpinnerComponent,
		AlertsComponent,
		NavComponent,
		LazyImageComponent,
		HomeComponent,
		MemberListComponent,
		MemberCardComponent,
		MemberDetailComponent,
		MemberEditComponent,
		MemberGalleryComponent,
		MemberMessagesComponent,
		MemberPhotoEditorComponent,
		MessagesComponent,
		ThreadsComponent,
		ThreadMessagesComponent,
		SignInComponent,
		SignUpComponent,
		WeatherComponent
	],
	providers: [
		HTTPHeadersInterceptorProvider,
		ErrorInterceptorProvider,
		HammerConfigProvider,
		Title,
		NgbAlertConfig,
		NgbToastConfig,
		NgbTooltipConfig,
		NgbPaginationConfig,
		NgbModalConfig,
		NgbProgressbarConfig,
		CountriesClient,
		WeatherClient,
		UserClient,
		AlertService,
		AuthGuard,
		UnsavedChangesGuard,
		MemberDetailResolver,
		MemberEditResolver,
		MemberPhotoEditorResolver
	],
	bootstrap: [
		AppComponent
	]
})
export class AppModule {
	constructor(faLibrary: FaIconLibrary) {
		faLibrary.addIcons();
		faLibrary.addIconPacks(fas, fab);
	}
}
