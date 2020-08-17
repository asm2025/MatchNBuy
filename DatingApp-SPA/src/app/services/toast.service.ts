import { Injectable, TemplateRef } from "@angular/core";
import { IAlert, AlertType } from "@common/Alert";

@Injectable({
	providedIn: "root"
})
export default class ToastService {
	toasts: IAlert[] = [];

	success(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Success,
			content: message
		});
	}

	error(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Error,
			content: message
		});
	}

	warning(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Warning,
			content: message
		});
	}

	alert(message: string | TemplateRef<any>) {
		this.toasts.push({
			type: AlertType.Information,
			content: message
		});
	}

	remove(alert: IAlert) {
		this.toasts = this.toasts.filter(e => e !== alert);
	}

	clear() {
		this.toasts.length = 0;
	}
}
