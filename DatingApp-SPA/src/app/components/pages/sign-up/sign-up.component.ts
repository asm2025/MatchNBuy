import { Component, OnInit, OnDestroy } from "@angular/core";
import { Router } from "@angular/router";
import {
	FormBuilder,
	FormGroup,
	FormControl,
	Validators
} from "@angular/forms";
import { Observable, BehaviorSubject, ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";

import { Genders } from "@data/common/Genders";
import { ICountry, ICity } from "@data/common/Country";
import { IUserToRegister } from "@data/model/User";
import CustomValidators from "@common/validation/custom-validators";
import FormHelper from "@common/helpers/form.helper";
import UserClient from "@services/web/UserClient";
import CountriesClient from "@services/web/CountriesClient";
import AlertService from "@services/alert.service";

@Component({
	selector: "app-sign-up",
	templateUrl: "./sign-up.component.html",
	styleUrls: ["./sign-up.component.scss"]
})
export default class SignUpComponent implements OnInit, OnDestroy {
	private readonly _countrySubject = new BehaviorSubject<string | null | undefined>(null);
	private readonly _selectedCountry = this._countrySubject.asObservable();
	private readonly _citiesSubject: ReplaySubject<ICity[]> = new ReplaySubject();
	private _formHelper: FormHelper;

	disposed$ = new ReplaySubject<boolean>();
	form$: FormGroup;
	countries: Observable<ICountry[]>;
	cities = this._citiesSubject.asObservable();

	get formHelper(): FormHelper {
		return this._formHelper;
	}

	constructor(private readonly _router: Router,
		private readonly _fb: FormBuilder,
		private readonly _userClient: UserClient,
		private readonly _countriesClient: CountriesClient,
		private readonly _alertService: AlertService) {
	}

	ngOnInit(): void {
		this.form$ = this._fb.group({
			userName: [
				null, Validators.compose([
					Validators.required,
					Validators.maxLength(128)
				])
			],
			password: [
				null, Validators.compose([
					Validators.required,
					Validators.minLength(8),
					Validators.maxLength(128)
				])
			],
			confirmPassword: [null, Validators.required],
			email: [
				null, Validators.compose([
					Validators.required,
					Validators.email,
					Validators.maxLength(2048)
				])
			],
			phoneNumber: [null, Validators.maxLength(10)],
			firstName: [
				null, Validators.compose([
					Validators.required,
					Validators.maxLength(128)
				])
			],
			lastName: [null, Validators.maxLength(128)],
			knownAs: [null, Validators.maxLength(128)],
			dateOfBirth: [null, Validators.required],
			gender: [Genders.NotSpecified, Validators.required],
			countryCode: [null, Validators.required],
			cityId: [null, Validators.required],
			introduction: [null],
			lookingFor: [null]
		}, { validators: CustomValidators.compare("password", "confirmPassword") });
		this._formHelper = new FormHelper(this.form$);
		this._selectedCountry
			.pipe(takeUntil(this.disposed$))
			.subscribe(countryCode => {
				this.form$.patchValue({ cityId: null });

				if (!countryCode) {
					this._citiesSubject.next([]);
					return;
				}

				this._countriesClient
					.cities(countryCode)
					.pipe(catchError(error => {
						this._alertService.toasts.error(error.toString());
						return of([]);
					}))
					.subscribe((response: ICity[]) => this._citiesSubject.next(response));
			});

		this.countries = this._countriesClient.list();
	}

	ngOnDestroy(): void {
		this._citiesSubject.complete();
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	setKnownAs(value: string | null | undefined, valueType: string) {
		const knownAsControl = FormHelper.getControl<FormControl>(this.form$, "knownAs");
		if (knownAsControl.dirty || knownAsControl.touched) return;

		let nameControl: FormControl;

		switch (valueType) {
			case "firstName":
				nameControl = FormHelper.getControl<FormControl>(this.form$, "lastName");
				value = ((value || "") + " " + (nameControl.value || "")).trim();
				break;
			case "lastName":
				nameControl = FormHelper.getControl<FormControl>(this.form$, "firstName");
				value = ((nameControl.value || "") + " " + (value || "")).trim();
				break;
			default:
				throw new Error("Invalid value type.");
		}

		this.form$.patchValue({ knownAs: value });
	}

	selectCountry(value: ICountry | null | undefined) {
		if (!value || !value.code)
			this._countrySubject.next(null);
		else
			this._countrySubject.next(value.code);
	}

	register(registrationInfo: IUserToRegister) {
		if (FormHelper.isFormInvalid(this.form$)) return;
		this._userClient.register(registrationInfo)
			.pipe(catchError(error => {
				this._alertService.alerts.error(error.toString());
				return of(null);
			}))
			.subscribe((res: any) => {
				if (!res) return;
				this._router.navigate(["/members"]);
			});
	}
}
