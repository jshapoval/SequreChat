using AutoMapper;
using Messenger.Common.DTOs;
using Messenger.Entities;

namespace Messenger.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Message, MessageDTO>();
            CreateMap<MessageDTO, Message>();

            CreateMap<Participant, ParticipantDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.MessengerUserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(x => x.MessengerUser.FullName))
                .ForMember(dest => dest.ImageId, opt => opt.MapFrom(x => x.MessengerUser.Avatar.Square_100Id));

            CreateMap<Dialog, DialogDTO>();
        }
    }
}
