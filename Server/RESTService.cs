using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using OctanificationServer.StaticData;

namespace OctanificationServer.Server
{

	internal class RestService {

        private readonly string _resourcesPath;

        internal RestService(string resourcesPath)
        {
            _resourcesPath = resourcesPath;
        }

        internal void ServeRest(HttpListenerContext context) {
			RestWorker worker = new RestWorker();
			Task.Factory.StartNew(() => worker.Serve(context, _resourcesPath));
		}

		class RestWorker {

            private Member[] _staticData;
		    private List<Dictionary<string, object>> outputList;

            internal void Serve(HttpListenerContext context, string resourcesPath) {
				Console.WriteLine("starting serving " + context.Request.HttpMethod + " " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);
                string body = readRequstBody(context.Request);
                Console.WriteLine(body);

                _staticData = new Member[] { };
                Dictionary<string, object> dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(body);


                Thread.Sleep(10000);

                using (StreamReader reader = new StreamReader(resourcesPath, Encoding.Unicode))
                {
                    Console.WriteLine("starting reading from the static file " + resourcesPath);
                    try
                    {
                        _staticData = JSONableExtensions.FromJsonArray<Member>(reader.ReadToEnd());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        byte[] b = Encoding.UTF8.GetBytes("ACK");
                        context.Response.StatusCode = 200;
                        context.Response.KeepAlive = false;
                        context.Response.ContentLength64 = b.Length;

                        var output = context.Response.OutputStream;
                        output.Write(b, 0, b.Length);
                        context.Response.Close();

                        Console.WriteLine("finished reading from the static file " + resourcesPath);
                        DataProcessing(dict);
                    }
                }
                Console.WriteLine("finished serving " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);
			}

			private string readRequstBody(HttpListenerRequest request) {
				string result;

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
                    result = reader.ReadToEnd();
                }
				return result;
			}

            class DataSend
            {
                private string _name;
                private List<string> _value;

                public DataSend(string name, List<string> value)
                {
                    this._name = name;
                    this._value = value;
                }
            }

            private void DataProcessing(Dictionary<string, object> input) {
                Console.WriteLine("Starting data processing...");

                outputList = new List<Dictionary<string, object>>();
                Member staticMember = _staticData.FirstOrDefault(s => s.Name == input["userName"].ToString());

                if (staticMember != null)
                {
                    //username
                    foreach (var entity in staticMember.Entities)
                    {
                        if (entity == input["entityType"].ToString())
                        {
                            input.Add("url", staticMember.Url);
                            input.Add("by", null);
                            outputList.Add(input);
                            break;
                        }
                        
                    }

                    foreach (var follow in staticMember.Followed)
                    {
                        var fl = new Dictionary<string, object>(input);
                        Member member = _staticData.FirstOrDefault(s => s.Name == follow);
                        if (member != null)
                        {
                            foreach (var entity in member.Entities)
                            {
                                if (entity == fl["entityType"].ToString())
                                {
                                    if (fl.ContainsKey("url")) fl["url"] = member.Url;
                                    else fl.Add("url", member.Url);

                                    if (fl.ContainsKey("by")) fl["by"] = fl["userName"].ToString();
                                    else fl.Add("by", fl["userName"].ToString());

                                    fl["userName"] = member.Name;

                                    outputList.Add(fl);
                                    break;
                                }
                            }
                        }  
                    }
                }
                else
                {
                    Console.WriteLine("The user does not exist in the list, do not send anything!");
                }

                SentJsonRest();
                Console.WriteLine("Finshing to processing the data...");
            }

            private void SentJsonRest()
            {
                var jss = new JavaScriptSerializer();

                foreach (var user in outputList)
                {
                    var json = jss.Serialize(user);
                    Console.WriteLine(json.ToString());

                    Console.WriteLine("Sending Message from ALM Octane for the user " + user["userName"].ToString());

                    string result = "";
                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        //result = client.UploadString(user["url"].ToString(), "POST", json);
                    }
                    Console.WriteLine(result);
                    Console.WriteLine("Waiting for the next connection...");
                    Console.WriteLine(result);
                }
            }

        }
    }
}
