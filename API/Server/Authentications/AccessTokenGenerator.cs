using Microsoft.IdentityModel.Tokens;
using Server.Models.Users;
using Server.UseCases.UserAuthentications;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Server.Authentications;
public class AccessTokenGenerator(string key, string issuer, string audience) : IAccessTokenGenerator
{
    private readonly string _key = key;

    private readonly string _issuer = issuer;

    private readonly string _audience = audience;

    /// <summary>
    /// トークンを生成する。
    /// </summary>
    /// <param name="userId">ユーザID</param>
    /// <returns>トークン</returns>
    public string Generate(UserId userId)
    {
        var secrityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        // 資格情報
        var credentials = new SigningCredentials(secrityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = [
            // sub（subject）クレームは JWT の主語となる主体の識別子である. JWT に含まれるクレームは, 通常 subject について述べたものである.  
            // https://openid-foundation-japan.github.io/draft-ietf-oauth-json-web-token-11.ja.html#subDef
            new(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            // jti (JWT ID) クレームは, JWT のための一意な識別子を提供する. その識別子の値は, 重複確率が無視できるほど十分低いことを保証できる方法で割り当てられなければならない
            // https://openid-foundation-japan.github.io/draft-ietf-oauth-json-web-token-11.ja.html#jtiDef
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddHours(1)
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }

    /// <summary>
    /// リクエストからユーザIDを解析する
    /// </summary>
    /// <param name="authHeader">認可ヘッダ文字列</param>
    /// <returns>ユーザID</returns>
    public static Guid ParseUserId(HttpRequest request)
    {
        var authHeader = request.Headers.Authorization.ToString();
        var handler = new JwtSecurityTokenHandler();
        // 認可ヘッダには「Bearer 」という接頭辞がついているため、取り除く
        authHeader = authHeader.Replace("Bearer ", "");
        var jwtSecurityToken = (JwtSecurityToken)handler.ReadToken(authHeader);
        // サブジェクトクレームにユーザIDがある
        var id = jwtSecurityToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
        // Guidとして解析し戻す
        return Guid.Parse(id);
    }

    /**
     * [参考]
     * https://qiita.com/te-k/items/600df0d0d812139de881
     * https://openid-foundation-japan.github.io/draft-ietf-oauth-json-web-token-11.ja.html
     */
}
