using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public UserRepository(IMapper mapper, DataContext dataContext)
    {
        _mapper = mapper;
        _dataContext = dataContext;
    }

    public async Task<MemberDto?> GetMemberByUserNameAsync(string username)
    {
        return await _dataContext.Users

             .Where(user => user.UserName == username)
             .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
             .SingleOrDefaultAsync();
    }



    public async Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var minBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        var query = _dataContext.Users.AsQueryable();
        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(user => user.Created),
            _ => query.OrderByDescending(user => user.LastActive),
        };

        query.AsNoTracking();

        query = query.Where(user => user.BirthDate >= minBirthDate && user.BirthDate <= maxBirthDate);

        query = query.Where(user => user.UserName != userParams.CurrentUserName);
        if (userParams.Gender != "non-binary")
            query = query.Where(user => user.Gender == userParams.Gender);

        return await PageList<MemberDto>.CreateAsync(
            query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
            userParams.PageNumber,
            userParams.PageSize);
    }

    // public Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams)
    // {
    //     throw new NotImplementedException();
    // }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _dataContext.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUserNameWithOutPhotoAsync(string username)
    {
        return await _dataContext.Users
                // .Include(user => user.Photos)
                .SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<AppUser?> GetUserByUserNameAsync(string username)
    {
        return await _dataContext.Users
                .Include(user => user.Photos)
                .SingleOrDefaultAsync(user => user.UserName == username);
    }

    // public async Task<IEnumerable<AppUser>> GetUsersAsync()
    // {
    //     return await _dataContext.Users.Include(user => user.Photos).ToListAsync();
    // }

    public async Task<bool> SaveAllAsync()
    {
        return await _dataContext.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        _dataContext.Entry(user).State = EntityState.Modified;
    }


}