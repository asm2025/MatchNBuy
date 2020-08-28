import { Component, TemplateRef, OnInit, OnDestroy } from "@angular/core";
import {
	fadeInRightOnEnterAnimation,
	fadeInUpOnEnterAnimation,
	fadeOutOnLeaveAnimation,
	fadeOutDownOnLeaveAnimation
} from "angular-animations";
import { ReplaySubject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import alertUtil, { IAlert, IToast, AlertType, AlertTheme } from "@common/Alert";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-alerts",
	templateUrl: "./alerts.component.html",
	styleUrls: ["./alerts.component.scss"],
	animations: [
		fadeInRightOnEnterAnimation(),
		fadeInUpOnEnterAnimation(),
		fadeOutOnLeaveAnimation(),
		fadeOutDownOnLeaveAnimation()
	]
})
export default class AlertsGlobalComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	alerts: IAlert[] = [];
	toasts: IToast[] = [];

	constructor(public readonly service: AlertService) {
	}

	ngOnInit(): void {
		this.service
			.alertsChanges
			.pipe(takeUntil(this.disposed$))
			.subscribe(alert => this.alerts.push(alert));
		this.service
			.toastsChanges
			.pipe(takeUntil(this.disposed$))
			.subscribe(toast => this.toasts.push(toast));
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	protected getAlertType(alert: IAlert): string {
		const alertTheme = alertUtil.toAlertTheme((alert.type || AlertType.Default));
		return AlertTheme[alertTheme].toLowerCase();
	}

	protected closeAlert(alert: IAlert) {
		this.alerts = this.alerts.filter(e => e !== alert);
	}

	protected getToastCssClass(toast: IToast): string {
		switch (toast.type) {
		case AlertType.Fatal:
		case AlertType.Error:
			return "bg-danger text-white";
		case AlertType.Warning:
			return "bg-warning text-dark";
		case AlertType.Success:
			return "bg-success text-white";
		case AlertType.Debug:
		case AlertType.Trace:
			return "bg-secondary text-white";
		default:
			return "bg-info text-white";
		}
	}

	protected closeToast(toast: IAlert) {
		this.toasts = this.toasts.filter(e => e !== toast);
	}

	protected isTemplate(alert: IAlert) {
		return alert.content instanceof TemplateRef;
	}

	protected isDismissible(alert: IAlert): boolean {
		return alertUtil.isDismissible(alert);
	}
}
