﻿using Azure.Core;
using FluentValidation.Results;
using ITDesk.SignInResultNameSpace;
using ITDeskServer.Abstractions;
using ITDeskServer.DTOs;
using ITDeskServer.Models;
using ITDeskServer.Services;
using ITDeskServer.Validator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;

namespace ITDeskServer.Controllers;
[AllowAnonymous]
public class AuthController(
    UserManager<AppUser> userManager, 
    SignInManager<AppUser> signInManager,
    JwtService jwtService) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto request, CancellationToken cancellationToken)
    {
        LoginValidator validator = new();
        ValidationResult validationResult = validator.Validate(request);
      
        if (!validationResult.IsValid)
        {
            return StatusCode(422,validationResult.Errors.Select(s=> s.ErrorMessage));
        }

             AppUser? appUser = await userManager.FindByNameAsync(request.UserNameOrEmail); //UserName'in DB de olup, olmadığını sorgular. Geriye AppUser döner.
        if (appUser is null)
        {
            appUser = await userManager.FindByEmailAsync(request.UserNameOrEmail);
            if (appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı bulunamadı!" });
            }
        }

       var result = await signInManager.CheckPasswordSignInAsync(appUser, request.Password, false);

        if (result.IsLockedOut)
        {
            TimeSpan? timeSpan = appUser.LockoutEnd - DateTime.UtcNow;
            if(timeSpan is not null)
            return BadRequest(new { Message = $"Kullanıcınız 3 kere yanlış şifre girişinden dolayı {Math.Ceiling(timeSpan.Value.TotalMinutes)} dakika kitlendi!" });
        }

        if (result.IsNotAllowed)
        {
            return BadRequest(new { Message = "Mail adresiniz onaylı değil!" });
        }

        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Şifreniz yanlış!" });
        }

        string token = jwtService.CreateToken(appUser, request.RememberMe);

        return Ok(new {AccessToken= token });
    }

    [HttpPost]
    public async Task<IActionResult> GoogleLogin(GoogleLoginDto request, CancellationToken cancellationToken)
    {
        AppUser? appUser = await userManager.FindByEmailAsync(request.Email);
        if(appUser is not null)
        {
            string token = jwtService.CreateToken(appUser, true);
            return Ok(new { AccessToken = token });
        }

        string userName = request.Email;

            appUser=new()
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.FirstName.ToLower(),
                GoogleProvideId = request.Id
            };

       IdentityResult result =await userManager.CreateAsync(appUser);
        if(result.Succeeded)
        {
            string token = jwtService.CreateToken(appUser, true);
            return Ok(new { AccessToken = token });
        }

        IdentityError? errorResult = result.Errors.FirstOrDefault();

        string errorMessage =
            errorResult is null ? 
            "Giriş esnasında bir hatayla karşılaştık lütfen yöneticinize danışın!" :
            errorResult.Description; 
        
        return BadRequest(new { Message = errorMessage });
    }
}
