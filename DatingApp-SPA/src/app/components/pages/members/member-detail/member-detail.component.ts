import { Component, OnInit, AfterViewInit, OnDestroy, Input } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import "hammerjs";
import {
	NgxGalleryOptions,
	NgxGalleryImage,
	NgxGalleryAnimation
} from "ngx-gallery-9";

import Range from "@common/collections/Range";
import { IUserForDetails } from "@data/model/User";
import { IPaginated } from "@common/pagination/Paginated";
import { ISortablePagination } from "@common/pagination/SortablePagination";
import { SortType } from "@common/sorting/SortType";
import { IPhoto } from "@data/model/Photo";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

import config from "@/config.json";

const TABS_MIN = 1;
const TABS_MAX = 3;

@Component({
	selector: "app-member-detail",
	templateUrl: "./member-detail.component.html",
	styleUrls: ["./member-detail.component.scss"]
})
export default class MemberDetailComponent implements OnInit, AfterViewInit, OnDestroy {
	disposed$ = new ReplaySubject<boolean>();
	user: IUserForDetails | null | undefined;
	activeTab = TABS_MIN;
	galleryOptions: NgxGalleryOptions[];
	images: NgxGalleryImage[];
	imagesPagination: ISortablePagination = {
		page: 1,
		pageSize: 12,
		orderBy: [
			{
				name: "dateAdded",
				type: SortType.Descending
			}
		]
	};

	@Input() id: string;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService) {
	}

	get hasUser(): boolean {
		return this.user !== null && this.user !== undefined;
	}

	ngOnInit() {
		this.galleryOptions = [
			{
				width: "500px",
				height: "500px",
				imagePercent: 100,
				thumbnailsColumns: 4,
				imageAnimation: NgxGalleryAnimation.Slide,
				preview: false
			}
		];
	}

	ngAfterViewInit() {
		this._route
			.queryParams
			.pipe(takeUntil(this.disposed$))
			.subscribe(params => {
				setTimeout(() => {
					if (!this.id && params["id"]) {
						this.id = params["id"];
					}

					let tab: number;

					try {
						tab = parseInt(params["tab"] || TABS_MIN.toString());
					} catch (e) {
						tab = TABS_MIN;
					}

					if (tab < TABS_MIN) tab = TABS_MIN;
					if (tab > TABS_MAX) tab = TABS_MAX;
					this.activeTab = tab;
				}, 0);
			});
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => {
				setTimeout(() => {
					this.user = data["resolved"];
				}, 0);
			});
		this.loadUserImages();
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	getUserImage(): string {
		if (!this.user) return config.users.defaultImage;
		return this.user.photoUrl || config.users.defaultImage;
	}

	loadUserImages() {
		if (!this.user) {
			this.images = [];
			return;
		}

		this._userClient.photos(this.user.id, this.imagesPagination)
			.pipe(catchError(error => {
				this._alertService.toasts.error(error.toString());
				return of({
					result: [],
					pagination: {
						...this.imagesPagination,
						page: 1,
						count: 0
					}
				});
			}))
			.subscribe((res: IPaginated<IPhoto>) => {
				this.imagesPagination = res.pagination;
				this.images = (res.result || []).map(e => {
					const img: NgxGalleryImage = {
						small: e.url,
						medium: e.url,
						big: e.url,
						description: e.description
					};
					return img;
				});
			});
	}

	imagesPageChanged(page: number): void {
		this.imagesPagination.page = page;
		this.loadUserImages();
	}

	range(start: number, count: number, step = 1): Range {
		return new Range(start, count, step);
	};
}
