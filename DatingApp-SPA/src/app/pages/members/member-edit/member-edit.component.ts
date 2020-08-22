import {
	Component,
	OnInit,
	ViewChild,
	HostListener
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgForm } from "@angular/forms";

import { IUserToUpdate } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import ToastService from "@services/toast.service";

@Component({
	selector: "app-member-edit",
	templateUrl: "./member-edit.component.html",
	styleUrls: ["./member-edit.component.scss"]
})
export default class MemberEditComponent implements OnInit {
	@ViewChild("editForm") editForm: NgForm;
	user: IUserToUpdate | null | undefined;
	photoUrl: string;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _toastService: ToastService) {
	}

	@HostListener("window:beforeunload", ["$event"])
	unloadNotification($event: any) {
		if (this.editForm.dirty) $event.returnValue = true;
	}

	ngOnInit() {
		this._route.data.subscribe(data => this.user = data["user"]);
		this._userClient.photoUrl.subscribe(url => this.photoUrl = url);
	}

	updateUser() {
		if (!this.user) return;
		this._userClient
			.update(this._userClient.token.nameid, this.user)
			.subscribe(
				() => {
					this._toastService.success("Profile updated successfully.");
					this.editForm.reset(this.user);
				},
				error => {
					this._toastService.error(error.toString());
				}
			);
	}

	updateUserPhoto(url: string | null | undefined) {
		//this.user.photoUrl = photoUrl;
	}
}
