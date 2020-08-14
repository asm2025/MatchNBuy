import { Component, OnInit } from "@angular/core";
import { setTheme } from "ngx-bootstrap/utils";
import { NgbPaginationConfig } from "@ng-bootstrap/ng-bootstrap";
import { JwtHelperService } from "@auth0/angular-jwt";

import UserClient from "@services/web/UserClient";
import { IUser } from "@data/model/User";

import config from "@/config.json";

@Component({
    selector: "app-root",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"]
})
export class AppComponent implements OnInit {
    title = config.title;
	jwtHelper = new JwtHelperService();

	constructor(paginationConfig: NgbPaginationConfig, private readonly _userClient: UserClient) {
		// use bootstrap 4 for ngx-bootstrap
		setTheme("bs4");

		// customize default values of pagination used by this component tree
		paginationConfig.boundaryLinks = true;
		paginationConfig.rotate = true;
		paginationConfig.ellipses = true;
		paginationConfig.directionLinks = true;
		paginationConfig.pageSize = 10;
		paginationConfig.maxSize = 5;
	}

	ngOnInit() {
		const jsonToken = localStorage.getItem("token");
		this._userClient.token = jsonToken ? this.jwtHelper.decodeToken(jsonToken) : null;

		const jsonUser = jsonToken ? localStorage.getItem("user") : null;
		const user: IUser | null | undefined = jsonUser ? JSON.parse(jsonUser) as IUser : null;
		this._userClient.user = user;
		this._userClient.changeMemberPhoto(user ? user.photoUrl : null);
	}
}
