import { Component, OnInit, OnDestroy } from "@angular/core";
import { Title } from "@angular/platform-browser";
import {
	Router,
	NavigationStart,
	NavigationEnd,
	NavigationCancel,
	NavigationError
} from "@angular/router";
import { ReplaySubject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import {
	NgbAlertConfig,
	NgbToastConfig,
	NgbTooltipConfig,
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
export default class AppComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	title = config.title;
	jwtHelper = new JwtHelperService();
	loading = false;

	constructor(alertConfig: NgbAlertConfig,
		toastConfig: NgbToastConfig,
		tooltipConfig: NgbTooltipConfig,
		paginationConfig: NgbPaginationConfig,
		modalConfig: NgbModalConfig,
		progressBarConfig: NgbProgressbarConfig,
		private readonly _titleService: Title,
		private readonly _router: Router,
		private readonly _userClient: UserClient) {

		// customize default values used by this component tree
		alertConfig.type = "info";

		toastConfig.autohide = true;
		toastConfig.delay = 1000;
		toastConfig.ariaLive = "polite";

		tooltipConfig.placement = "top-left";

		paginationConfig.boundaryLinks = true;
		paginationConfig.rotate = true;
		paginationConfig.ellipses = true;
		paginationConfig.directionLinks = true;
		paginationConfig.pageSize = 10;
		paginationConfig.maxSize = 5;

		modalConfig.centered = true;
		modalConfig.scrollable = true;

		progressBarConfig.showValue = true;

		this._titleService.setTitle(config.title);
		this._router
			.events
			.pipe(takeUntil(this.disposed$))
			.subscribe(event => {
				if (event instanceof NavigationStart) this.loading = true;

				if (event instanceof NavigationEnd
					|| event instanceof NavigationCancel
					|| event instanceof NavigationError) {
					this.loading = false;
				}
			});
	}

	ngOnInit() {
		this._userClient.init();
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}
}
