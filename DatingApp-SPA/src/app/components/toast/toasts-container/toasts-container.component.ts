import { Component, TemplateRef } from "@angular/core";

import alertUtil, { IAlert, AlertType } from "@common/Alert";
import ToastService from "@services/toast.service";

@Component({
	selector: "app-toasts-container",
	templateUrl: "./toasts-container.component.html",
	styleUrls: ["./toasts-container.component.scss"],
	host: { "[class.ngb-toasts]": "true" }
})
export default class ToastsContainerComponent {
	constructor(protected readonly _toastService: ToastService) {
	}

	protected isTemplate(toast: IAlert) {
		return toast.content instanceof TemplateRef;
	}

	protected getToastCssClass(toast: IAlert): string {
		let css = "p-3 mb-2 ";

		switch (toast.type) {
			case AlertType.Fatal:
			case AlertType.Error:
				css += "bg-danger text-white";
				break;
			case AlertType.Warning:
				css += "bg-warning text-dark";
				break;
			case AlertType.Success:
				css += "bg-success text-white";
				break;
			case AlertType.Debug:
			case AlertType.Trace:
				css += "bg-secondary text-white";
				break;
			default:
				css += "bg-info text-white";
				break;
		}

		return css;
	}

	protected isDismissible(toast: IAlert): boolean {
		return alertUtil.isDismissible(toast);
	}
}
