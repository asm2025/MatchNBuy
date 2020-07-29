// This will allow you to load `.json` files from disk
// import * as graph from './data/graph.json';
declare module "*.json" {
	const value: any;
	export default value;
}

// This will allow you to load JSON from remote URL responses
// import data from "json!http://foo.com/data_returns_json_response/";
declare module "json!*" {
	const value: any;
	export default value;
}
