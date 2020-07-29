export interface IKeyedCollection<T = any> extends Array<T> {
	indexOfKey(key: string): number;
	containsKey(key: string): boolean;
	item(key: string): T | undefined;
	remove(key: string): T | undefined;
}

export default abstract class KeyedCollection<T = any> extends Array<T> implements IKeyedCollection<T> {
	private _keys: Array<string> = [];

	indexOfKey(key: string): number {
		return this._keys.indexOf(key);
	}

	containsKey(key: string): boolean {
		return this.indexOfKey(key) > -1;
	}

	item(key: string): T | undefined {
		const index = this.indexOfKey(key);
		return index < 0 ? undefined : this[index];
	}

	push(...value: T[]): number {
		let n = 0;
		value.forEach(v => {
			const key = this.getKeyForItem(v);
			if (!key) throw new Error("Key cannot be null or empty.");
			if (this.containsKey(key)) throw new Error("Duplicate key found.");
			this._keys.push(key);
			super.push(v);
			n++;
		});

		return n;
	}

	remove(key: string): T | undefined {
		const index = this.indexOfKey(key);
		if (index < 0) return undefined;

		const value = this[index];
		delete this[index];
		delete this._keys[index];
		return value;
	}

	abstract getKeyForItem(value: T): string;
}
