using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using OctanificationServer.StaticData;


namespace OctanificationServer.Server
{

    class StaticsService
    {
        private readonly string _resourcesPath;

        internal StaticsService(string resourcesPath)
        {
            _resourcesPath = resourcesPath;
        }

        internal void ServeStatic(HttpListenerContext context)
        {
            Console.WriteLine("starting serving... " + context.Request.HttpMethod + " " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);
            Member[] staticData = new Member[] {};
            Member requstBody = ReadRequstBody(context);

            //Thread.Sleep(10000);

            Console.WriteLine("finished to serving... " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);

            if (File.Exists(_resourcesPath))
            {

                var json = new JavaScriptSerializer();

                using (StreamReader file = File.OpenText(_resourcesPath))
                {
                    Console.WriteLine("starting reading from the static file " + _resourcesPath);
                    try
                    {
                        string input = file.ReadToEnd();
                        staticData = JSONableExtensions.FromJsonArray<Member>(input);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        Console.WriteLine("finished reading from the static file " + _resourcesPath);
                        DataProcessing(requstBody, staticData);
                    }
                }
            }
            else
            {
                Console.WriteLine("Static data file doesn't exist");
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        private Member ReadRequstBody(HttpListenerContext context)
        {
            string body;
            var jss = new JavaScriptSerializer();
            Console.WriteLine("starting reading the request stream");

            // Get the data from the HTTP stream
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                body = reader.ReadToEnd();

                byte[] b = Encoding.UTF8.GetBytes("ACK");
                context.Response.StatusCode = 200;
                context.Response.KeepAlive = false;
                context.Response.ContentLength64 = b.Length;

                var output = context.Response.OutputStream;
                output.Write(b, 0, b.Length);
                context.Response.Close();
            }

            Console.WriteLine("finished reading the request stream");
            return JSONableExtensions.FromJson<Member>(body);
        }

        private object ReadStaticData(string resourcesPath)
        {
            object input;
            var jss = new JavaScriptSerializer();
            Console.WriteLine("starting reading the static file " + _resourcesPath);

            // Get the static data
            using (StreamReader file = File.OpenText(resourcesPath))
            {
                input = jss.DeserializeObject(file.ReadToEnd());
            }

            Console.WriteLine("finished reading the static file " + _resourcesPath);

            return input;
        }

        private void DataProcessing(Member input, Member[] staticInput)
        {
            Console.WriteLine("Starting data processing...");
            Member staticMember = null;

            staticMember = staticInput.FirstOrDefault(s => s.Name == input.Name);
 
            if (staticMember != null)
            {
                //Entities
                foreach (var entity in staticMember.Entities)
                {
                    foreach (var inputEntity in input.Entities)
                    {
                        if (inputEntity == entity)
                        {
                            input.Entities.Remove(entity);
                            break;
                        }
                    }
                }
                staticMember.Entities.AddRange(input.Entities);

                //Following
                foreach (var following in staticMember.Following)
                {
                    foreach (var inputFollowing in input.Following)
                    {
                        if (inputFollowing == following)
                        {
                            input.Following.Remove(following);
                            break;
                        }
                    }
                }
                staticMember.Following.AddRange(input.Following);

                if (staticMember.Url != input.Url) staticMember.Url = input.Url;
            }
            else
            {
                IList<Member> staticCollection = staticInput;
                staticCollection.Add(input);
            }

//            foreach (var member in staticInput)
//                foreach (var user in member.Following)
//                {
//                    if (user == input.Name) input.Followed[input.Followed.Length] = user;
//                }
//                foreach (var newUser in input.Following)
//                {
//                    if (newUser == member.Name) member.Followed[member.Followed.Length] = newUser;
//                }
//            }
//            if (!isExists) staticInput[staticInput.Length] = input;
//

            
            Console.WriteLine("Finshing to processing the data...");
        }

    }
}
