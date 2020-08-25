import { Component, OnInit, OnDestroy } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, BehaviorSubject, ReplaySubject, Subscription } from "rxjs";

import { Genders } from "@data/common/Genders";
import { ICountry, ICity } from "@data/model/Country";
import { IUserToRegister } from "@data/model/User";
import UserClient from "@services/web/UserClient";
import CountriesClient from "@services/web/CountriesClient";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-sign-up",
	templateUrl: "./sign-up.component.html",
	styleUrls: ["./sign-up.component.scss"]
})
export default class SignUpComponent implements OnInit, OnDestroy {
	registrationInfo: IUserToRegister = {
		userName: "",
		password: "",
		email: "",
		phoneNumber: "",
		firstName: "",
		lastName: "",
		knownAs: "",
		dateOfBirth: new Date(),
		gender: Genders.NotSpecified,
		cityId: ""
	};

	confirmPassword = "";

	private readonly _countrySubject = new BehaviorSubject<string | null | undefined>(null);
	private readonly _selectedCountry = this._countrySubject.asObservable();
	private readonly _citiesSubject: ReplaySubject<ICity[]> = new ReplaySubject();
	private _selectedCountrySubscription: Subscription;

	countries: Observable<ICountry[]>;
	cities = this._citiesSubject.asObservable();

	constructor(private readonly _router: Router,
		private readonly _userClient: UserClient,
		private readonly _countriesClient: CountriesClient,
		private readonly _alertService: AlertService) {
	}

	ngOnInit(): void {
		this._selectedCountrySubscription = this._selectedCountry.subscribe(countryCode => {
			this.registrationInfo.cityId = "";

			if (!countryCode) {
				this._citiesSubject.next([]);
				return;
			}

			try {
				this._countriesClient.cities(countryCode)
					.subscribe((response: ICity[]) => this._citiesSubject.next(response),
						error => {
							this._citiesSubject.next([]);
							this._alertService.toasts.error(error.toString());
						});
			} catch (e) {
				this._citiesSubject.next([]);
				this._alertService.toasts.error(e.toString());
			}
		});

		this.countries = this._countriesClient.list();
	}

	ngOnDestroy(): void {
		this._citiesSubject.complete();
		this._selectedCountrySubscription.unsubscribe();
	}

	setKnownAs(value: string | null | undefined) {
		this.registrationInfo.knownAs = value || "";
	}

	selectCountry(value: ICountry | null | undefined) {
		if (!value || !value.code)
			this._countrySubject.next(null);
		else
			this._countrySubject.next(value.code);
	}

	register() {
		try {
			this._userClient.register(this.registrationInfo)
				.subscribe(() => {
					this._alertService.toasts.success("Logged in successfully.");
					this._router.navigate(["/members"]);
				},
					error => {
						this._alertService.alerts.error(error.toString());
					});
		} catch (e) {
			this._alertService.alerts.error(e.toString());
		}
	}
}
