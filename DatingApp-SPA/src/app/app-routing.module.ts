import { RouterModule, Routes } from "@angular/router";

import AuthGuard from "@/_guards/auth.guard";

import HomeComponent from "@pages/home/home.component";

import SignInComponent from "@pages/sign-in/sign-in.component";
import SignUpComponent from "@pages/sign-up/sign-up.component";

import ListsComponent from "@pages/lists/lists.component";
import ListsResolver from "@pages/lists/lists.resolver";
import MemberListComponent from "@pages/members/member-list/member-list.component";
import MemberDetailComponent from "@pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@pages/members/member-detail/member-detail.resolver";
import MemberEditComponent from "@pages/members/member-edit/member-edit.component";
import MemberEditResolver from "@pages/members/member-edit/member-edit.resolver";
import MemberEditUnsavedChanges from "@pages/members/member-edit/member-edit-unsaved-changes.guard";

import MessagesComponent from "@pages/messages/messages.component";
import MessagesResolver from "@pages/messages/messages.resolver";
import MessageThreadsComponent from "@pages/messages/message-threads/message-threads.component";
import MessageThreadsResolver from "@pages/messages/message-threads/message-threads.resolver";
import ThreadMessagesComponent from "@pages/messages/thread-messages/thread-messages.component";
import ThreadMessagesResolver from "@pages/messages/thread-messages/thread-messages.resolver";

import WeatherComponent from "@pages/weather/weather.component";
import WeatherResolver from "@pages/weather/weather.resolver";

export const routes: Routes = [
	{
		path: "",
		component: HomeComponent,
		pathMatch: "full"
	},
	{
		path: "login",
		component: SignInComponent
	},
	{
		path: "register",
		component: SignUpComponent
	},
	{
		path: "weather",
		component: WeatherComponent,
		runGuardsAndResolvers: "always",
		resolve: { resolved: WeatherResolver }
	},
	{
		path: "",
		runGuardsAndResolvers: "always",
		canActivate: [AuthGuard],
		children: [{
				path: "lists",
				component: ListsComponent,
				resolve: { resolved: ListsResolver }
			},
			{
				path: "members",
				component: MemberListComponent,
				resolve: { resolved: ListsResolver },
				children: [{
						path: ":id",
						component: MemberDetailComponent,
						resolve: { resolved: MemberDetailResolver }
					},
					{
						path: "edit",
						component: MemberEditComponent,
						resolve: { resolved: MemberEditResolver },
						canDeactivate: [MemberEditUnsavedChanges]
					}]
			},
			{
				path: "messages",
				component: MessagesComponent,
				resolve: { resolved: MessagesResolver },
				children: [{
						path: "threads",
						component: MessageThreadsComponent,
						resolve: { resolved: MessageThreadsResolver }
					},
					{
						path: "thread",
						component: ThreadMessagesComponent,
						resolve: { resolved: ThreadMessagesResolver }
					}]
			}
		]
	},
	{
		path: "**",
		redirectTo: "",
		pathMatch: "full"
	}
];

const AppRoutingModule = RouterModule.forRoot(routes, { useHash: false });
export default AppRoutingModule;
