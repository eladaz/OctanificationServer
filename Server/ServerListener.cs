using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OctanificationServer.Server
{

	public class ServerListener
    {
		private readonly int _port;
		private readonly string _resourcesPath;
		private bool _shuttingDown;

		private Thread _mainThread;
		private HttpListener _listener;

		private readonly RestService _restsService;
		private readonly StaticsService _staticsService;

		public ServerListener(int port, string resourcesPath) {
			_port = port;
			_resourcesPath = resourcesPath;
			_shuttingDown = false;

			_restsService = new RestService(resourcesPath);
			_staticsService = new StaticsService(resourcesPath);
		}

        private string resolveAddress()
        {
            //return Dns.GetHostEntry("localhost").HostName;
            string localIp = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                }
            }
            return localIp;
        }

        #region Public APIs
        public void StartServer() {

            if (_mainThread == null || _mainThread.ThreadState != ThreadState.Running) {
				_mainThread = new Thread(StartService);
				_mainThread.Name = "Server Main Thread";
				_mainThread.Start();
				Console.WriteLine("Server started on thread {0} and listening on {1}:{2}", _mainThread.ManagedThreadId, resolveAddress(), _port);
			} else {
				Console.WriteLine("Server may be started only once");
			}
		}

		public void Stop() {
			Console.WriteLine("Server shutting down... ");
			_listener.Stop();
			_shuttingDown = true;
		}
		#endregion

		#region Internals
		private void StartService() {
            string localIp = resolveAddress();
            string serverUrl = string.Format("http://localhost:{0}/", _port);
            //string serverUrl = string.Format("http://{0}:{1}/", localIp, _port);

            _listener = new HttpListener();
			_listener.Prefixes.Add(serverUrl);
			_listener.Start();

			//	main event loop
			while (!_shuttingDown) {
				try {
					Console.WriteLine("Waiting for the next connection...");
					HttpListenerContext context = _listener.GetContext();
					if (context.Request.RawUrl.IndexOf("/static", StringComparison.Ordinal) == 0) {
                       _staticsService.ServeStatic(context);
					} else {
						_restsService.ServeRest(context);
					}
				} catch (HttpListenerException le) {
					if (le.ErrorCode == 995) Console.WriteLine("Done");
					else throw le;
				} catch (Exception e) {
					throw e;
				}
			}

			_listener.Stop();
			_listener.Close();
		}

		private async void ServeStatic(string path, HttpListenerContext context) {
			string filePath = Path.Combine(_resourcesPath, path.Substring(8));
			if (File.Exists(filePath)) {
				using (FileStream fs = File.OpenRead(filePath)) {
					Console.WriteLine("starting serving the static file " + path);
					try {
						await fs.CopyToAsync(context.Response.OutputStream);
					} catch (Exception e) {
						Console.WriteLine(e.Message);
					} finally {
						context.Response.Close();
						Console.WriteLine("finished serving the static file " + path);
					}
				}
			} else {
				context.Response.StatusCode = 404;
				context.Response.Close();
			}
		}


        #endregion
    }
}