using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WalkingTec.Mvvm.Core.Auth
{
    public class TokenService : ITokenService
    {
        private readonly ILogger _logger;
        private readonly JwtOptions _jwtOptions;

        private const Token _emptyToken = null;

        private readonly Configs _configs;
        private readonly IDataContext _dc;
        public IDataContext DC => _dc;

        public TokenService(
            ILogger<TokenService> logger,
            IOptions<JwtOptions> jwtOptions
        )
        {
            _configs = GlobalServices.GetRequiredService<Configs>();
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
            _dc = createDataContext();
        }

        public async Task<Token> IssueTokenAsync(LoginUserInfo loginUserInfo)
        {
            if (loginUserInfo == null)
                throw new ArgumentNullException(nameof(loginUserInfo));

            var signinCredentials = new SigningCredentials(_jwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: new List<Claim>()
                {
                    new Claim(AuthConstants.JwtClaimTypes.Subject, loginUserInfo.Id.ToString()),
                    new Claim(AuthConstants.JwtClaimTypes.Name, loginUserInfo.Name)
                },
                expires: DateTime.Now.AddSeconds(_jwtOptions.Expires),
                signingCredentials: signinCredentials
            );


            var refreshToken = new PersistedGrant()
            {
                UserId = loginUserInfo.Id,
                Type = "refresh_token",
                CreationTime = DateTime.Now,
                RefreshToken = Guid.NewGuid().ToString("N"),
                Expiration = DateTime.Now.AddSeconds(_jwtOptions.RefreshTokenExpires)
            };
            _dc.AddEntity(refreshToken);
            await _dc.SaveChangesAsync();

            return await Task.FromResult(new Token()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions),
                ExpiresIn = _jwtOptions.Expires,
                TokenType = AuthConstants.JwtTokenType,
                RefreshToken = refreshToken.RefreshToken
            });
        }

        private IDataContext createDataContext()
        {
            string cs = "default";
            var globalIngo = GlobalServices.GetRequiredService<GlobalData>();
            return (IDataContext)globalIngo.DataContextCI?.Invoke(new object[] { _configs.ConnectionStrings?.Where(x => x.Key.ToLower() == cs).Select(x => x.Value).FirstOrDefault(), _configs.DbType });
        }


        /// <summary>
        /// refresh token
        /// </summary>
        /// <param name="refreshToken">refreshToken</param>
        /// <returns></returns>
        public async Task<Token> RefreshTokenAsync(string refreshToken)
        {
            // 获取 RefreshToken
            PersistedGrant persistedGrant = await _dc.Set<PersistedGrant>().Where(x => x.RefreshToken == refreshToken).SingleOrDefaultAsync();
            if (persistedGrant != null)
            {
                // 校验 regresh token 有效期
                if (persistedGrant.Expiration < DateTime.Now)
                    throw new Exception("refresh token 已过期");

                // 删除 refresh token
                _dc.DeleteEntity(persistedGrant);
                await _dc.SaveChangesAsync();

                var user = await _dc.Set<FrameworkUserBase>()
                                    .Include(x => x.UserRoles)
                                    .Where(x => x.ID == persistedGrant.UserId)
                                    .SingleAsync();

                //生成并返回登录用户信息
                var loginUserInfo = new LoginUserInfo
                {
                    Id = user.ID,
                    ITCode = user.ITCode,
                    Name = user.Name,
                    PhotoId = user.PhotoId
                };

                // 清理过期 refreshtoken
                //var sql = $"DELETE FROM persistedgrants WHERE Expiration<'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
                //_dc.RunSQL(sql);
                await ClearExpiredRefreshTokenAsync();
                // 颁发 token
                return await IssueTokenAsync(loginUserInfo);
            }
            else
                throw new Exception("非法的 refresh Token");
        }

        /// <summary>
        /// clear expired refresh tokens
        /// </summary>
        /// <returns></returns>
        public async Task ClearExpiredRefreshTokenAsync()
        {
            var dataTime = DateTime.Now;
            var mapping = _dc.Model.FindEntityType(typeof(PersistedGrant)).Relational();
            var sql = $"DELETE FROM {mapping.TableName} WHERE Expiration<=@dataTime";
            _dc.RunSQL(sql, new
            {
                dataTime = dataTime
            });
            _logger.LogDebug("清理过期的refreshToken：【sql:{0}】", sql);
            await Task.CompletedTask;
        }

    }
}
