using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// UI-thread ownership only. Snapshots contain immutable stops and never expose a writable array.
internal sealed class AmbientGlowGradientObserver(Action changed) : IDisposable
{
    private GradientStops? _source;
    private readonly HashSet<GradientStop> _observedStops = [];
    internal IReadOnlyList<ImmutableGradientStop>? Snapshot { get; private set; }

    internal void SetSource(GradientStops? source)
    {
        if (ReferenceEquals(_source, source)) return;
        Dispose();
        _source = source;
        if (_source is not null) _source.CollectionChanged += OnCollectionChanged;
        ObserveStops();
        Snapshot = CreateSnapshot(source);
    }

    private void ObserveStops()
    {
        foreach (var stop in _observedStops) stop.PropertyChanged -= OnStopChanged;
        _observedStops.Clear();
        if (_source is null) return;
        foreach (var stop in _source)
            if (_observedStops.Add(stop)) stop.PropertyChanged += OnStopChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ObserveStops();
        Publish();
    }

    private void OnStopChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == GradientStop.ColorProperty || e.Property == GradientStop.OffsetProperty) Publish();
    }

    private void Publish()
    {
        Snapshot = CreateSnapshot(_source);
        changed();
    }

    internal static IReadOnlyList<ImmutableGradientStop>? CreateSnapshot(GradientStops? source)
    {
        if (source is null || source.Count == 0) return null;
        var stops = new List<(int Index, ImmutableGradientStop Stop)>(source.Count);
        for (var i = 0; i < source.Count; i++)
        {
            var stop = source[i];
            if (double.IsFinite(stop.Offset))
                stops.Add((i, new ImmutableGradientStop(Math.Clamp(stop.Offset, 0, 1), stop.Color)));
        }
        if (stops.Count == 0) return null;
        stops.Sort(static (a, b) =>
        {
            var comparison = a.Stop.Offset.CompareTo(b.Stop.Offset);
            return comparison != 0 ? comparison : a.Index.CompareTo(b.Index);
        });
        var snapshot = new ImmutableGradientStop[stops.Count];
        for (var i = 0; i < stops.Count; i++) snapshot[i] = stops[i].Stop;
        return Array.AsReadOnly(snapshot);
    }

    public void Dispose()
    {
        if (_source is not null) _source.CollectionChanged -= OnCollectionChanged;
        foreach (var stop in _observedStops) stop.PropertyChanged -= OnStopChanged;
        _observedStops.Clear();
        _source = null;
        Snapshot = null;
    }
}
