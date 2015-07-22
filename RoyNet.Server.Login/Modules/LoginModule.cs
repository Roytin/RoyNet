﻿using System;
using System.Linq;
using Dapper;
using Nancy;

namespace RoyNet.Server.Login
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get["/login/{uname}/{pwd}"] = p =>
            {
                Console.WriteLine("{0},{1} want login", p["uname"], p["pwd"]);
                using (var conn = ConnectionProvider.Connection)
                {
                    string uid = conn.Query<string>("select uid from Account where username=@username and password=md5(@password)", new
                    {
                        username = p["uname"],
                        password = p["pwd"]
                    }).FirstOrDefault();
                    if (uid != null)
                    {
                        string token = TokenManager.CreateToken(uid);
                        return Response.AsJson(new { Result = "OK", Token = token });//, ServerList = LoginServer.Config.GameServers
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };

            Get["/reg/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    var account = new
                    {
                        username = p["uname"],
                        password = p["pwd"]
                    };
                    int ret = conn.Execute("insert into Account(`username`,`password`) values(@username,@password)", account);
                    if (ret == 1)
                    {
                        string uid = conn.Query<string>("select uid from Account where username=@username", account).First();
                        string token = TokenManager.CreateToken(uid);
                        return Response.AsJson(new { Result = "OK", Token = token });//, ServerList = LoginServer.Config.GameServers 
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };

            Get["/chk/{token}"] = p =>
            {
                string uid;
                if (TokenManager.Check(p["token"], out uid))
                {
                    return Response.AsJson(new { Result = "OK", UID = uid });
                }
                else
                {
                    return Response.AsJson(new { Result = "Failed" });
                }
            };
        }
    }
}
