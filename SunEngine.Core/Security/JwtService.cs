using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Flurl.Util;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SunEngine.Core.Cache.Services;
using SunEngine.Core.Configuration.Options;
using SunEngine.Core.Controllers;
using SunEngine.Core.DataBase;
using SunEngine.Core.Managers;
using SunEngine.Core.Models;
using SunEngine.Core.Services;
using SunEngine.Core.Utils;

namespace SunEngine.Core.Security
{
    /// <summary>
    /// Functions for generating, renewing and reading Jwt tokens.
    /// </summary>
    public class JwtService : DbService
    {       
        private readonly SunUserManager userManager;
        private readonly JwtOptions jwtOptions;
        private readonly ILogger logger;
        private readonly IRolesCache rolesCache;

        public JwtService(
            DataBaseConnection db,
            SunUserManager userManager,
            IRolesCache rolesCache,
            IOptions<JwtOptions> jwtOptions,
            ILoggerFactory loggerFactory) : base(db)
        {
            this.userManager = userManager;
            this.jwtOptions = jwtOptions.Value;
            logger = loggerFactory.CreateLogger<AccountController>();
            this.rolesCache = rolesCache;
        }

        private JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        
        public async Task<SunClaimsPrincipal> RenewSecurityTokensAsync(HttpResponse response, int userId,
            LongSession longSession = null)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            return await RenewSecurityTokensAsync(response, user, longSession);
        }

        public Task<SunClaimsPrincipal> RenewSecurityTokensAsync(HttpResponse response, User user,
            long sessionId)
        {
            var longSession = db.LongSessions.FirstOrDefault(x=>x.Id == sessionId);
            return RenewSecurityTokensAsync(response, user, longSession);
        }

        public async Task<SunClaimsPrincipal> RenewSecurityTokensAsync(HttpResponse response, User user,
            LongSession longSession = null)
        {
            void GenerateTokens(LongSession longSession1)
            {
                longSession1.LongToken1 = CryptoRandomizer.GetRandomString(LongSession.LongToken1Length);
                longSession1.LongToken2 = CryptoRandomizer.GetRandomString(LongSession.LongToken2Length);
                longSession1.ExpirationDate = DateTime.UtcNow.AddDays(jwtOptions.LongTokenLiveTimeDays);
            }


            if (longSession == null)
            {
                longSession = new LongSession
                {
                    UserId = user.Id,
                    DeviceInfo = ""
                };

                GenerateTokens(longSession);

                longSession.Id = await db.InsertWithInt64IdentityAsync(longSession);
            }
            else
            {
                GenerateTokens(longSession);
                await db.UpdateAsync(longSession);
            }


            var lat2Token = CreateLong2AuthToken(longSession, out string lat2r);

            response.Cookies.Append(
                TokenClaimNames.LongToken2CoockiName,
                lat2Token,
                new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = longSession.ExpirationDate
                }
            );

            TokenAndClaimsPrincipal tokenAndClaimsPrincipal =
                await GenerateShortAuthTokenAsync(user, lat2r, longSession.LongToken2,longSession.Id);

            string json = JsonConvert.SerializeObject(new
            {
                LongToken = new
                {
                    Token = longSession.LongToken1,
                    Expiration = longSession.ExpirationDate.ToInvariantString()
                },
                ShortToken = tokenAndClaimsPrincipal.Token
            }, jsonSerializerSettings);

            response.Headers.Add(Headers.TokensHeaderName, json);

            return tokenAndClaimsPrincipal.ClaimsPrincipal;
        }


        private class TokenAndClaimsPrincipal
        {
            public string Token;
            public SunClaimsPrincipal ClaimsPrincipal;
        }

        private async Task<TokenAndClaimsPrincipal> GenerateShortAuthTokenAsync(User user, string lat2r, string lat2, long sessionId)
        {
            // Generate and issue a JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.ShortJwtSecurityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(TokenClaimNames.LongToken2Ran, lat2r),
                new Claim(JwtRegisteredClaimNames.Jti, CryptoRandomizer.GetRandomString(DbColumnSizes.BlackListShortToken_TokenId))
            };

            var roleNames = await userManager.GetRolesAsync(user);

            foreach (var role in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Issuer,
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddMinutes(jwtOptions.ShortTokenLiveTimeMinutes),
                signingCredentials: credentials);

            var claimsIdentity =  new ClaimsIdentity(claims,"JwtShortToken");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return new TokenAndClaimsPrincipal
            {
                ClaimsPrincipal = new SunClaimsPrincipal(claimsPrincipal, rolesCache, sessionId, lat2),
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        public JwtSecurityToken ReadLongToken2(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.LongJwtSecurityKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Issuer,
                IssuerSigningKey = key
            };

            try
            {
                var principal =
                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                if (principal != null)
                    return (JwtSecurityToken) securityToken;
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public ClaimsPrincipal ReadShortToken(string token, out SecurityToken securityToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.ShortJwtSecurityKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Issuer,
                IssuerSigningKey = key
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            }
            catch
            {
                securityToken = null;
                return null;
            }
        }

        private string CreateLong2AuthToken(LongSession longSession, out string lat2r)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.LongJwtSecurityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            lat2r = CryptoRandomizer.GetRandomString(10);
            
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, longSession.UserId.ToString()),
                new Claim(TokenClaimNames.LongToken2Ran, lat2r),
                new Claim(TokenClaimNames.LongToken2Db, longSession.LongToken2),
                new Claim(TokenClaimNames.SessionId, longSession.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Issuer,
                claims: claims.ToArray(),
                expires: longSession.ExpirationDate,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public void MakeLogoutCookiesAndHeaders(HttpResponse response)
        {
            response.Headers.Clear();
            
            response.Cookies.Delete(TokenClaimNames.LongToken2CoockiName,
                new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    IsEssential = true
                });

            response.Headers.Add(Headers.TokensHeaderName, Headers.TokensExpireValue); 
        }
    }
}