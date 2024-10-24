﻿using Robust.Client.Graphics;
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Enums;

namespace Content.Client._SSS.RadarOverlay;

public sealed class RadarOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly RadarOverlaySystem _suspicionRadarOverlaySystem;
    private readonly TransformSystem _transformSystem;


    private const float MinSize = 10f;
    private const float MaxSize = 35f;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public RadarOverlay()
    {
        IoCManager.InjectDependencies(this);

        _suspicionRadarOverlaySystem = _entityManager.System<RadarOverlaySystem>();
        _transformSystem = _entityManager.System<TransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (_player.LocalEntity is not { } localEntity)
            return;

        if (args.ViewportControl == null)
            return;

        var localPlayerPosition = _transformSystem.GetWorldPosition(localEntity);
        var bounds = args.ViewportBounds;

        foreach (var radarInfo in _suspicionRadarOverlaySystem.RadarInfos)
        {
            var distance = Vector2.Distance(radarInfo.Position, localPlayerPosition);
            var screenPosition = args.ViewportControl.WorldToScreen(radarInfo.Position);

            // Size of the radar blip is based on the distance from the player. The closer the player is, the smaller the blip.
            var radius = Math.Clamp((int)(MaxSize - distance), MinSize, MaxSize);

            // We clamp the radar blips to the screen bounds so you always see them.

            if (screenPosition.X > bounds.Right)
            {
                screenPosition.X = bounds.Right;
            }
            else if (screenPosition.X < bounds.Left)
            {
                screenPosition.X = bounds.Left;
            }

            if (screenPosition.Y > bounds.Bottom)
            {
                screenPosition.Y = bounds.Bottom;
            }
            else if (screenPosition.Y < bounds.Top)
            {
                screenPosition.Y = bounds.Top;
            }

            handle.DrawCircle(screenPosition, radius, radarInfo.Color, false);
        }
    }
}
