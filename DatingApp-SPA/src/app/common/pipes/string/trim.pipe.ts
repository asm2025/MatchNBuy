import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
	name: "trim"
})

export default class TrimPipe implements PipeTransform {
	transform(value: string | null | undefined): string {
		if (!value) return "";
		return value.trim();
	}
}
