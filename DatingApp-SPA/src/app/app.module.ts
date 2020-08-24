import { NgModule, Injectable } from "@angular/core";
import {
	BrowserModule,
	Title,
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

import AppComponent from "./app.component";
import AlertsComponent from "@components/alert/alerts/alerts.component";
import NavComponent from "@components/nav/nav.component";

import HomeComponent from "@pages/home/home.component";

import SignInComponent from "@pages/sign-in/sign-in.component";
import SignUpComponent from "@pages/sign-up/sign-up.component";

import ListsComponent from "@pages/lists/lists.component";
import ListsResolver from "@pages/lists/lists.resolver";
import MemberListComponent from "@pages/members/member-list/member-list.component";
import MemberListResolver from "@pages/members/member-list/member-list.resolver";
import PhotoEditorComponent from "@pages/members/photo-editor/photo-editor.component";
import MemberCardComponent from "@pages/members/member-card/member-card.component";
import MemberEditComponent from "@pages/members/member-edit/member-edit.component";
import MemberEditResolver from "@pages/members/member-edit/member-edit.resolver";
import MemberEditUnsavedChanges from "@pages/members/member-edit/member-edit-unsaved-changes.guard";
import MemberDetailComponent from "@pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@pages/members/member-detail/member-detail.resolver";
import MemberMessagesComponent from "@pages/members/member-messages/member-messages.component";

import MessagesComponent from "@pages/messages/messages.component";
import MessagesResolver from "@pages/messages/messages.resolver";
import MessageThreadsComponent from "@pages/messages/message-threads/message-threads.component";
import MessageThreadsResolver from "@pages/messages/message-threads/message-threads.resolver";
import ThreadMessagesComponent from "@pages/messages/thread-messages/thread-messages.component";
import ThreadMessagesResolver from "@pages/messages/thread-messages/thread-messages.resolver";

import WeatherComponent from "@pages/weather/weather.component";
import WeatherResolver from "@pages/weather/weather.resolver";

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
		RouterModule.forRoot(appRoutes),
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		FileUploadModule,
		NgbModule,
		MatIconModule,
		JwtModule.forRoot({
			config: {
				tokenGetter: getToken,
				allowedDomains: ["localhost:8000"],
				disallowedRoutes: ["localhost:8000/Users/Login"]
			}
		})
	],
	declarations: [
		TimeAgoPipe,
		AppComponent,
		AlertsComponent,
		NavComponent,
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
		NgbPaginationConfig,
		NgbModalConfig,
		NgbProgressbarConfig,
		CountriesClient,
		WeatherClient,
		UserClient,
		AlertService,
		ListsResolver,
		MemberListResolver,
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
export class AppModule { }
