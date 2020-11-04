import { Component, OnInit, AfterViewInit, OnDestroy, Input, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";
import { FileUploader, FileItem } from "ng2-file-upload";

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
	private readonly _uploader: FileUploader;

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

		this._uploader = new FileUploader({
			url: `${config.backend.url}/${this.user.id}/Photos/Add`,
			//allowedFileType: ["image"],
			maxFileSize: 10 * 1024 * 1024, // 10MB
			authToken: `Bearer ${this._userClient.token}`,
			queueLimit: 1,
			disableMultipart: true, // must be true for formatDataFunction to be called.
			formatDataFunctionIsAsync: true,
			formatDataFunction: async (item: any) => {
				return new Promise((resolve) => {
					resolve({
						name: item.file.name,
						length: item.file.size,
						contentType: item.file.type,
						date: new Date()
					});
				});
			}
		});

		this._uploader.onBeforeUploadItem = this.imageUploaderBeforeUploadItem;
		this._uploader.onProgressItem = this.imageUploaderProgressItem;
		this._uploader.onSuccessItem = this.imageUploaderSuccessItem;
		this._uploader.onErrorItem = this.imageUploaderErrorItem;
		this._uploader.onCancelItem = this.imageUploaderCancelItem;
		this._uploader.onCompleteItem = this.imageUploaderCompleteItem;
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
		this._uploader.response
			.pipe(takeUntil(this.disposed$))
			.subscribe(res => this._alertService.toasts.success(res));
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
					this.selectTab(this._tabSubject.value);
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
		alert(`imageDefaultClick: ${id}`);
	}

	imageEditClick(id: string) {
		alert(`imageEditClick: ${id}`);
	}

	imageDeleteClick(id: string) {
		alert(`imageDeleteClick: ${id}`);
	}

	imageUploaderHover(hover: boolean) {
		this.hasDropFile = hover;
	}

	imageUploaderDropFiles(files: FileList) {
		if (files.length === 0) return;
		this._uploader.uploadAll();
	}

	imageUploaderBeforeUploadItem(item: FileItem) {
		this._alertService.toasts.info(`Uploading file '${item.file.name}', size: ${item.file.size}...`);
	}

	imageUploaderProgressItem(item: FileItem, progress: any) {
		console.log("Progress", item.file.name, progress);
		this.uploadProgress = progress;
	}

	imageUploaderSuccessItem(item: FileItem) {
		console.log("Success", item.file.name);
		//this._alertService.toasts.success(`File '${item.file.name}' was uploaded successfully.`);
	}

	imageUploaderErrorItem(item: FileItem, response: string, status: number) {
		console.log("Error", item.file.name, response, status);
		//this._alertService.toasts.error(`File '${item.file.name}' failed to be uploaded. Status: ${status}. Response: ${response}`);
	}

	imageUploaderCancelItem(item: FileItem) {
		console.log("Cancelled", item.file.name);
		//this._alertService.toasts.warning(`Uploading file '${item.file.name}' was cancelled.`);
	}

	imageUploaderCompleteItem() {
		console.log("Completed");
		this.uploadProgress = 0.0;
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
