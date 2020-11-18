import {
	Component,
	OnInit,
	OnDestroy,
	ViewChild,
	HostListener
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgForm } from "@angular/forms";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";

import { IUser, IUserToUpdate } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-member-edit",
	templateUrl: "./member-edit.component.html",
	styleUrls: ["./member-edit.component.scss"]
})
export default class MemberEditComponent implements OnInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	user: IUserToUpdate | null | undefined;
	photoUrl: string;

	@ViewChild("editForm") editForm: NgForm;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	@HostListener("window:beforeunload", ["$event"])
	unloadNotification($event: any) {
		if (this.editForm && this.editForm.dirty === true) $event.returnValue = true;
	}

	ngOnInit() {
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => this.user = data["user"]);
		this._userClient
			.photoUrl
			.pipe(takeUntil(this.disposed$))
			.subscribe(url => this.photoUrl = url);
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	updateUser() {
		if (!this._userClient.user || !this.user) return;
		const id = (<IUser>this._userClient.user).id;
		this._userClient
			.update(id, this.user)
			.pipe(catchError(error => {
				this._alertService.alerts.error(error.toString());
				return of(null);
			}))
			.subscribe(() => {
					this._alertService.toasts.success("Profile updated successfully.");
					this.editForm.reset(this.user);
				});
	}

	updateUserPhoto(url: string | null | undefined) {
		this._userClient.setPhotoUrl(url);
	}
}
