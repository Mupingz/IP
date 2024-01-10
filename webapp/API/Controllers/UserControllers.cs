﻿using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


#nullable disable
//[Authorize]
public class UsersController : BaseApiController
{
    private IMapper _mapper;
    private readonly IImageService _imageService;
    private IUserRepository _userRepository;

    public UsersController(IImageService imageService, IUserRepository userRepository, IMapper mapper)
    {
        _mapper = mapper;
        _imageService = imageService;
        _userRepository = userRepository;

    }
    private async Task<AppUser?> _GetUser()
    {
        var username = User.GetUsername();
        if (username is null) return null;
        return await _userRepository.GetUserByUserNameAsync(username);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        return Ok(await _userRepository.GetMembersAsync());
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto?>> GetUser(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return _mapper.Map<MemberDto>(user);
    }


    [HttpGet("username/{username}")]
    public async Task<ActionResult<MemberDto?>> GetUserByUserName(string username)
    {
        // var user = await _userRepository.GetUserByUserNameAsync(username);
        // return _mapper.Map<MemberDto>(user);
        return await _userRepository.GetMemberByUserNameAsync(username);
    }
    [HttpPut]
    public async Task<ActionResult> UpdateUserProfile(MemberUpdateDto memberUpdateDto)
    {
        var appUser = await _GetUser();
        //User.FindFirst(ClaimTypes.NameIdentifier)?.Value; //มาจาก TokenService.cs -> CreateToken
        // if (username is null) return Unauthorized();
        // var appUser = await _userRepository.GetUserByUserNameAsync(username);
        if (appUser is null) return NotFound();

        _mapper.Map(memberUpdateDto, appUser);
        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user profile!");
    }

    [HttpPost("add-image")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _GetUser();
        if (user is null) return NotFound();

        var result = await _imageService.AddImageAsync(file);
        if (result.Error is not null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);
        if (await _userRepository.SaveAllAsync())
        {
            return CreatedAtAction( //status 201
                    nameof(GetUserByUserName),
                    new { username = user.UserName },
                    _mapper.Map<PhotoDto>(photo)
                );
        }
        return BadRequest("something has gone wrong");
    }
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _GetUser();
        if (user is null) return NotFound(); //

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (photo is null) return NotFound(); //

        if (photo.IsMain) return BadRequest("this photo(id:" + photo.Id + ") is already main photo");

        // for (int i = 0; 1 < user.Photos.Count; i++)
        // {
        //     user.Photos[i].IsMain = false;
        // }

        var currentMainPhoto = user.Photos.FirstOrDefault(photo => photo.IsMain == true);
        if (currentMainPhoto is not null) currentMainPhoto.IsMain = false;
        photo.IsMain = true;

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Something has gone wrong!");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _GetUser();
        if (user is null) return NotFound();

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (photo is null) return NotFound();

        if (photo.IsMain) return BadRequest("can't delete main photo");

        if (photo.PublicId is not null)
        {
            var result = await _imageService.DeleteImageAsync(photo.PublicId);
            if (result.Error is not null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);
        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Something has gone wrong!");
    }
}

