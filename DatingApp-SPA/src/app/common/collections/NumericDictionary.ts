export interface INumericDictionary<T = any> {
	[key: number]: T;
}

export default class NumericDictionary<T = any> implements INumericDictionary<T> {
	[key: number]: T;
}
