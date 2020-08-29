using AutoMapper;
using Messenger.EF;
using System;
using System.Threading.Tasks;
using Messenger.Common.DTOs;
using Messenger.Entities;

namespace Messenger.Repositories
{
    public class FileService
    {
        private readonly IMapper _mapper;
        private readonly MessengerDbContext db;        

        public FileService(MessengerDbContext db, IMapper mapper)
        {
            this.db = db;
            _mapper = mapper;
        }

        public async Task Create(FileDTO fileDTO)
        {
            var file = _mapper.Map<FileDTO, File>(fileDTO);
            await db.Files.AddAsync(file);
            await db.SaveChangesAsync();
        }

        public async Task<FileDTO> GetById(Guid id)
        {
            var file = await db.Files.FindAsync(id);

            if (file == null)
                throw new ApplicationException("File not found");

            return _mapper.Map<File, FileDTO>(file);
        }
    }
}
