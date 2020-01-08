# ClusterPack

Cluster-as-a-Library project that aims to bring a [SWIM-based](http://www.cs.cornell.edu/~asdas/research/dsn02-swim.pdf) cluster membership protocol support in form of library that can be embedded into custom user application. It doesn't aim to introduce any kind of higher-level patterns or abstractions (like actors), only to simplify cluster membership management.

## APIs

There's are several basic abstractions used to compose everything together:

- `IMembership` which provides the cluster membership protocol itself. Currently SWIM-based membership implementation is in the works. Other modes could include cassandra style bully algorithm or HyParView.
- `ITransport` which abstracts transport layer. Currently TCP is supported, while (fully artificial and deterministic) test transport is in the works.
- `IDiscovery` which abstracts the way to receive intial set of seed nodes. Currently only static list is available, but other modes like mDNS or Consul arbiter could be provided.

### ITransport interface

```csharp
public interface ITransport
{
    /// <summary>
    /// Asynchronously sends provided <paramref name="payload"/> to a given <paramref name="destination"/>.
    /// Completes once the whole <paramref name="payload"/> has been received and acknowledged by remote side.
    /// </summary>
    ValueTask SendAsync(IPEndPoint destination, ReadOnlySequence<byte> payload, CancellationToken cancellationToken);
    
    /// <summary>
    /// Binds current transport to the given <paramref name="endpoint"/>, returning an asynchronous sequence
    /// of serialized messages send to that <paramref name="endpoint"/>.
    /// </summary>
    IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint, CancellationToken cancellationToken);
}
```

### IMembership interface

```csharp
public interface IMembership
{
    /// <summary>
    /// Returns a membership info of current node.
    /// </summary>
    Member Local { get; }
    
    /// <summary>
    /// Returns a membership info about all known active cluster members.
    /// </summary>
    ImmutableSortedSet<Member> ActiveMembers { get; }
    
    /// <summary>
    /// Returns an async sequence of user-defined messages incoming from remote members.
    /// </summary>
    IAsyncEnumerable<Message> IncomingMessages { get; }
    
    /// <summary>
    /// Returns an async sequence of membership events. This way you can react to changes in the cluster structure.
    /// </summary>
    IAsyncEnumerable<MembershipEvent> MembershipEvents { get; }
    
    /// <summary>
    /// Asynchronously sends a requested <paramref name="payload"/> to a given <paramref name="recipient"/>.
    /// Returned task completes after message was successfully send over the transport layer.
    /// </summary>
    ValueTask SendAsync(NodeId recipient, ReadOnlySequence<byte> payload);

    /// <summary>
    /// Asynchronously sends a requested <paramref name="payload"/> to all known active members of the cluster. 
    /// Returned task completes after message was successfully send over the transport layer to all members.
    /// </summary>
    ValueTask BroadcastAsync(ReadOnlySequence<byte> payload);
}
```

### IDiscovery interface

```csharp
public interface IDiscovery
{
    ValueTask<IEnumerable<EndPoint>> DiscoverNodesAsync(CancellationToken cancellationToken);
}
```