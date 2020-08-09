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

			CreateMap<User, UserForSerialization>().ReverseMap();
			CreateMap<UserToRegister, User>().ReverseMap();
			CreateMap<UserToUpdate, User>().ReverseMap();
			CreateMap<User, UserForLoginDisplay>().ReverseMap();
			CreateMap<User, UserForList>()
				.ForMember(e => e.Age, opt => opt.MapFrom(e => DateTime.Today.Years(e.DateOfBirth)));
			CreateMap<User, UserForDetails>()
				.ForMember(e => e.Age, opt => opt.MapFrom(e => DateTime.Today.Years(e.DateOfBirth)))
				.ForMember(e => e.City, opt => opt.MapFrom(e => e.City != null ? e.City.Name : string.Empty))
				.ForMember(e => e.Country, opt => opt.MapFrom(e => e.City != null && e.City.Country != null ? e.City.Country.Name : string.Empty))
				.ForMember(e => e.Interests, opt => opt.MapFrom(e => e.UserInterests.Select(x => x.Interest.Name).ToArray()))
				.ForMember(e => e.Roles, opt => opt.MapFrom(e => e.UserRoles.Select(x => x.Role.Name).ToArray()));

			CreateMap<Photo, PhotoForList>();
			CreateMap<PhotoToEdit, Photo>().ReverseMap();

			CreateMap<IGrouping<string, Message>, MessageThread>()
				.ForMember(m => m.ThreadId, opt => opt.MapFrom(g => g.Key))
				.ForMember(m => m.Count, opt => opt.MapFrom(g => g.Count()))
				.ForMember(m => m.IsRead, opt => opt.MapFrom(g => g.All(e => e.DateRead == null)))
				.ForMember(m => m.LastModified, opt => opt.MapFrom(g => g.OrderByDescending(e => e.MessageSent).First().MessageSent));

			CreateMap<MessageToAdd, Message>().ReverseMap();
			CreateMap<MessageToEdit, Message>().ReverseMap();
			CreateMap<Message, MessageForList>()
				.ForMember(m => m.Sender, opt => opt.MapFrom(u => u.Sender))
				.ForMember(m => m.Recipient, opt => opt.MapFrom(u => u.Recipient));
		}
	}
}