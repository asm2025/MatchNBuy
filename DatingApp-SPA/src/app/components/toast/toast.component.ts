import { Component } from "@angular/core";
import alertUtil, { IAlert, AlertType, AlertTheme } from "@common/Alert";
import ToastService from "@services/toast.service";

@Component({
	selector: "app-toast",
	templateUrl: "./toast.component.html",
	styleUrls: ["./toast.component.scss"]
})
export class ToastComponent {
	constructor(private readonly _toastService: ToastService) {
	}

	protected getAlertType(alert: IAlert): string {
		const alertTheme = alertUtil.toAlertTheme((alert.type || AlertType.Default));
		return AlertTheme[alertTheme].toLowerCase();
	}

	protected isDismissible(alert: IAlert): boolean {
		return alertUtil.isDismissible(alert);
	}
}
