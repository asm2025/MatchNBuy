"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.WeatherComponent = void 0;
const tslib_1 = require("tslib");
const core_1 = require("@angular/core");
const Message_1 = require("@common/Message");
let WeatherComponent = class WeatherComponent {
    constructor(weatherClient) {
        this.weatherClient = weatherClient;
        this.paginated = { pagination: { page: 1, pageSize: 7, count: 0 } };
    }
    ngOnInit() {
        this.list();
    }
    list() {
        this.clearMessages();
        this.weatherClient.list(this.paginated.pagination)
            .subscribe((paginated) => this.paginated = paginated, (error) => {
            this.messages.push({
                type: Message_1.MessageType.Error,
                content: error.message
            });
        });
    }
    clearMessages() {
        if (!this.messages) {
            this.messages = [];
        }
        else {
            this.messages.length = 0;
        }
    }
};
WeatherComponent = tslib_1.__decorate([
    core_1.Component({
        selector: "app-weather",
        templateUrl: "./weather.component.html",
        styleUrls: ["./weather.component.css"]
    })
], WeatherComponent);
exports.WeatherComponent = WeatherComponent;
//# sourceMappingURL=weather.component.js.map