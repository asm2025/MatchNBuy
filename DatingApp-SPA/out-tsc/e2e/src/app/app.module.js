"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const platform_browser_1 = require("@angular/platform-browser");
const animations_1 = require("@angular/platform-browser/animations");
const http_1 = require("@angular/common/http");
const ng_bootstrap_1 = require("@ng-bootstrap/ng-bootstrap");
const ng_bootstrap_2 = require("@ng-bootstrap/ng-bootstrap");
const app_component_1 = require("@/app.component");
const message_component_1 = require("@components/message/message.component");
const nav_component_1 = require("@components/nav/nav.component");
const weather_component_1 = require("@components/weather/weather.component");
const forecast_component_1 = require("@components/forecast/forecast.component");
const WeatherClient_1 = require("@services/web/WeatherClient");
const UserClient_1 = require("@services/web/UserClient");
let AppModule = class AppModule {
};
AppModule = tslib_1.__decorate([
    core_1.NgModule({
        imports: [
            platform_browser_1.BrowserModule,
            animations_1.BrowserAnimationsModule,
            http_1.HttpClientModule,
            ng_bootstrap_1.NgbModule
        ],
        declarations: [
            app_component_1.AppComponent,
            message_component_1.MessageComponent,
            weather_component_1.WeatherComponent,
            forecast_component_1.ForecastComponent,
            nav_component_1.NavComponent
        ],
        providers: [
            ng_bootstrap_2.NgbPaginationConfig,
            WeatherClient_1.default,
            UserClient_1.default
        ],
        bootstrap: [
            app_component_1.AppComponent
        ]
    })
], AppModule);
exports.AppModule = AppModule;
//# sourceMappingURL=app.module.js.map