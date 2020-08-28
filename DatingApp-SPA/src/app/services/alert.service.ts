import { Injectable, TemplateRef, OnDestroy } from "@angular/core";
import { ReplaySubject } from "rxjs";

import { IAlert, IToast, AlertType } from "@common/Alert";

@Injectable({
	providedIn: "root"
})
export default class AlertService implements OnDestroy {
	private _alerts: ReplaySubject<IAlert> = new ReplaySubject();
	private _toasts: ReplaySubject<IToast> = new ReplaySubject();

	readonly alertsChanges = this._alerts.asObservable();
	readonly toastsChanges = this._toasts.asObservable();

	ngOnDestroy(): void {
		this._alerts.complete();
		this._toasts.complete();
	}

	alerts = new class {
		constructor(public readonly service: AlertService) {
		}

		success(message: string | TemplateRef<any>) {
			this.service._alerts.next({
				type: AlertType.Success,
				content: message
			});
		}

		error(message: string | TemplateRef<any>) {
			this.service._alerts.next({
				type: AlertType.Error,
				content: message
			});
		}

		warning(message: string | TemplateRef<any>) {
			this.service._alerts.next({
				type: AlertType.Warning,
				content: message
			});
		}

		info(message: string | TemplateRef<any>) {
			this.service._alerts.next({
				type: AlertType.Information,
				content: message
			});
		}

		log(message: string | TemplateRef<any>) {
			this.service._alerts.next({
				type: AlertType.Debug,
				content: message
			});
		}
	}(this);

	toasts = new class {
		constructor(public readonly service: AlertService) {
		}

		success(message: string | TemplateRef<any>, delay?: number | null | undefined) {
			this.service._toasts.next({
				type: AlertType.Success,
				title: "Success",
				content: message,
				delay: delay
			});
		}

		error(message: string | TemplateRef<any>, delay?: number | null | undefined) {
			this.service._toasts.next({
				type: AlertType.Error,
				title: "Error",
				content: message,
				delay: delay
			});
		}

		warning(message: string | TemplateRef<any>, delay?: number | null | undefined) {
			this.service._toasts.next({
				type: AlertType.Warning,
				title: "Warning",
				content: message,
				delay: delay
			});
		}

		info(message: string | TemplateRef<any>, delay?: number | null | undefined) {
			this.service._toasts.next({
				type: AlertType.Information,
				title: "Information",
				content: message,
				delay: delay
			});
		}

		log(message: string | TemplateRef<any>, delay?: number | null | undefined) {
			this.service._toasts.next({
				type: AlertType.Debug,
				title: "Log",
				content: message,
				delay: delay
			});
		}
	}(this);
}
