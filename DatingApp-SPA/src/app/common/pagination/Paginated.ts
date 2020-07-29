import { IPagination } from "./Pagination";

export interface IPaginated<T> {
	result?: T[];
	pagination: IPagination;
}

export class Paginated<T> implements IPaginated<T> {
    result?: T[];
	pagination: IPagination;
}
