import { Component, Input } from "@angular/core";
import alertUtil, { IAlert, AlertType, AlertTheme } from "@common/Alert";

@Component({
	selector: "app-alert",
	templateUrl: "./alert.component.html",
	styleUrls: ["./alert.component.scss"]
})
export class AlertComponent {
	@Input() alerts: IAlert[];

	protected getAlertType(alert: IAlert): string {
		const alertTheme = alertUtil.toAlertTheme((alert.type || AlertType.Default));
		return AlertTheme[alertTheme].toLowerCase();
	}

	protected isDismissible(alert: IAlert): boolean {
		return alertUtil.isDismissible(alert);
	}

	protected close(alert: IAlert): void {
		if (!alert || !this.alerts || this.alerts.length === 0) return;
		this.alerts = this.alerts.filter(e => e !== alert);
	}
}
