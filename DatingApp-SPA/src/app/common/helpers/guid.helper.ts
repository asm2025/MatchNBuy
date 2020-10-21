import StringHelper from "./string.helper";

export default class GuidHelper {
	static toNumber(value: string | null | undefined): number {
		if (!value) return 0;
		value = this.toDigits(value);
		if (value.length % 2 !== 0) value = "0" + value;

		const bn = BigInt(`0x${value}`);
		const d = bn.toString(10);
		return Number(d);
	}

	static strip(value: string | null | undefined) {
		return StringHelper.trim(value, "{}");
	}

	static toDigits(value: string | null | undefined) {
		return this.strip(value).replace(/-/g, "");
	}
}
