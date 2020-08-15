import { Injectable, TemplateRef } from "@angular/core";
import { IAlert, AlertType } from "@common/Alert";

const TIMEOUT = 1000;

@Injectable({
	providedIn: "root"
})
export default class ToastService {
	toasts: IAlert[] = [];

	success(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Success,
			content: message,
			dismissable: true,
			delay: TIMEOUT
		});
	}

	error(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Error,
			content: message,
			dismissable: true,
			delay: TIMEOUT
		});
	}

	warning(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Warning,
			content: message,
			dismissable: true,
			delay: TIMEOUT
		});
	}

	alert(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Information,
			content: message,
			dismissable: true,
			delay: TIMEOUT
		});
	}

	remove(alert: IAlert) {
		this.toasts = this.toasts.filter(e => e !== alert);
	}

	clear() {
		this.toasts.length = 0;
	}
}
