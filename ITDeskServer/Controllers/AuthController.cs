﻿using ITDeskServer.DTOs;
using ITDeskServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;

namespace ITDeskServer.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    public AuthController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto request, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _userManager.FindByNameAsync(request.UserNameOrEmail); //UserName'in DB de olup, olmadığını sorgular. Geriye AppUser döner.
        if (appUser is null)
        {
            appUser = await _userManager.FindByEmailAsync(request.UserNameOrEmail);
            if (appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı bulunamadı!" });
            }
        }

        if(appUser.WrongTryCount == 3)
        {
            TimeSpan timeSpan = appUser.LastWrongTry.Date - DateTime.Now.Date;
            if (timeSpan.TotalDays < 0)
            {
                appUser.WrongTryCount = 0;
                await _userManager.UpdateAsync(appUser);
            }
            else
            {
                timeSpan = appUser.LockDate - DateTime.Now;
                if (timeSpan.TotalMinutes <= 0)
                {
                    appUser.WrongTryCount = 0;
                    await _userManager.UpdateAsync(appUser);
                }
                else
                {
                    return BadRequest(new { ErrorMessage = $"Şifrenizi yanlış girdiğinizden dolayı kullanıcınız kitlendi {Math.Ceiling(timeSpan.TotalMinutes)} dakika daha beklemelisiniz!" });
                }
            }
           
        }


        var checkPasswordIsCorrect = await _userManager.CheckPasswordAsync(appUser, request.Password);
       
        if (!checkPasswordIsCorrect)
        {
            TimeSpan timeSpan = appUser.LastWrongTry.Date- DateTime.Now.Date;
            if (timeSpan.TotalDays <0)
            {
                appUser.WrongTryCount = 0;
                await _userManager.UpdateAsync(appUser);
            }
            if(appUser.WrongTryCount <3)
            {
                appUser.WrongTryCount++;
                appUser.LastWrongTry = DateTime.Now;
                await _userManager.UpdateAsync(appUser);
            }
           
            if(appUser.WrongTryCount == 3)
            {
                appUser.LastWrongTry = DateTime.Now;
                appUser.LockDate = DateTime.Now.AddMinutes(15);
                await _userManager.UpdateAsync(appUser);
                return BadRequest(new { Message = "3 kere şifrenizi yanlış girdiğinizden dolayı kullanıcınız kitlendi 15 dakika daha beklemelisiniz!" });
            }
            
            return BadRequest(new { Message =$"Şifre yanlış! Deneme {appUser.WrongTryCount} / 3 " });
        }
        appUser.WrongTryCount = 0;//Doğru şifre girildiğinde yanlış şifre sayısını sıfırlar.
        await _userManager.UpdateAsync(appUser);
        return Ok();
    }
}
