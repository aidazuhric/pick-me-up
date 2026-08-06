import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../providers/order_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/formats.dart';

class FleetMapScreen extends StatefulWidget {
  const FleetMapScreen({super.key});

  @override
  State<FleetMapScreen> createState() => _FleetMapScreenState();
}

class _FleetMapScreenState extends State<FleetMapScreen> {
  static const LatLng _fallbackCentre = LatLng(43.3438, 17.8078);
  static const String _tileUrl =
      'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
  static const String _userAgent = 'ba.edu.fit.pickmeup.desktop';

  final _provider = OrderProvider();
  final _mapController = MapController();

  List<Order> _orders = const [];
  Order? _selected;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _mapController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final result = await _provider.get(filter: {
        'Page': 1,
        'PageSize': 100,
        'SortBy': 'CreatedAt desc',
      });

      if (!mounted) return;

      setState(() {
        _orders = (result.items ?? const <Order>[])
            .where((o) => o.pickupLat != null && o.pickupLng != null)
            .toList();
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString();
        _isLoading = false;
      });
      ApiErrorHandler.surface(context, e);
    }
  }

  LatLng get _centre {
    if (_orders.isEmpty) return _fallbackCentre;

    final latitude =
        _orders.map((o) => o.pickupLat!).reduce((a, b) => a + b) / _orders.length;
    final longitude =
        _orders.map((o) => o.pickupLng!).reduce((a, b) => a + b) / _orders.length;

    return LatLng(latitude, longitude);
  }

  Color _statusColour(String? status) => switch (status) {
        'Completed' => AppColors.toggleOn,
        'Cancelled' => AppColors.danger,
        'InProgress' => AppColors.accentCyan,
        'Confirmed' => AppColors.primaryIndigo,
        _ => AppColors.textMuted,
      };

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Fleet map', style: theme.textTheme.headlineSmall),
                  const SizedBox(height: AppSpacing.xs),
                  Text(
                    'Where your rides were picked up, from the coordinates recorded on each order. Live vehicle tracking arrives with the driver app.',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
            OutlinedButton.icon(
              onPressed: _isLoading ? null : _load,
              icon: const Icon(Icons.refresh, size: 16),
              label: const Text('Reload'),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        _Legend(orders: _orders, statusColour: _statusColour),
        const SizedBox(height: AppSpacing.md),
        Expanded(child: _buildMapCard(theme)),
      ],
    );
  }

  Widget _buildMapCard(ThemeData theme) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Stack(
          children: [
            _buildMap(),
            if (_isLoading)
              const Center(
                child: SizedBox(
                  height: 28,
                  width: 28,
                  child: CircularProgressIndicator(strokeWidth: 2.5),
                ),
              ),
            if (!_isLoading && _error == null && _orders.isEmpty)
              const _EmptyBanner(),
            if (_selected != null)
              Positioned(
                left: AppSpacing.md,
                bottom: AppSpacing.md,
                child: _RideCallout(
                  order: _selected!,
                  colour: _statusColour(_selected!.status),
                  onClose: () => setState(() => _selected = null),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildMap() {
    return FlutterMap(
      mapController: _mapController,
      options: MapOptions(
        initialCenter: _centre,
        initialZoom: 13,
        minZoom: 3,
        maxZoom: 18,
        onTap: (_, _) => setState(() => _selected = null),
      ),
      children: [
        TileLayer(
          urlTemplate: _tileUrl,
          userAgentPackageName: _userAgent,
          maxZoom: 19,
        ),
        PolylineLayer(
          polylines: [
            for (final order in _orders)
              if (order.destinationLat != null && order.destinationLng != null)
                Polyline(
                  points: [
                    LatLng(order.pickupLat!, order.pickupLng!),
                    LatLng(order.destinationLat!, order.destinationLng!),
                  ],
                  strokeWidth: 2.5,
                  color: _statusColour(order.status).withValues(alpha: 0.55),
                ),
          ],
        ),
        MarkerLayer(
          markers: [
            for (final order in _orders)
              Marker(
                point: LatLng(order.pickupLat!, order.pickupLng!),
                width: 34,
                height: 34,
                child: _PickupMarker(
                  colour: _statusColour(order.status),
                  isSelected: _selected?.id == order.id,
                  onTap: () => setState(() => _selected = order),
                ),
              ),
          ],
        ),
        const RichAttributionWidget(
          attributions: [
            TextSourceAttribution('OpenStreetMap contributors'),
          ],
        ),
      ],
    );
  }
}

class _PickupMarker extends StatelessWidget {
  const _PickupMarker({
    required this.colour,
    required this.isSelected,
    required this.onTap,
  });

  final Color colour;
  final bool isSelected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: colour,
          shape: BoxShape.circle,
          border: Border.all(
            color: isSelected ? AppColors.textPrimary : Colors.white,
            width: isSelected ? 3 : 2,
          ),
        ),
        child: const Icon(Icons.local_taxi, size: 17, color: Colors.white),
      ),
    );
  }
}

