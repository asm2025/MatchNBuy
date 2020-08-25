import {
	Component,
	OnInit,
	OnDestroy,
	ViewChild,
	HostListener
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgForm } from "@angular/forms";
import { Subscription } from "rxjs";

import { IUser, IUserToUpdate } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-member-edit",
	templateUrl: "./member-edit.component.html",
	styleUrls: ["./member-edit.component.scss"]
})
export default class MemberEditComponent implements OnInit, OnDestroy {
	@ViewChild("editForm") editForm: NgForm;
	user: IUserToUpdate | null | undefined;
	photoUrl: string;

	private _routeSubscription: Subscription;
	private _photoSubscription: Subscription;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	@HostListener("window:beforeunload", ["$event"])
	unloadNotification($event: any) {
		if (this.editForm.dirty) $event.returnValue = true;
	}

	ngOnInit() {
		this._routeSubscription = this._route.data.subscribe(data => this.user = data["user"]);
		this._photoSubscription = this._userClient.photoUrl.subscribe(url => this.photoUrl = url);
	}

	ngOnDestroy() {
		this._routeSubscription.unsubscribe();
		this._photoSubscription.unsubscribe();
	}

	updateUser() {
		if (!this._userClient.user || !this.user) return;
		const id = (<IUser>this._userClient.user).id;
		this._userClient
			.update(id, this.user)
			.subscribe(
				() => {
					this._alertService.toasts.success("Profile updated successfully.");
					this.editForm.reset(this.user);
				},
				error => {
					this._alertService.toasts.error(error.toString());
				}
			);
	}

	updateUserPhoto(url: string | null | undefined) {
		//this.user.photoUrl = photoUrl;
	}
}
