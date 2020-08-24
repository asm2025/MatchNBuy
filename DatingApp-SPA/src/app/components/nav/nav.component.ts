import { Component, OnInit, OnDestroy } from "@angular/core";
import { Router } from "@angular/router";
import { Subscription } from "rxjs";

import UserClient from "@services/web/UserClient";

import config from "@/config.json";

@Component({
	selector: "app-nav",
	templateUrl: "./nav.component.html",
	styleUrls: ["./nav.component.scss"]
})
export default class NavComponent implements OnInit, OnDestroy {
	photoUrl: string;
	title = config.title;

	private _photoUrlSubscription: Subscription;

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		this._photoUrlSubscription = this._userClient.photoUrl.subscribe(url => this.photoUrl = url);
	}

	ngOnDestroy() {
		this._photoUrlSubscription.unsubscribe();
	}

	logout() {
		this._userClient.logout();
		this._router.navigate(["/"]);
	}

	isSignedIn(): boolean {
		return this._userClient.isSignedIn();
	}
}
