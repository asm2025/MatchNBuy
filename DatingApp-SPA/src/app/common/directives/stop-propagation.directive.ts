import { Directive, HostListener } from "@angular/core";

@Directive({
	selector: "[stop-propagation]"
})
export class StopPropagation {
	@HostListener("click", ["$event"])

	onClick(event: any) {
		event.stopPropagation();
	}
}
