using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Configuration.Settings;
using Stratis.Bitcoin.P2P.Peer;
using Stratis.Bitcoin.P2P.Protocol;
using Stratis.Bitcoin.P2P.Protocol.Behaviors;

namespace Stratis.Bitcoin.Connection
{
    /// <summary>
    /// A behaviour that will manage the lifetime of peers.
    /// </summary>
    public class PeerBanningBehavior : NetworkPeerBehavior
    {
        /// <summary>Logger factory to create loggers.</summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>Handle the lifetime of a peer.</summary>
        private readonly IPeerBanning peerBanning;

        /// <summary>The node settings.</summary>
        private readonly NodeSettings nodeSettings;

        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        public PeerBanningBehavior(ILoggerFactory loggerFactory, IPeerBanning peerBanning, NodeSettings nodeSettings)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.loggerFactory = loggerFactory;
            this.peerBanning = peerBanning;
            this.nodeSettings = nodeSettings;
        }

        /// <inheritdoc />
        protected override void DetachCore()
        {
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new PeerBanningBehavior(this.loggerFactory, this.peerBanning, this.nodeSettings);
        }

        /// <inheritdoc />
        protected override void AttachCore()
        {
            this.logger.LogTrace("()");

            INetworkPeer peer = this.AttachedPeer;
            if (peer.State == NetworkPeerState.Connected)
            {
                if (this.peerBanning.IsBanned(peer.RemoteSocketEndpoint))
                {
                    this.logger.LogDebug("Peer '{0}' was previously banned.", peer.RemoteSocketEndpoint);
                    peer.Disconnect("A banned node tried to connect.");
                    this.logger.LogTrace("(-)[PEER_BANNED]");
                    return;
                }
            }

            this.logger.LogTrace("(-)");
        }
    }
}
