import { RouterModule, Routes } from "@angular/router";

import AuthGuard from "@app/guards/auth.guard";
import UnsavedChangesGuard from "@common/guards/unsaved-changes.guard";

import HomeComponent from "@components/pages/home/home.component";
import AboutComponent from "@components/pages/about/about.component";
import ContactComponent from "@components/pages/contact/contact.component";

import MemberListComponent from "@components/pages/members/member-list/member-list.component";
import MemberDetailComponent from "@components/pages/members/member-detail/member-detail.component";
import MemberDetailResolver from "@components/pages/members/member-detail/member-detail.resolver";
import MemberEditComponent from "@components/pages/members/member-detail/member-edit/member-edit.component";
import MemberEditResolver from "@components/pages/members/member-detail/member-edit/member-edit.resolver";
import MemberGalleryComponent from "@components/pages/members/member-detail/member-gallery/member-gallery.component";
import MemberMessagesComponent from "@components/pages/members/member-detail/member-messages/member-messages.component";
import MemberPhotoEditorComponent from "@components/pages/members/member-detail/member-photo-editor/member-photo-editor.component";
import MemberPhotoEditorResolver from "@components/pages/members/member-detail/member-photo-editor/member-photo-editor.resolver";

import MessagesComponent from "@components/pages/messages/messages.component";
import ThreadsComponent from "@components/pages/messages/threads/threads.component";
import ThreadMessagesComponent from "@components/pages/messages/thread-messages/thread-messages.component";

import SignInComponent from "@components/pages/sign-in/sign-in.component";
import SignUpComponent from "@components/pages/sign-up/sign-up.component";

import WeatherComponent from "@components/pages/weather/weather.component";

export const routes: Routes = [{
	path: "",
	component: HomeComponent,
	pathMatch: "full"
},
{
	path: "about",
	component: AboutComponent
},
{
	path: "contact",
	component: ContactComponent
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
	component: WeatherComponent
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
			component: MemberListComponent
		},
		{
			path: ":id",
			children: [{
				path: "",
				pathMatch: "full",
				component: MemberDetailComponent,
				resolve: { resolved: MemberDetailResolver }
			},
			{
				path: "edit",
				component: MemberEditComponent,
				resolve: { resolved: MemberEditResolver },
				canDeactivate: [UnsavedChangesGuard]
			},
			{
				path: "messages",
				component: MemberMessagesComponent
			},
			{
				path: "gallery",
				children: [{
					path: "",
					pathMatch: "full",
					component: MemberGalleryComponent
				},
				{
					path: ":photoId",
					component: MemberPhotoEditorComponent,
					resolve: { resolved: MemberPhotoEditorResolver },
					canDeactivate: [UnsavedChangesGuard]
				}]
			}]
		}]
	},
	{
		path: "messages",
		children: [{
			path: "",
			pathMatch: "full",
			component: MessagesComponent
		},
		{
			path: "threads",
			children: [{
				path: "",
				pathMatch: "full",
				component: ThreadsComponent
			},
			{
				path: ":id",
				component: ThreadMessagesComponent
			}],
		}]
	}]
},
{
	path: "**",
	redirectTo: "",
	pathMatch: "full"
}];

const AppRoutingModule = RouterModule.forRoot(routes, { useHash: false });
export default AppRoutingModule;
