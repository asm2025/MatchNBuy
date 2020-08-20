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

import AppComponent from "@/app.component";
import AlertComponent from "@components/alert/alert.component";
import ToastComponent from "@components/toast/toast.component";
import NavComponent from "@components/nav/nav.component";
import HomeComponent from "@pages/home/home.component";
import RegisterComponent from "@pages/home/register/register.component";
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
import WeatherComponent from "@pages/weather/weather.component";
import WeatherResolver from "@pages/weather/weather.resolver";

import CountriesClient from "@services/web/CountriesClient";
import WeatherClient from "@services/web/WeatherClient";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";
import { ErrorInterceptorProvider } from "@services/error.service";

export function tokenGetter() {
	return localStorage.getItem("token");
}

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
				tokenGetter,
				allowedDomains: ["localhost:8000"],
				disallowedRoutes: ["localhost:8000/Users/Login"]
			}
		})
	],
	declarations: [
		TimeAgoPipe,
		AppComponent,
		AlertComponent,
		ToastComponent,
		NavComponent,
		HomeComponent,
		RegisterComponent,
		ListsComponent,
		MemberListComponent,
		PhotoEditorComponent,
		MemberCardComponent,
		MemberEditComponent,
		MemberDetailComponent,
		MemberMessagesComponent,
		MessagesComponent,
		MessageThreadsComponent,
		WeatherComponent
	],
	providers: [
		{ provide: HAMMER_GESTURE_CONFIG, useClass: CustomHammerConfig },
		ErrorInterceptorProvider,
		NgbAlertConfig,
		NgbToastConfig,
		NgbPaginationConfig,
		NgbModalConfig,
		NgbProgressbarConfig,
		CountriesClient,
		WeatherClient,
		UserClient,
		ToastService,
		ListsResolver,
		MemberListResolver,
		MemberEditResolver,
		MemberEditUnsavedChanges,
		MemberDetailResolver,
		MessagesResolver,
		MessageThreadsResolver,
		WeatherResolver
	],
	bootstrap: [
		AppComponent
	]
})
export class AppModule { }
