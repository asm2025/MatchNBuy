"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class KeyedCollection extends Array {
    constructor() {
        super(...arguments);
        this._keys = [];
    }
    indexOfKey(key) {
        return this._keys.indexOf(key);
    }
    containsKey(key) {
        return this.indexOfKey(key) > -1;
    }
    item(key) {
        const index = this.indexOfKey(key);
        return index < 0 ? undefined : this[index];
    }
    push(...value) {
        let n = 0;
        value.forEach(v => {
            const key = this.getKeyForItem(v);
            if (!key)
                throw new Error("Key cannot be null or empty.");
            if (this.containsKey(key))
                throw new Error("Duplicate key found.");
            this._keys.push(key);
            super.push(v);
            n++;
        });
        return n;
    }
    remove(key) {
        const index = this.indexOfKey(key);
        if (index < 0)
            return undefined;
        const value = this[index];
        delete this[index];
        delete this._keys[index];
        return value;
    }
}
exports.default = KeyedCollection;
//# sourceMappingURL=KeyedCollection.js.map