import { NgModule, Injectable } from "@angular/core";
import {
	BrowserModule,
	Title,
	HammerGestureConfig,
	HAMMER_GESTURE_CONFIG
} from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";

import { JwtModule } from "@auth0/angular-jwt";
import { LazyLoadImageModule } from "ng-lazyload-image";
import {
	NgbModule,
	NgbAlertConfig,
	NgbToastConfig,
	NgbTooltipConfig,
	NgbPaginationConfig,
	NgbModalConfig,
	NgbProgressbarConfig
} from "@ng-bootstrap/ng-bootstrap";
import { NgxGalleryModule } from "ngx-gallery-9";
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

import { NgControlStatus } from "@common/directives/ng_control_status";

import AppComponent from "./app.component";
import SpinnerComponent from "@components/spinner/spinner.component";
import AlertsComponent from "@components/alert/alerts/alerts.component";
import NavComponent from "@components/nav/nav.component";
import LazyImageComponent from "@components/lazy-image/lazy-image.component";

import HomeComponent from "@components/pages/home/home.component";

import SignInComponent from "@components/pages/sign-in/sign-in.component";
import SignUpComponent from "@components/pages/sign-up/sign-up.component";

import ListsComponent from "@components/pages/lists/lists.component";
import ListsResolver from "@components/pages/lists/lists.resolver";
import MemberListComponent from "@components/pages/members/member-list/member-list.component";
import PhotoEditorComponent from "@components/pages/members/photo-editor/photo-editor.component";
import MemberCardComponent from "@components/pages/members/member-card/member-card.component";
import MemberEditComponent from "@components/pages/members/member-edit/member-edit.component";
import MemberEditResolver from "@components/pages/members/member-edit/member-edit.resolver";
import MemberEditUnsavedChanges from "@components/pages/members/member-edit/member-edit-unsaved-changes.guard";
import MemberDetailComponent from "@components/pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@components/pages/members/member-detail/member-detail.resolver";
import MemberMessagesComponent from "@components/pages/members/member-messages/member-messages.component";

import MessagesComponent from "@components/pages/messages/messages.component";
import MessagesResolver from "@components/pages/messages/messages.resolver";
import MessageThreadsComponent from "@components/pages/messages/message-threads/message-threads.component";
import MessageThreadsResolver from "@components/pages/messages/message-threads/message-threads.resolver";
import ThreadMessagesComponent from "@components/pages/messages/thread-messages/thread-messages.component";
import ThreadMessagesResolver from "@components/pages/messages/thread-messages/thread-messages.resolver";

import WeatherComponent from "@components/pages/weather/weather.component";
import WeatherResolver from "@components/pages/weather/weather.resolver";

import CountriesClient from "@services/web/CountriesClient";
import WeatherClient from "@services/web/WeatherClient";
import UserClient, { getToken } from "@services/web/UserClient";
import AlertService from "@services/alert.service";
import { ErrorInterceptorProvider } from "@services/error.service";

@Injectable()
export class CustomHammerConfig extends HammerGestureConfig {
	overrides = {
		pinch: { enable: false },
		rotate: { enable: false }
	};
}

@NgModule({
	imports: [
		BrowserModule,
		BrowserAnimationsModule,
		FormsModule,
		ReactiveFormsModule,
		HttpClientModule,
		LazyLoadImageModule,
		FontAwesomeModule,
		//MatIconModule,
		NgSelectModule,
		NgOptionHighlightModule,
		NgxGalleryModule,
		FileUploadModule,
		NgbModule,
		JwtModule.forRoot({
			config: {
				tokenGetter: getToken,
				allowedDomains: ["localhost:8000"],
				disallowedRoutes: ["localhost:8000/Users/Login"]
			}
		}),
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
		NgControlStatus,
		AppComponent,
		SpinnerComponent,
		AlertsComponent,
		NavComponent,
		LazyImageComponent,
		HomeComponent,
		SignInComponent,
		SignUpComponent,
		ListsComponent,
		MemberListComponent,
		PhotoEditorComponent,
		MemberCardComponent,
		MemberEditComponent,
		MemberDetailComponent,
		MemberMessagesComponent,
		MessagesComponent,
		MessageThreadsComponent,
		ThreadMessagesComponent,
		WeatherComponent
	],
	providers: [
		{ provide: HAMMER_GESTURE_CONFIG, useClass: CustomHammerConfig },
		Title,
		ErrorInterceptorProvider,
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
		ListsResolver,
		MemberEditResolver,
		MemberEditUnsavedChanges,
		MemberDetailResolver,
		MessagesResolver,
		MessageThreadsResolver,
		ThreadMessagesResolver,
		WeatherResolver
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
