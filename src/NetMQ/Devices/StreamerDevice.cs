using System;

namespace NetMQ.Devices
{
	/// <summary>
	/// Collects tasks from a set of pushers and forwards these to a set of pullers.
	/// </summary>
	/// <remarks>
	/// Generally used to bridge networks. Messages are fair-queued from pushers and
	/// load-balanced to pullers. This device is part of the pipeline pattern. The
	/// frontend speaks to pushers and the backend speaks to pullers.
	/// </remarks>
	public class StreamerDevice : DeviceBase<PullSocket, PushSocket>
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="ForwarderDevice"/> class that will run in a
		/// self-managed thread.
		/// </summary>
		/// <param name="context">The <see cref="Context"/> to use when creating the sockets.</param>
		/// <param name="frontendBindAddress">The endpoint used to bind the frontend socket.</param>
		/// <param name="backendBindAddress">The endpoint used to bind the backend socket.</param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the device.</param>
		public StreamerDevice(Context context, string frontendBindAddress, string backendBindAddress,
		                      DeviceMode mode = DeviceMode.Threaded)
			: this(context, mode) {

			FrontendSetup.Bind(frontendBindAddress);
			BackendSetup.Bind(backendBindAddress);
		}

		private StreamerDevice(Context context, DeviceMode mode)
			: base(context, context.CreatePullSocket(), context.CreatePushSocket(), mode) {
		}

		protected override void FrontendHandler(PullSocket socket) {
			bool more;

			do {
				var data = socket.Receive(out more);

				if (more)
					BackendSocket.SendMore(data);
				else {
					BackendSocket.Send(data);
				}
			} while (more);
		}
	}
}