import { Component, OnInit } from "@angular/core";

import { IMessage, MessageType } from "@common/Message";

import WeatherClient from "@services/web/WeatherClient";
import { IForecast } from "@data/model/Forecast";
import { IPaginated } from "@common/pagination/Paginated";

@Component({
    selector: "app-weather",
    templateUrl: "./weather.component.html",
    styleUrls: ["./weather.component.scss"]
})
export class WeatherComponent implements OnInit {
    protected paginated: IPaginated<IForecast> = { pagination: { page: 1, pageSize: 7, count: 0 } };
    protected messages: IMessage[];

    constructor(private readonly _weatherClient: WeatherClient) {
    }

    ngOnInit(): void {
        this.list();
    }

    list() {
		this.clearMessages();
        this._weatherClient.list(this.paginated.pagination)
            .subscribe((paginated: IPaginated<IForecast>) => this.paginated = paginated
                , (error: any) => {
					this.messages.push({
						type: MessageType.Error,
						content: <string>error.message
					});
				});
    }

    clearMessages() {
        if (!this.messages) {
            this.messages = [];
        } else {
            this.messages.length = 0;
        }
    }
}
