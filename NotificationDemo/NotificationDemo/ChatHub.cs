using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NotificationDemo
{
   public class ChatHub : Hub
   {
      private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

      public async Task SendMessage(string user, string message)
      {
         await Clients.User(user).SendAsync(message);
         await Clients.All.SendAsync("ReceiveMessage", $"{user} with connection {Context.ConnectionId}", message);
         //await Clients.Clients("Context.ConnectionId").SendAsync("ReceiveMessage", user, message);
      }

      public override Task OnConnectedAsync()
      {
         var httpContext = Context.GetHttpContext();
         if (httpContext != null)
         {
            //var jwtToken = httpContext.Request.Query["access_token"];
            var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI2NTNjMDliMS1jZjY2LTQzNjctOTAxYS05YjM0ZDc3MjgyY2YiLCJzdWIiOiJhbWl0Lmsuc2hhcm1hQG10eGIyYi5jb20iLCJlbWFpbCI6ImFtaXQuay5zaGFybWFAbXR4YjJiLmNvbSIsInVuaXF1ZV9uYW1lIjoiNmE5MTkyNzUtYWVlMC00OWZhLTljYzEtNTJjMjdlNzdiMDU0IiwiZ2l2ZW5fbmFtZSI6IkFtaXQiLCJmYW1pbHlfbmFtZSI6IlNoYXJtYSIsImV4cCI6MTYzMDk0MjY0MSwiaXNzIjoid3d3LmFiYy54eXoiLCJhdWQiOiJ3d3cuYWJjLnh5eiJ9.aPWVVStaXmMYf5yv69qTKYbVay0Ez3UpFWHOInF_0yg";
            var handler = new JwtSecurityTokenHandler();
            if (!string.IsNullOrEmpty(jwtToken))
            {
               var token = handler.ReadJwtToken(jwtToken);
               var tokenS = token as JwtSecurityToken;

               // replace email with your claim name
               var jti = tokenS.Claims.First(claim => claim.Type == "email").Value;
               if (jti != null && jti != "")
               {
                  _connections.Add(jti, Context.ConnectionId);
               }
            }
         }

         return base.OnConnectedAsync();
      }

      public override Task OnDisconnectedAsync(Exception? exception)
      {
         string name = Context.User.Identity.Name;
         _connections.Remove(name, Context.ConnectionId);
         return base.OnDisconnectedAsync(exception);
      }
   }
}
