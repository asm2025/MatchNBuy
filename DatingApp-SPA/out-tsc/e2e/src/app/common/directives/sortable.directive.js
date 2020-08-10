"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SortableHeader = void 0;
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const rotate = { "asc": "desc", "desc": "", "": "asc" };
let SortableHeader = class SortableHeader {
    constructor() {
        this.direction = "";
        this.sort = new core_1.EventEmitter();
    }
    rotate() {
        this.direction = rotate[this.direction];
        this.sort.emit({ column: this.sortable, direction: this.direction });
    }
};
tslib_1.__decorate([
    core_1.Input()
], SortableHeader.prototype, "sortable", void 0);
tslib_1.__decorate([
    core_1.Input()
], SortableHeader.prototype, "direction", void 0);
tslib_1.__decorate([
    core_1.Output()
], SortableHeader.prototype, "sort", void 0);
SortableHeader = tslib_1.__decorate([
    core_1.Directive({
        selector: "th[sortable]",
        host: {
            '[class.asc]': 'direction === "asc"',
            '[class.desc]': 'direction === "desc"',
            '(click)': "rotate()"
        }
    })
], SortableHeader);
exports.SortableHeader = SortableHeader;
//# sourceMappingURL=sortable.directive.js.map