class _Legend extends StatelessWidget {
  const _Legend({required this.orders, required this.statusColour});

  final List<Order> orders;
  final Color Function(String?) statusColour;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final counts = <String, int>{};

    for (final order in orders) {
      final status = order.status ?? '';
      counts[status] = (counts[status] ?? 0) + 1;
    }

    return Wrap(
      spacing: AppSpacing.sm,
      runSpacing: AppSpacing.sm,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        Text(
          orders.length == 1
              ? '1 ride on the map'
              : '${orders.length} rides on the map',
          style: theme.textTheme.titleSmall,
        ),
        for (final entry in counts.entries)
          Container(
            padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.sm,
              vertical: AppSpacing.xs,
            ),
            decoration: BoxDecoration(
              color: statusColour(entry.key).withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
              border: Border.all(
                color: statusColour(entry.key).withValues(alpha: 0.4),
              ),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Container(
                  width: 8,
                  height: 8,
                  decoration: BoxDecoration(
                    color: statusColour(entry.key),
                    shape: BoxShape.circle,
                  ),
                ),
                const SizedBox(width: AppSpacing.xs + 2),
                Text(
                  '${Formats.statusLabel(entry.key)} · ${entry.value}',
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
          ),
      ],
    );
  }
}

class _RideCallout extends StatelessWidget {
  const _RideCallout({
    required this.order,
    required this.colour,
    required this.onClose,
  });

  final Order order;
  final Color colour;
  final VoidCallback onClose;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      width: 320,
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        border: Border.all(color: AppColors.border),
        boxShadow: const [
          BoxShadow(color: Color(0x1A000000), blurRadius: 12, offset: Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 10,
                height: 10,
                decoration: BoxDecoration(color: colour, shape: BoxShape.circle),
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  Formats.statusLabel(order.status),
                  style: theme.textTheme.titleSmall,
                ),
              ),
              IconButton(
                tooltip: 'Close',
                visualDensity: VisualDensity.compact,
                onPressed: onClose,
                icon: const Icon(Icons.close, size: 16),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(order.passengerName ?? '', style: theme.textTheme.bodyMedium),
          const SizedBox(height: AppSpacing.xs),
          Text(order.route, style: theme.textTheme.bodySmall),
          const SizedBox(height: AppSpacing.sm),
          Text(
            [
              order.driverName ?? 'No driver assigned',
              if (order.fare != null) Formats.money(order.fare),
              Formats.day(order.createdAt),
            ].join(' · '),
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}

class _EmptyBanner extends StatelessWidget {
  const _EmptyBanner();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        margin: const EdgeInsets.all(AppSpacing.lg),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          border: Border.all(color: AppColors.border),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.location_off_outlined,
                size: 18, color: AppColors.textMuted),
            const SizedBox(width: AppSpacing.sm),
            Text(
              'No rides with a recorded pick-up location yet.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }
}
