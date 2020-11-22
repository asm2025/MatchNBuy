import { Component, OnInit, OnDestroy } from "@angular/core";
import { Router } from "@angular/router";
import { ReplaySubject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import UserClient from "@services/web/UserClient";

import config from "@/config.json";

@Component({
	selector: "app-nav",
	templateUrl: "./nav.component.html",
	styleUrls: ["./nav.component.scss"]
})
export default class NavComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	photoUrl: string;
	title = config.title;

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient) {
	}

	ngOnInit() {
		this._userClient
			.user
			.pipe(takeUntil(this.disposed$))
			.subscribe(() => this.photoUrl = this._userClient.photoUrl);
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	logout() {
		this._userClient.logout();
		this._router.navigate(["/"]);
	}

	isSignedIn(): boolean {
		return this._userClient.isSignedIn();
	}
}
