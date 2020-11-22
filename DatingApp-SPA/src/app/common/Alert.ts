import { TemplateRef } from "@angular/core";

export enum AlertType {
	Default = 0,
	Trace = 1,
	Debug = 2,
	Information = 3,
	Success = 4,
	Warning = 5,
	Error = 6,
	Fatal = 7
}

export enum AlertTheme {
	Light = 0,
	Dark = 1,
	Secondary = 2,
	Primary = 3,
	Info = 4,
	Success = 5,
	Warning = 6,
	Danger = 7
}

export interface IAlert {
	type?: AlertType;
	content: string | TemplateRef<any>;
	dismissible?: boolean | null | undefined;
	delay?: number | null | undefined;
}

export interface IToast extends IAlert {
	title?: string | null | undefined;
}

const alertUtil = {
	toAlertTheme: (alertType: AlertType): AlertTheme => {
		let type: AlertTheme;

		switch (alertType) {
			case AlertType.Trace:
			case AlertType.Debug:
				type = AlertTheme.Secondary;
				break;
			case AlertType.Information:
				type = AlertTheme.Info;
				break;
			case AlertType.Success:
				type = AlertTheme.Success;
				break;
			case AlertType.Warning:
				type = AlertTheme.Warning;
				break;
			case AlertType.Error:
			case AlertType.Fatal:
				type = AlertTheme.Danger;
				break;
			default:
				type = AlertTheme.Light;
				break;
		}

		return type;
	},
	toAlertType: (alertTheme: AlertTheme): AlertType => {
		let type: AlertType;

		switch (alertTheme) {
			case AlertTheme.Light:
			case AlertTheme.Dark:
			case AlertTheme.Secondary:
			case AlertTheme.Primary:
				type = AlertType.Debug;
				break;
			case AlertTheme.Info:
				type = AlertType.Information;
				break;
			case AlertTheme.Success:
				type = AlertType.Success;
				break;
			case AlertTheme.Warning:
				type = AlertType.Warning;
				break;
			case AlertTheme.Danger:
				type = AlertType.Error;
				break;
			default:
				type = AlertType.Default;
				break;
		}

		return type;
	},
	isDismissible: (alert: IAlert): boolean => {
		return alert.dismissible === undefined || alert.dismissible === null || alert.dismissible;
	}

};

export default alertUtil;
