import {
	Component,
	OnInit,
	AfterViewInit,
	OnDestroy,
	ViewChild,
	HostListener
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgForm } from "@angular/forms";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";

import { IUserToUpdate } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";
import { IIsDirty } from "@common/guards/unsaved-changes.guard";

@Component({
	selector: "app-member-edit",
	templateUrl: "./member-edit.component.html",
	styleUrls: ["./member-edit.component.scss"]
})
export default class MemberEditComponent implements OnInit, AfterViewInit, OnDestroy, IIsDirty {
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
		if (this.isDirty()) $event.returnValue = true;
	}

	ngOnInit() {
		this._userClient
			.user
			.pipe(takeUntil(this.disposed$))
			.subscribe(() => this.photoUrl = this._userClient.photoUrl);
	}

	ngAfterViewInit() {
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => this.user = data["user"]);
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

    isDirty(): boolean {
		return this.editForm && this.editForm.dirty === true;
	}

	updateUser() {
		if (!this.user) return;
		const userId = this._userClient.userId;
		if (!userId) return;
		this._userClient
			.update(userId, this.user)
			.pipe(catchError(error => {
				this._alertService.alerts.error(error.toString());
				return of(null);
			}))
			.subscribe((res: IUserToUpdate | null | undefined) => {
				if (!res) return;
				this._alertService.toasts.success("Profile updated successfully.");
					this.user = res;
					this.editForm.reset(this.user);
				});
	}

	updateUserPhoto(url: string | null | undefined) {
		this._userClient.photoUrl = url || "";
	}
}
