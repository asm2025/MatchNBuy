import { Injectable } from "@angular/core";
import {
	HammerGestureConfig,
	HAMMER_GESTURE_CONFIG
} from "@angular/platform-browser";

@Injectable({
	providedIn: "root"
})
export class HammerConfig extends HammerGestureConfig {
	overrides = {
		pinch: { enable: false },
		rotate: { enable: false }
	};
}

const HammerConfigProvider = {
	provide: HAMMER_GESTURE_CONFIG,
	useClass: HammerConfig,
	multi: true
};

export default HammerConfigProvider;
