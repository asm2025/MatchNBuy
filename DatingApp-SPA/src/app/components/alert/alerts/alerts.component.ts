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
			.subscribe(alert => {
				this.alerts.push(alert);
				if (!alert.delay || alert.delay < 50) return;
				setTimeout(() => this.closeAlert(alert), alert.delay);
			});
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
		let css = "p-1 bg-white text-dark ";

		switch (toast.type) {
			case AlertType.Fatal:
			case AlertType.Error:
				css += "border border-danger";
				break;
			case AlertType.Warning:
				css += "border border-warning";
				break;
			case AlertType.Success:
				css += "border border-success";
				break;
			case AlertType.Debug:
			case AlertType.Trace:
				css += "border border-secondary";
				break;
			default:
				css += "border border-info";
				break;
		}

		return css;
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
