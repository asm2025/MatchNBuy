import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { IAlert, AlertType } from "@common/Alert";
import UserClient from "@services/web/UserClient";
import { IUser } from "@data/model/User";
import { IPaginated } from "@common/pagination/Paginated";

@Component({
    selector: "app-nav",
    templateUrl: "./nav.component.html",
    styleUrls: ["./nav.component.scss"]
})
export class NavComponent implements OnInit {

    constructor() { }

    ngOnInit() {
    }

}
