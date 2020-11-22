import { Component, OnInit, AfterViewInit, OnDestroy, Input, ViewChild } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { BehaviorSubject, ReplaySubject, of } from "rxjs";
import { takeUntil, catchError } from "rxjs/operators";
import {
	fadeInOnEnterAnimation,
	fadeOutOnLeaveAnimation
} from "angular-animations";
import { NgbCarouselConfig, NgbCarousel, NgbSlideEventSource } from "@ng-bootstrap/ng-bootstrap";
import { FileUploader, FileItem } from "ng2-file-upload";

import Range from "@common/collections/Range";
import { IUserForDetails } from "@data/model/User";
import { IPaginated } from "@common/pagination/Paginated";
import { ISortablePagination } from "@common/pagination/SortablePagination";
import { SortType } from "@common/sorting/SortType";
import { IPhoto, IPhotoToEdit } from "@data/model/Photo";
import UserClient from "@services/web/UserClient";
import AlertService from "@services/alert.service";

import config from "@/config.json";

const TABS_MIN = 1;
const TABS_MAX = 3;

@Component({
	selector: "app-member-detail",
	templateUrl: "./member-detail.component.html",
	styleUrls: ["./member-detail.component.scss"],
	animations: [
		fadeInOnEnterAnimation(),
		fadeOutOnLeaveAnimation()
	]
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

	imageProps: IPhotoToEdit = {
		description: "",
		isDefault: false
	};

	hasDropFile = false;
	uploadProgress = 0.0;

	@Input() id: string;

	@ViewChild("galleryCarousel") galleryCarousel!: NgbCarousel;

	constructor(private readonly _router: Router,
		private readonly _route: ActivatedRoute,
		private readonly _userClient: UserClient,
		private readonly _alertService: AlertService,
		carouselConfig: NgbCarouselConfig) {

		carouselConfig.showNavigationArrows = false;
		carouselConfig.showNavigationIndicators = false;

		const authUser = this._userClient.user;
		if (!authUser) throw new Error("Unauthorized");
		this._uploader = new FileUploader({
			url: `${config.backend.url}/Users/${authUser.id}/Photos/Add`,
			//allowedFileType: ["image"],
			maxFileSize: 10 * 1024 * 1024, // 10MB
			authToken: `Bearer ${this._userClient.token}`,
			queueLimit: 1,
			removeAfterUpload: true
		});

		this._uploader.onBeforeUploadItem = this.imageUploaderBeforeUploadItem.bind(this);
		this._uploader.onBuildItemForm = this.imageUploaderBuildItemForm.bind(this);
		this._uploader.onSuccessItem = this.imageUploaderSuccessItem.bind(this);
		this._uploader.onErrorItem = this.imageUploaderErrorItem.bind(this);
		this._uploader.onCancelItem = this.imageUploaderCancelItem.bind(this);
		this._uploader.onCompleteAll = this.imageUploaderComplete.bind(this);
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

	ngAfterViewInit(): void {
		this._route
			.params
			.pipe(takeUntil(this.disposed$))
			.subscribe(params => {
				setTimeout(() => {
					if (!this.id && params["id"]) {
						this.id = params["id"];
						this.loadUserData();
						return;
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

	get uploadedFile(): File | null {
		return this._uploader.queue.length > 0
			? this._uploader.queue[0]._file
			: null;
	}

	get hasUploadFile(): boolean {
		return this._uploader.queue.length > 0;
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
		if (files.length === 0 || files[0].size > 128) return;
		this._uploader.clearQueue();
	}

	imageUploaderBeforeUploadItem(item: FileItem) {
		this._alertService.alerts.info(`Uploading file '${item.file.name}', size: ${item.file.size} byte(s)...`, true, this._alertService.defaultDelay);
	}

	imageUploaderBuildItemForm(item: FileItem, form: FormData) {
		for (const key of Object.keys(this.imageProps)) {
			const value = this.imageProps[key];
			if (!value) continue;
			form.append(key, value.toString());
		}
	}

	imageUploaderSuccessItem(item: FileItem) {
		this._alertService.alerts.success(`File '${item.file.name}' was uploaded successfully.`, true, this._alertService.defaultDelay);
	}

	imageUploaderErrorItem(item: FileItem, response: string, status: number) {
		this._alertService.alerts.error(`File '${item.file.name}' failed to be uploaded. Status: ${status}. Response: ${response}`);
		this._uploader.clearQueue();
	}

	imageUploaderCancelItem(item: FileItem) {
		this._alertService.alerts.warning(`Uploading file '${item.file.name}' was cancelled.`, true, this._alertService.defaultDelay);
	}

	imageUploaderComplete() {
		this.imageProps = {
			description: "",
			isDefault: false
		};
		this._uploader.clearQueue();
		this.loadUserImages();
	}

	imageUploaderUploadClick() {
		this._uploader.uploadAll();
	}

	imageUploaderCancelClick() {
		this._uploader.cancelAll();
		this._uploader.clearQueue();
	}

	messagesPageChanged(page: number): void {
		this.imagesPagination.page = page;
		this.loadUserMessages();
	}

	loadUserData() {
		if (!this.id) {
			setTimeout(() => this.user = null, 0);
			return;
		}
		this._userClient
			.get(this.id)
			.pipe(catchError(error => {
				this._alertService.alerts.error(error.toString());
				this._router.navigate(["/"]);
				return of(null);
			}))
			.subscribe((res: IUserForDetails | null | undefined) => {
				this.user = res;
				this.selectTab(this._tabSubject.value);
			});
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
