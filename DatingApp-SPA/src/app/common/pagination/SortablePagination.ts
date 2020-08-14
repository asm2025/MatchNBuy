import { ISortField } from "@common/sorting/SortField";
import { IPagination } from "./Pagination";

export interface ISortablePagination extends IPagination {
	orderBy?: Array<ISortField>;
}
