import { Component, TemplateRef, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs";

import alertUtil, { IAlert, IToast, AlertType, AlertTheme } from "@common/Alert";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-alerts",
	templateUrl: "./alerts.component.html",
	styleUrls: ["./alerts.component.scss"],
})
export default class AlertsGlobalComponent implements OnInit, OnDestroy {
	alerts: IAlert[] = [];
	toasts: IToast[] = [];

	private _alertsSubscription: Subscription;
	private _toastsSubscription: Subscription;

	constructor(public readonly service: AlertService) {
	}

	ngOnInit(): void {
		this._alertsSubscription = this.service
			.alertsChanges
			.subscribe(alert => this.alerts.push(alert));
		this._toastsSubscription = this.service
			.toastsChanges
			.subscribe(toast => this.toasts.push(toast));
	}

	ngOnDestroy(): void {
		this._alertsSubscription.unsubscribe();
		this._toastsSubscription.unsubscribe();
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
