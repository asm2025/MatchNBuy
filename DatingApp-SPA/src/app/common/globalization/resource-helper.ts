import Cookies from "js-cookie";
import * as _ from "lodash";

import config from "@app/config.json";

const cookieName = (config.language && config.language.cookieName) ? <string>config.language.cookieName : "culture";

export default class ResourceHelper {
	private static __resources = {
		en: {
		}
	};

	private static _language: string | null | undefined;

	static get language(): string {
		if (!this._language) {
			this._language = Cookies.get(cookieName) || "en";
			const n = this._language.indexOf("-");
			if (n > -1) this._language = this._language.substr(0, n);

			const cults = Object.keys(this.__resources);
			if (!_.includes(cults, this._language)) this._language = cults[0];
		}

		return this._language;
	}

	static set language(value: string) {
		this._language = value || "en";
		Cookies.set(cookieName, this._language);
	}

	static getString(name: string, defaultValue = ""): string {
		if (!name) return defaultValue;
		const lang = this.language;
		name = _.camelCase(name);
		return (_.property(`${lang}.${name}`)(this.__resources) || defaultValue) as string;
	}
}
