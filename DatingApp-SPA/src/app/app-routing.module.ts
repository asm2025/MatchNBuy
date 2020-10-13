import { RouterModule, Routes } from "@angular/router";

import AuthGuard from "@/_guards/auth.guard";

import HomeComponent from "@components/pages/home/home.component";

import SignInComponent from "@components/pages/sign-in/sign-in.component";
import SignUpComponent from "@components/pages/sign-up/sign-up.component";

import MemberListComponent from "@components/pages/members/member-list/member-list.component";
import MemberListResolver from "@components/pages/members/member-list/member-list.resolver";
import MemberDetailComponent from "@components/pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@components/pages/members/member-detail/member-detail.resolver";
import MemberEditComponent from "@components/pages/members/member-edit/member-edit.component";
import MemberEditResolver from "@components/pages/members/member-edit/member-edit.resolver";
import MemberEditUnsavedChanges from "@components/pages/members/member-edit/member-edit-unsaved-changes.guard";

import MessagesComponent from "@components/pages/messages/messages.component";
import MessagesResolver from "@components/pages/messages/messages.resolver";
import MessageThreadsComponent from "@components/pages/messages/message-threads/message-threads.component";
import MessageThreadsResolver from "@components/pages/messages/message-threads/message-threads.resolver";
import ThreadMessagesComponent from "@components/pages/messages/thread-messages/thread-messages.component";
import ThreadMessagesResolver from "@components/pages/messages/thread-messages/thread-messages.resolver";

import WeatherComponent from "@components/pages/weather/weather.component";
import WeatherResolver from "@components/pages/weather/weather.resolver";

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
				path: "members",
				children: [{
						path: "",
						pathMatch: "full",
						component: MemberListComponent,
						resolve: { resolved: MemberListResolver }
					},
					{
						path: ":id",
						pathMatch: "full",
						component: MemberDetailComponent,
						resolve: { resolved: MemberDetailResolver }
					},
					{
						path: ":id/edit",
						component: MemberEditComponent,
						resolve: { resolved: MemberEditResolver },
						canDeactivate: [MemberEditUnsavedChanges]
					}]
			},
			{
				path: "messages",
				children: [{
						path: "",
						pathMatch: "full",
						component: MessagesComponent,
						resolve: { resolved: MessagesResolver }
					},
					{
						path: "threads",
						pathMatch: "full",
						component: MessageThreadsComponent,
						resolve: { resolved: MessageThreadsResolver }
					},
					{
						path: "threads/:id",
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
