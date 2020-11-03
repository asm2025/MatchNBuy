import { Component, OnInit, AfterViewInit, OnDestroy, Input, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";
import { FileUploader } from "ng2-file-upload";

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
	private readonly _tabSubject = new BehaviorSubject<number>(TABS_MIN);

	disposed$ = new ReplaySubject<boolean>();
	user: IUserForDetails | null | undefined;
	activeTab = this._tabSubject.asObservable();
	selectedImageId = "";
	images: IPhoto[] = [];
	imagesPagination: ISortablePagination = {
		page: 1,
		pageSize: 4,
		orderBy: [{
			name: "isDefault"
		},
		{
			name: "dateAdded",
			type: SortType.Descending
		}]
	};
	uploader: FileUploader;
	hasDropFile = false;
	uploadProgress = 0.0;

	@Input() id: string;

	@ViewChild("galleryCarousel") galleryCarousel!: NgbCarousel;

	constructor(private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService,
		carouselConfig: NgbCarouselConfig) {

		carouselConfig.showNavigationArrows = false;
		carouselConfig.showNavigationIndicators = false;

		this.uploader = new FileUploader({
			allowedFileType: ["image"],
			maxFileSize: 10 * 1024 * 1024, // 10MB
			authToken: this._userClient.token || "",
			queueLimit: 1,
			disableMultipart: true, // must be true for formatDataFunction to be called.
			formatDataFunctionIsAsync: true,
			formatDataFunction: async (item: any) => {
				console.log("formatDataFunction", item);
				return new Promise((resolve) => {
					resolve({
						name: item._file.name,
						length: item._file.size,
						contentType: item._file.type,
						date: new Date()
					});
				});
			}
		});

		this.uploader.onProgressItem = (item, progress) => this.uploadProgress = progress;
		this.uploader.onCompleteItem = (item, response, status, headers) => this.uploadProgress = 0.0;
	}

	ngOnInit() {
		this._tabSubject
			.pipe(takeUntil(this.disposed$))
			.subscribe(tab => {
				if (!this.user) return;

				switch (tab) {
					case 1:
						// about user
						break;
					case 2:
						// photos
						if (this.imagesPagination.page !== 1) {
							this.imagesPagination = {
								...this.imagesPagination,
								page: 1
							};
						} else {
							this.loadUserImages();
						}
						break;
					case 3:
						// messages
						break;
				}
			});
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

					this.selectTab(tab);
				}, 0);
			});
		this._route
			.data
			.pipe(takeUntil(this.disposed$))
			.subscribe(data => {
				setTimeout(() => {
					this.user = data["resolved"];

					if (!this.user)
						this.uploader.options.url = "";
					else
						this.uploader.options.url = `${config.backend.url}/${this.user.id}/Photos/Add`;

					this.selectTab(TABS_MIN);
				}, 0);
			});
	}

	ngOnDestroy(): void {
		this.disposed$.next(true);
		this.disposed$.complete();
	}

	get isCurrentUser(): boolean {
		if (!this.user) return false;
		const currentUser = this._userClient.user;
		if (!currentUser) return false;
		return currentUser.id === this.user.id;
	}

	get userImage(): string {
		if (!this.user) return config.users.defaultImage;
		return this.user.photoUrl || config.users.defaultImage;
	}

	get isUploading(): boolean {
		return this.uploadProgress > 0.0;
	}

	imagesPageChanged(page: number): void {
		this.imagesPagination.page = page;
		this.loadUserImages();
	}

	imageDefaultClick(id: string) {
		console.log("imageDefaultClick", id);
	}

	imageEditClick(id: string) {
		console.log("imageEditClick", id);
	}

	imageDeleteClick(id: string) {
		console.log("imageDeleteClick", id);
	}

	imageUploaderHover(evt: boolean) {
		this.hasDropFile = evt;
	}

	messagesPageChanged(page: number): void {
		this.imagesPagination.page = page;
		this.loadUserMessages();
	}

	selectTab(tab: number) {
		if (tab < TABS_MIN) tab = TABS_MIN;
		if (tab > TABS_MAX) tab = TABS_MAX;
		this._tabSubject.next(tab);
	}

	loadUserImages() {
		if (!this.user) return;
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
				this.images = res.result || [];

				if (this.images.length > 0) {
					const id = (this.images.find(e => e.isDefault) || this.images[0]).id;
					this.selectImage(id);
				} else {
					this.selectImage(null);
				}
			});
	}

	selectImage(id: string | null | undefined) {
		setTimeout(() => {
			this.selectedImageId = id || "";

			if (!this.galleryCarousel.slides) {
				this.galleryCarousel.activeId = this.selectedImageId;
				return;
			}

			this.galleryCarousel.select(this.selectedImageId, NgbSlideEventSource.INDICATOR);
		}, 0);
	}

	loadUserMessages() {
		alert("Load user messages...");
	}

	range(start: number, count: number, step = 1): Range {
		return new Range(start, count, step);
	};
}
