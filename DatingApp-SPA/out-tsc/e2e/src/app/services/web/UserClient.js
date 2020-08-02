"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const angular_jwt_1 = require("@auth0/angular-jwt");
const rxjs_1 = require("rxjs");
const operators_1 = require("rxjs/operators");
const ApiClient_1 = require("@common/web/ApiClient");
const config_json_1 = require("@/config.json");
let UserClient = class UserClient extends ApiClient_1.default {
    constructor(client) {
        super(config_json_1.default.datingApp.url, client);
        this.jwt = new angular_jwt_1.JwtHelperService();
        this.photo = new rxjs_1.BehaviorSubject("@images/user.png");
        this.photoUrl = this.photo.asObservable();
    }
    list(pagination) {
        return this.client.post(`${this.baseUrl}/user/`, pagination);
    }
    login(userName, password) {
        return this.client.post(`${this.baseUrl}/users/login`, { userName, password })
            .pipe(operators_1.map((res) => {
            console.log(res);
            return true;
        }));
    }
};
UserClient = tslib_1.__decorate([
    core_1.Injectable({
        providedIn: "root"
    })
], UserClient);
exports.default = UserClient;
//# sourceMappingURL=UserClient.js.map