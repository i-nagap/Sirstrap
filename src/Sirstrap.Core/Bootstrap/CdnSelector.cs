namespace Sirstrap.Core.Bootstrap
{
    public sealed class CdnSelector(HttpClient httpClient, SirstrapSettings settings, CdnRanking ranking, ITelemetry telemetry)
    {
        private static readonly TimeSpan _probeTimeout = TimeSpan.FromSeconds(5);

        private int _rankingGeneration;

        private string Commit(string baseUri, IReadOnlyList<string> baseUrisFastestFirst, CdnResolutionSource source)
        {
            ranking.PrimaryBaseUri = baseUri;
            ranking.BaseUrisFastestFirst = baseUrisFastestFirst;

            RecordResolution(baseUri, source);

            return baseUri;
        }

        private async Task<CdnProbeResult?> ProbeAsync(string baseUri, RobloxDeploymentTarget target, CancellationToken cancellationToken)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            timeout.CancelAfter(_probeTimeout);

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, RobloxPackageUris.ForProbe(target, baseUri));
                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Debug("A Roblox download server did not respond correctly (Server: {0}, HTTP status: {1}).", baseUri, (int)response.StatusCode);

                    return RecordProbe(baseUri, stopwatch, isSuccessful: false);
                }

                return RecordProbe(baseUri, stopwatch, isSuccessful: true);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                Log.Debug(ex, "A Roblox download server did not respond in time (Server: {0}, Timeout: {1} s).", baseUri, _probeTimeout.TotalSeconds);

                return RecordProbe(baseUri, stopwatch, isSuccessful: false);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not reach a Roblox download server (Server: {0}).", baseUri);

                return RecordProbe(baseUri, stopwatch, isSuccessful: false);
            }
        }

        private async Task<(string BaseUri, CdnResolutionSource Source)> ProbeFastestAsync(RobloxDeploymentTarget target, int generation, CancellationToken cancellationToken)
        {
            Log.Information("Looking for the fastest Roblox download server.");

            List<Task<CdnProbeResult?>> pendingProbes = [.. RobloxCdn.KnownBaseUris.Select(baseUri => ProbeAsync(baseUri, target, cancellationToken))];
            List<string> respondingBaseUris = [];

            PublishRanking(generation, respondingBaseUris);

            while (pendingProbes.Count > 0)
            {
                Task<CdnProbeResult?> completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                _ = pendingProbes.Remove(completedProbe);

                CdnProbeResult? result = await completedProbe.ConfigureAwait(false);

                if (result == null)
                {
                    continue;
                }

                respondingBaseUris.Add(result.BaseUri);

                PublishRanking(generation, respondingBaseUris);

                Log.Information("Using the fastest Roblox download server (Server: {0}, Response time: {1} ms).", result.BaseUri, (int)result.Elapsed.TotalMilliseconds);

                _ = RankRemainingProbesAsync(generation, respondingBaseUris, pendingProbes);

                return (result.BaseUri, CdnResolutionSource.Probe);
            }

            Log.Warning("No Roblox download server responded, using the default one (Server: {0}).", RobloxCdn.DefaultBaseUri);

            return (RobloxCdn.DefaultBaseUri, CdnResolutionSource.Fallback);
        }

        private void PublishRanking(int generation, List<string> respondingBaseUris)
        {
            if (Volatile.Read(ref _rankingGeneration) != generation)
            {
                return;
            }

            ranking.BaseUrisFastestFirst = [.. respondingBaseUris, .. RobloxCdn.KnownBaseUris.Where(baseUri => !respondingBaseUris.Contains(baseUri))];
        }

        private async Task RankRemainingProbesAsync(int generation, List<string> respondingBaseUris, List<Task<CdnProbeResult?>> pendingProbes)
        {
            while (pendingProbes.Count > 0)
            {
                Task<CdnProbeResult?> completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                _ = pendingProbes.Remove(completedProbe);

                if (await completedProbe.ConfigureAwait(false) is not
                    {
                    } result)
                {
                    continue;
                }

                respondingBaseUris.Add(result.BaseUri);

                PublishRanking(generation, respondingBaseUris);
            }
        }

        private CdnProbeResult? RecordProbe(string baseUri, Stopwatch stopwatch, bool isSuccessful)
        {
            stopwatch.Stop();
            telemetry.RecordDistribution("cdn.probe.duration", stopwatch.Elapsed.TotalMilliseconds, MeasurementUnit.Duration.Millisecond, new Dictionary<string, object>
            {
                ["baseUri"] = baseUri,
                ["success"] = isSuccessful
            });

            return isSuccessful ? new CdnProbeResult(baseUri, stopwatch.Elapsed) : null;
        }

        private void RecordResolution(string baseUri, CdnResolutionSource source)
        {
            telemetry.RecordCounter("cdn.resolved", new Dictionary<string, object>
            {
                ["baseUri"] = baseUri,
                ["source"] = source.ToString()
            });
        }

        public async Task<string> SelectAsync(RobloxDeploymentTarget target, CancellationToken cancellationToken = default)
        {
            int generation = Interlocked.Increment(ref _rankingGeneration);
            string cdnOverride = RobloxCdn.NormalizeBaseUri(settings.RobloxCdnUriOverride);

            settings.RobloxCdnUriOverride = cdnOverride;

            if (!string.IsNullOrEmpty(cdnOverride))
            {
                Log.Information("Using the Roblox download server set in the settings (Server: {0}).", cdnOverride);

                return Commit(cdnOverride, [cdnOverride], CdnResolutionSource.Override);
            }

            if (string.IsNullOrEmpty(target.VersionHash))
            {
                Log.Warning("The Roblox version is unknown, so the download servers cannot be tested. Using the default one (Server: {0}).", RobloxCdn.DefaultBaseUri);

                return Commit(RobloxCdn.DefaultBaseUri, [RobloxCdn.DefaultBaseUri], CdnResolutionSource.Fallback);
            }

            (string baseUri, CdnResolutionSource source) = await ProbeFastestAsync(target, generation, cancellationToken).ConfigureAwait(false);

            ranking.PrimaryBaseUri = baseUri;

            RecordResolution(baseUri, source);

            return baseUri;
        }

        private sealed record CdnProbeResult(string BaseUri, TimeSpan Elapsed);
    }
}