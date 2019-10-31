using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs
{
    //Hub 类管理连接、组和消息。
    //public class ChatHub : Hub
    //{
    //    public async Task SendMessage(string user, string message)
    //    {
    //        await Clients.All.SendAsync("ReceiveMessage", user, message);
    //    }


    //    public Task SendMessageToCaller(string user, string message)
    //    {
    //        return Clients.Caller.SendAsync("ReceiveMessage", user,message);
    //    }

    //}

    [Authorize]
    public class ChatHub : Hub
    {
      

        public async Task SendMessageToUser(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }


        public async Task<string> SendMessageToUser1(string user, string message)
        {
            return user + ":" + message;
        }

        /// <summary>
        /// 给某一个组发消息
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task SendToUserMessage(string groupName,string message)
        {
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.{message}");

        }


        /// <summary>
        /// 先添加到组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);//groupName 可以作为用户Id，每个用户Id作为一个分组，这样给每组发消息就等于给某个用户发消息
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }


        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public async Task CustomerObject(Request request)//使用自定义对象参数来确保向后兼容性
        {

        }


        public class Request
        {

        }

    }





    //public class ChatHub : Hub<IChatClient>
    //{
    //    [HubMethodName("SendMessageToUser")]//更改集线器方法的名称
    //    public async Task SendMessage(string user, string message)
    //    {
    //        var user1 = Context.User;//获取用户信息


    //        //await Clients.All.ReceiveMessage(user, message);//给所有用户发送消息




    //       // throw new HubException("This error will be sent to the client!");//使用HubException类抛异常


    //        //await Clients.Caller.SendAsync("ReceiveMessage", user, message); //只给调用者发送消息，其他人不发送

    //        //await Clients.Group("SignalR Users").SendAsync("ReceiveMessage", message);//给某一组发送消息
    //    }

    //    public override async Task OnConnectedAsync()
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");//客户端连接到集线器时候将用户添加到组
    //        await base.OnConnectedAsync();
    //    }

    //    public override async Task OnDisconnectedAsync(Exception exception)//每次打开一个浏览器就建立一个连接
    //    {
    //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");//客户端断开连接时候将用户移除组
    //        await base.OnDisconnectedAsync(exception);//网络故障exception将会有值,直接将浏览器关闭没有值
    //    }
    //}

    //public interface IChatClient
    //{
    //    Task ReceiveMessage(string user, string message);//这里的方法名称就是客户端要调用的js方法名称

    //}

}
