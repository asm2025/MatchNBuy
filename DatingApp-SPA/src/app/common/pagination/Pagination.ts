import * as _ from "lodash";

export interface IPagination {
	page?: number;
	pageSize?: number;
	count?: number;
}

export class Pagination implements IPagination {
	private _page = 1;
	private _pageSize = 10;
	private _count = 0;

	get page(): number {
		return this._page;
	}

	set page(value: number) {
		this._page = _.inRange(value, 1, Number.MAX_SAFE_INTEGER)
			? value
			: 1;
	}

	get pageSize(): number {
		return this._pageSize;
	}

	set pageSize(value: number) {
		this._pageSize = _.inRange(value, 1, Number.MAX_SAFE_INTEGER)
			? value
			: 10;
	}

	get count(): number {
		return this._count;
	}

	set count(value: number) {
		this._count = _.inRange(value, 0, Number.MAX_VALUE)
			? value
			: 0;
	}
}
