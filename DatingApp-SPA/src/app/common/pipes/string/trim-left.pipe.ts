import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
	name: "trimLeft"
})

export default class TrimLeftPipe implements PipeTransform {
	transform(value: string | null | undefined): string {
		if (!value) return "";
		return value.trimLeft();
	}
}
