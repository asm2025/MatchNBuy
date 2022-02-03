import { Component, Input } from "@angular/core";

import config from "@app/config.json";

@Component({
	selector: "app-lazy-image",
	templateUrl: "./lazy-image.component.html",
	styleUrls: ["./lazy-image.component.scss"]
})
export default class LazyImageComponent {
	spinnerImage = <string>config.images.spinner;
	@Input()
	src: string | null | undefined;
	@Input()
	err: string | null | undefined = null;
	@Input()
	cssClass: string | null | undefined = null;
	@Input()
	alt: string | null | undefined = null;
	@Input()
	responsive: boolean | null | undefined = null;
}
