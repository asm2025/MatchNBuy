"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
let ApiClient = class ApiClient {
    constructor(baseUrl, client) {
        this.baseUrl = baseUrl;
        this.client = client;
    }
};
ApiClient = tslib_1.__decorate([
    core_1.Injectable()
], ApiClient);
exports.default = ApiClient;
//# sourceMappingURL=ApiClient.js.map