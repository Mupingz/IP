using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Company.ClassLibrary1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

//[Authorize]
public class UsersController : BaseApiController
{
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public UsersController(IImageService imageService, IMapper mapper, IUserRepository userRepository)
    {
        _imageService = imageService;
        _mapper = mapper;
        _userRepository = userRepository;
    }
    private async Task<AppUser?> _GetUser()
    {
        var username = User.GetUsername();
        if (username is null) return null;
        return await _userRepository.GetUserByUserNameAsync(username);
    }

    //[Authorize(Roles = "Administrator")]
    [HttpGet]

    public async Task<ActionResult<PageList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var username = User.GetUsername();
        if (username is null) return NotFound();

        var currentUser = await _userRepository.GetUserByUserNameAsync(username);
        if (currentUser is null) return NotFound();
        userParams.CurrentUserName = currentUser.UserName;
        if (string.IsNullOrEmpty(userParams.Gender))
        {
            if (currentUser.Gender != "non-binary")
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            else
                userParams.Gender = "non-binary";
        }

        var pages = await _userRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(
            new PaginationHeader(pages.CurrentPage, pages.PageSize, pages.TotalCount, pages.TotalPages));
        return Ok(pages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto?>> GetUser(int id)
    {
        var aappuser = await _userRepository.GetUserByIdAsync(id);
        return _mapper.Map<MemberDto>(await _userRepository.GetUserByIdAsync(id));
    }

    [Authorize(Roles = "Administrator,Moderator,Member")]
    [HttpGet("username/{username}")]
    public async Task<ActionResult<MemberDto?>> GetUserByUserName(string username)
    {
        //var user = await _userRepository.GetUserByUserNameAsync(username);
        //return _mapper.Map<MemberDto>(user);
        return await _userRepository.GetMemberByUserNameAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserProfile(MemberUpdateDto memberUpdateDto)
    {
        var appUser = await _GetUser(); //User.FindFirst(ClaimTypes.NameIdentifier)?.Value; //มาจาก TokenService.cs -> CreateToken
        // if (appUser is null) return Unauthorized();

        // var appUser = await _userRepository.GetUserByUserNameAsync(appUser.UserName);
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
        // return BadRequest("user not found");
        var result = await _imageService.AddImageAsync(file);
        if (result.Error is not null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        if (user.Photos.Count == 0)
            photo.IsMain = true;

        user.Photos.Add(photo);
        if (await _userRepository.SaveAllAsync()) //return _mapper.Map<PhotoDto>(photo);
            return CreatedAtAction( //status 201
                    nameof(GetUserByUserName),
                    new { username = user.UserName },
                    _mapper.Map<PhotoDto>(photo)
                );
        return BadRequest("Something has gone wrong!");
    }
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _GetUser();
        if (user is null) return NotFound();
        // return BadRequest("user not found");
        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (photo is null) return NotFound();
        // return BadRequest("Photo " + photoId + " not found , photo.count = " + user.Photos.Count);
        if (photo.IsMain) return BadRequest("this photo(id:" + photoId + ") is already main photo");

        // for (int i = 0; i < user.Photos.Count; i++)
        // {
        //     user.Photos[i].IsMain = false;
        // }
        var currentMainPhoto = user.Photos.FirstOrDefault(photo => photo.IsMain == true);
        if (currentMainPhoto is not null)
            currentMainPhoto.IsMain = false;
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