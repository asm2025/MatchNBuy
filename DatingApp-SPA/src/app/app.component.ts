import { Component, OnInit } from "@angular/core";
import {
	NgbAlertConfig,
	NgbToastConfig,
	NgbPaginationConfig,
	NgbModalConfig,
	NgbProgressbarConfig
} from "@ng-bootstrap/ng-bootstrap";
import { JwtHelperService } from "@auth0/angular-jwt";

import UserClient from "@services/web/UserClient";

import config from "@/config.json";

@Component({
    selector: "app-root",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"],
	host: { '[class.ngb-toasts]': "true" }
})
export default class AppComponent implements OnInit {
    title = config.title;
	jwtHelper = new JwtHelperService();

	constructor(alertConfig: NgbAlertConfig,
		toastConfig: NgbToastConfig,
		paginationConfig: NgbPaginationConfig,
		modalConfig: NgbModalConfig,
		progressBarConfig: NgbProgressbarConfig,
		private readonly _userClient: UserClient) {

		// customize default values used by this component tree
		alertConfig.type = "info";

		toastConfig.autohide = true;
		toastConfig.delay = 1000;
		toastConfig.ariaLive = "polite";

		paginationConfig.boundaryLinks = true;
		paginationConfig.rotate = true;
		paginationConfig.ellipses = true;
		paginationConfig.directionLinks = true;
		paginationConfig.pageSize = 10;
		paginationConfig.maxSize = 5;

		modalConfig.centered = true;
		modalConfig.scrollable = true;

		progressBarConfig.showValue = true;
	}

	ngOnInit() {
		this._userClient.init();
	}
}
