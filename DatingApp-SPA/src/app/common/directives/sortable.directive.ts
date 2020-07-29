﻿import { Directive, EventEmitter, Input, Output } from "@angular/core";

export type SortDirection = "asc" | "desc" | "";
const rotate: { [key: string]: SortDirection; } = { "asc": "desc", "desc": "", "": "asc" };

export interface ISortEvent {
	column: string;
	direction: SortDirection;
}

@Directive({
	selector: "th[sortable]",
	host: {
		'[class.asc]': 'direction === "asc"',
		'[class.desc]': 'direction === "desc"',
		'(click)': "rotate()"
	}
})
export class SortableHeader {
	@Input() sortable: string;
	@Input() direction: SortDirection = "";
	@Output() sort = new EventEmitter<ISortEvent>();

	rotate() {
		this.direction = rotate[this.direction];
		this.sort.emit({ column: this.sortable, direction: this.direction });
	}
}
