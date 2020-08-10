"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Pagination = void 0;
const _ = require("lodash");
class Pagination {
    constructor() {
        this._page = 1;
        this._pageSize = 10;
        this._count = 0;
    }
    get page() {
        return this._page;
    }
    set page(value) {
        this._page = _.inRange(value, 1, Number.MAX_SAFE_INTEGER)
            ? value
            : 1;
    }
    get pageSize() {
        return this._pageSize;
    }
    set pageSize(value) {
        this._pageSize = _.inRange(value, 1, Number.MAX_SAFE_INTEGER)
            ? value
            : 10;
    }
    get count() {
        return this._count;
    }
    set count(value) {
        this._count = _.inRange(value, 0, Number.MAX_VALUE)
            ? value
            : 0;
    }
}
exports.Pagination = Pagination;
//# sourceMappingURL=Pagination.js.map