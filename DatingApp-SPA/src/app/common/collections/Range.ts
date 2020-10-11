export default class Range {
	constructor(public readonly start: number,
		public readonly count: number,
		public readonly step: number = 1) {
	}

	*[Symbol.iterator]() {
		const start = this.start,
			end = start + this.count;

		for (let i = start; i < end; i += this.step) {
			yield i;
		}
	}
}
