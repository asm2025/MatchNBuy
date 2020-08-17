import { Component, OnInit } from "@angular/core";
import UserClient from "@services/web/UserClient";

@Component({
	selector: "app-home",
	templateUrl: "./home.component.html",
	styleUrls: ["./home.component.scss"]
})
export class HomeComponent implements OnInit {
	private _registerMode = false;

	constructor(private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		//
	}

	registerToggle() {
		this._registerMode = true;
	}

	cancelRegisterMode(registerMode: boolean) {
		this._registerMode = registerMode;
	}
}
