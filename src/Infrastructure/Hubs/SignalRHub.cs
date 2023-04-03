// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using CleanArchitecture.Blazor.Infrastructure.Constants;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace CleanArchitecture.Blazor.Infrastructure.Hubs;

public interface ISignalRHub
{
    Task Start(string message);
    Task Completed(string message);
    Task SendMessage(string from,string message);
    Task Disconnect(string userId);
    Task Connect(string userId);
    Task SendNotification(string message);
}
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SignalRHub : Hub<ISignalRHub>
{
    private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();


    public SignalRHub()
    {

    }
    public override async Task OnConnectedAsync()
    {
        var id = Context.ConnectionId;
        var userName = Context.User?.Identity?.Name ?? string.Empty;
        if (!_onlineUsers.ContainsKey(id))
        {
            _onlineUsers.TryAdd(id, userName);
        }
        await Clients.All.Connect(userName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        //try to remove key from dictionary
        if (_onlineUsers.TryRemove(id, out string? userName))
        {
            await Clients.All.Disconnect(userName);
        }

    }
    public async Task SendMessage(string message)
    {
        var userName = Context.User?.Identity?.Name ?? string.Empty;
        await Clients.All.SendMessage(userName, message);
    }
    public async Task SendNotification(string message)
    {
        await Clients.All.SendNotification(message);
    }
    public async Task Completed(string message)
    {
        await Clients.All.Completed(message);
    }
    private bool _disposed = false;
    // Protected implementation of Dispose pattern.
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        // Call base class implementation.
        base.Dispose(disposing);
    }
}
