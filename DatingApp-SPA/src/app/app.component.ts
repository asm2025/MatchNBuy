import { Component } from "@angular/core";
//import { setTheme } from "ngx-bootstrap/utils";
import { NgbPaginationConfig } from "@ng-bootstrap/ng-bootstrap";

@Component({
    selector: "app-root",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.css"]
})
export class AppComponent {
    title = "Dating App";

	constructor(paginationConfig: NgbPaginationConfig) {
		// use bootstrap 4 for ngx-bootstrap
		//setTheme("bs4");

		// customize default values of pagination used by this component tree
		paginationConfig.boundaryLinks = true;
		paginationConfig.rotate = true;
		paginationConfig.ellipses = true;
		paginationConfig.directionLinks = true;
		paginationConfig.pageSize = 10;
		paginationConfig.maxSize = 5;
	}
}
