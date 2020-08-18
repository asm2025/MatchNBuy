import { Component, TemplateRef } from "@angular/core";
import ToastService from "@services/toast.service";

@Component({
	selector: "app-toast",
	templateUrl: "./toast.component.html",
	styleUrls: ["./toast.component.scss"],
	host: {'[class.ngb-toasts]': "true"}
})
export class ToastComponent {
	constructor(private readonly _toastService: ToastService) {
	}

	success(message: string | TemplateRef<any>) {
		this._toastService.success(message);
	}

	error(message: string | TemplateRef<any>) {
		this._toastService.error(message);
	}

	warning(message: string | TemplateRef<any>) {
		this._toastService.warning(message);
	}

	alert(message: string | TemplateRef<any>) {
		this._toastService.alert(message);
	}
}
