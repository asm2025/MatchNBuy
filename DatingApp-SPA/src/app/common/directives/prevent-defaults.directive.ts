import { Directive, HostListener } from "@angular/core";

@Directive({
	selector: "[prevent-defaults]"
})
export class PreventDefaults {
	@HostListener("click", ["$event"])

	onClick(event: any) {
		event.preventDefaults();
	}
}
