"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
let AppComponent = class AppComponent {
    constructor(paginationConfig) {
        this.title = "Dating App";
        paginationConfig.boundaryLinks = true;
        paginationConfig.rotate = true;
        paginationConfig.ellipses = true;
        paginationConfig.directionLinks = true;
        paginationConfig.pageSize = 10;
        paginationConfig.maxSize = 5;
    }
};
AppComponent = tslib_1.__decorate([
    core_1.Component({
        selector: "app-root",
        templateUrl: "./app.component.html",
        styleUrls: ["./app.component.css"]
    })
], AppComponent);
exports.AppComponent = AppComponent;
//# sourceMappingURL=app.component.js.map