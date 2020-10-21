export default class StringHelper {
	static trim(value: string | null | undefined, find: string): string {
		if (!value) return "";
		if (!find) return value;

		const reg = this.escape(find);
		return value.replace(RegExp(`^[${reg}]+|[${reg}]+$`, "g"), "");
	}

	static escape(value: string | null | undefined) {
		return !value ? "" : value.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&");
	}
}
