using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OctanificationServer.Server
{

	internal class RestService {
	    internal void ServeRest(HttpListenerContext context) {
			RestWorker worker = new RestWorker();
			Task.Factory.StartNew(() => worker.Serve(context));
		}

		class RestWorker {
		    internal void Serve(HttpListenerContext context) {
				Console.WriteLine("starting serving " + context.Request.HttpMethod + " " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);
				string body = readRequstBody(context.Request);
				Console.WriteLine(body);

				Thread.Sleep(10000);
				context.Response.OutputStream.Write(Encoding.UTF8.GetBytes("Hello world!"), 0, 12);
				context.Response.Close();
				Console.WriteLine("finished serving " + context.Request.RawUrl + " on thread " + Thread.CurrentThread.ManagedThreadId);
			}

			private String readRequstBody(HttpListenerRequest request) {
				string result;
				using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
					result = reader.ReadToEnd();
				}
				return result;
			}
		}
	}
}
