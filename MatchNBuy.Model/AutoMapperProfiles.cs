using System;
using System.Linq;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Patterns.Pagination;
using AutoMapper;
using MatchNBuy.Model.Parameters;
using MatchNBuy.Model.TransferObjects;

namespace MatchNBuy.Model
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			CreateMap<SortablePagination, ListSettings>().ReverseMap();
			CreateMap<UserList, ListSettings>().ReverseMap();

			CreateMap<Country, CountryForList>();

			CreateMap<City, CityForList>();

			CreateMap<UserToRegister, User>().ReverseMap();
			CreateMap<UserToUpdate, User>().ReverseMap();
			CreateMap<User, UserForLoginDisplay>()
				.ForMember(e => e.Age, opt => opt.MapFrom(e => DateTime.Today.Years(e.DateOfBirth)))
				.ForMember(e => e.CountryCode, opt => opt.MapFrom(e => e.City == null ? string.Empty : e.City.CountryCode))
				.ForMember(e => e.Country, opt => opt.MapFrom(e => e.City != null && e.City.Country != null ? e.City.Country.Name : string.Empty))
				.ForMember(e => e.City, opt => opt.MapFrom(e => e.City != null ? e.City.Name : string.Empty));
			CreateMap<User, UserForList>()
				.IncludeBase<User, UserForLoginDisplay>()
				.ForMember(e => e.Likes, opt => opt.MapFrom(e => e.Likers.Count));
			CreateMap<User, UserForDetails>()
				.IncludeBase<User, UserForList>()
				.ForMember(e => e.Interests, opt => opt.MapFrom(e => e.UserInterests.Select(x => x.Interest.Name).ToArray()))
				.ForMember(e => e.Roles, opt => opt.MapFrom(e => e.UserRoles.Select(x => x.Role.Name).ToArray()));
			CreateMap<User, UserForSerialization>()
				.IncludeBase<User, UserForList>();

			CreateMap<Photo, PhotoForList>();
			CreateMap<PhotoToEdit, Photo>().ReverseMap();

			CreateMap<Thread, MessageThread>()
				.ForMember(m => m.ThreadId, opt => opt.MapFrom(e => e.Id))
				.ForMember(m => m.Count, opt => opt.MapFrom(e => e.Messages.Count))
				.ForMember(m => m.IsRead, opt => opt.MapFrom(e => e.Messages.All(e => e.DateRead == null)))
				.ForMember(m => m.LastModified, opt => opt.MapFrom(e => e.Modified));

			CreateMap<MessageToAdd, Message>().ReverseMap();
			CreateMap<MessageToEdit, Message>().ReverseMap();
			CreateMap<MessageForList, Message>().ReverseMap();
		}
	}
}