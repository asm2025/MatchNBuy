"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const moment_1 = require("moment");
const ApiClient_1 = require("@common/web/ApiClient");
const config_json_1 = require("@/config.json");
let WeatherClient = class WeatherClient extends ApiClient_1.default {
    constructor(client) {
        super(config_json_1.default.datingApp.url, client);
    }
    list(pagination) {
        return this.client.post(`${this.baseUrl}/weather/`, pagination);
    }
    get(date) {
        const d = encodeURIComponent(moment_1.default(date).format("yyyy-MM-dd"));
        return this.client.get(`${this.baseUrl}/weather/date=${d}`);
    }
};
WeatherClient = tslib_1.__decorate([
    core_1.Injectable()
], WeatherClient);
exports.default = WeatherClient;
//# sourceMappingURL=WeatherClient.js.map