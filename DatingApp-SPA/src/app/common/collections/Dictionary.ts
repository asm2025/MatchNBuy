export interface IDictionary<T = any> {
	[key: string]: T;
}

export default class Dictionary<T = any> implements IDictionary<T> {
	[key: string]: T;
}
