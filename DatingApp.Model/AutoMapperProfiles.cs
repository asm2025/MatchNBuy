using System;
using System.Linq;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Patterns.Pagination;
using AutoMapper;
using DatingApp.Model.Parameters;
using DatingApp.Model.TransferObjects;

namespace DatingApp.Model
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			CreateMap<SortablePagination, ListSettings>().ReverseMap();
			CreateMap<UserSortablePagination, ListSettings>().ReverseMap();
			CreateMap<UserList, ListSettings>().ReverseMap();

			CreateMap<Country, CountryForList>();

			CreateMap<City, CityForList>();

			CreateMap<User, UserForSerialization>().ReverseMap();
			CreateMap<UserToRegister, User>().ReverseMap();
			CreateMap<UserToUpdate, User>().ReverseMap();
			CreateMap<User, UserForList>()
				.ForMember(e => e.Age, opt => opt.MapFrom(e => DateTime.Today.Years(e.DateOfBirth)))
				.ForMember(e => e.Likers, opt => opt.MapFrom(e => e.Likers.Count))
				.ForMember(e => e.Likees, opt => opt.MapFrom(e => e.Likees.Count));
			CreateMap<User, UserForDetails>()
				.ForMember(e => e.Age, opt => opt.MapFrom(e => DateTime.Today.Years(e.DateOfBirth)))
				.ForMember(e => e.Likers, opt => opt.MapFrom(e => e.Likers.Count))
				.ForMember(e => e.Likees, opt => opt.MapFrom(e => e.Likees.Count))
				.ForMember(e => e.City, opt => opt.MapFrom(e => e.City != null ? e.City.Name : string.Empty))
				.ForMember(e => e.Country, opt => opt.MapFrom(e => e.City != null && e.City.Country != null ? e.City.Country.Name : string.Empty))
				.ForMember(e => e.Interests, opt => opt.MapFrom(e => e.UserInterests.Select(x => x.Interest.Name).ToArray()))
				.ForMember(e => e.Roles, opt => opt.MapFrom(e => e.UserRoles.Select(x => x.Role.Name).ToArray()));

			CreateMap<Photo, PhotoForList>();
			CreateMap<PhotoToEdit, Photo>().ReverseMap();

			CreateMap<MessageToAdd, Message>().ReverseMap();
			CreateMap<Message, MessageForList>()
				.ForMember(m => m.SenderPhotoUrl, opt => opt.MapFrom(u => u.Sender.PhotoUrl))
				.ForMember(m => m.RecipientPhotoUrl, opt => opt.MapFrom(u => u.Recipient.PhotoUrl));
		}
	}
}