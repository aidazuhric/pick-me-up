import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../providers/order_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/status_pill.dart';
import '../../widgets/map_attribution.dart';

class DriverRideDetailScreen extends StatefulWidget {
  const DriverRideDetailScreen({super.key, required this.orderId});

  final int orderId;

  static Future<void> open(BuildContext context, int orderId) {
    return Navigator.of(context).push<void>(
      MaterialPageRoute<void>(
        builder: (_) => DriverRideDetailScreen(orderId: orderId),
      ),
    );
  }

  @override
  State<DriverRideDetailScreen> createState() => _DriverRideDetailScreenState();
}

class _DriverRideDetailScreenState extends State<DriverRideDetailScreen> {
  final _orders = OrderProvider();

  Order? _ride;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final ride = await _orders.getById(widget.orderId);

      if (!mounted) return;

      setState(() {
        _ride = ride;
        _isLoading = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_ride == null ? 'Ride' : 'Ride #${_ride!.id}'),
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    final ride = _ride;

    if (_error != null || ride == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error ?? 'This ride could not be loaded.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final theme = Theme.of(context);

    return ListView(
      children: [
        _buildMap(ride),
        Padding(
          padding: const EdgeInsets.all(AppSpacing.screenPadding),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      ride.passengerName ?? '',
                      style: theme.textTheme.headlineSmall,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  StatusPill(status: ride.status),
                ],
              ),
              const SizedBox(height: AppSpacing.xs),
              Text(
                Formats.moment(ride.completedAt ?? ride.createdAt),
                style: theme.textTheme.bodySmall,
              ),
              const SizedBox(height: AppSpacing.lg),
              _Line(
                icon: Icons.my_location,
                colour: AppColors.toggleOn,
                text: ride.pickupAddress ?? '',
              ),
              if ((ride.destinationAddress ?? '').isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: _Line(
                    icon: Icons.flag,
                    colour: AppColors.danger,
                    text: ride.destinationAddress!,
                  ),
                ),
              const Divider(height: AppSpacing.xl),
              Row(
                children: [
                  Expanded(
                    child: _Stat(
                      label: 'Fare',
                      value: ride.fare == null
                          ? 'Not priced'
                          : Formats.money(ride.fare),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: _Stat(
                      label: 'Distance',
                      value: ride.distanceKm == null
                          ? 'Not recorded'
                          : Formats.distance(ride.distanceKm),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: _Stat(label: 'Vehicle', value: ride.carPlate ?? '—'),
                  ),
                ],
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildMap(Order ride) {
    final pickup = ride.pickupLat != null && ride.pickupLng != null
        ? LatLng(ride.pickupLat!, ride.pickupLng!)
        : null;
    final destination =
        ride.destinationLat != null && ride.destinationLng != null
        ? LatLng(ride.destinationLat!, ride.destinationLng!)
        : null;

    if (pickup == null) {
      return const SizedBox.shrink();
    }

    return SizedBox(
      height: (MediaQuery.of(context).size.height * 0.28).clamp(150.0, 280.0),
      child: FlutterMap(
        options: MapOptions(
          initialCenter: pickup,
          initialZoom: 14,
          initialCameraFit: destination == null
              ? null
              : CameraFit.bounds(
                  bounds: LatLngBounds(pickup, destination),
                  padding: const EdgeInsets.all(44),
                ),
          interactionOptions: const InteractionOptions(
            flags: InteractiveFlag.pinchZoom | InteractiveFlag.drag,
          ),
        ),
        children: [
          TileLayer(
            urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
            userAgentPackageName: 'ba.fit.pickmeup_mobile',
            maxZoom: 19,
          ),
          if (destination != null)
            PolylineLayer(
              polylines: [
                Polyline(
                  points: [pickup, destination],
                  strokeWidth: 4,
                  color: AppColors.mobileHeader.withValues(alpha: 0.75),
                ),
              ],
            ),
          MarkerLayer(
            markers: [
              _marker(pickup, AppColors.toggleOn, Icons.my_location),
              if (destination != null)
                _marker(destination, AppColors.danger, Icons.flag),
            ],
          ),
          const MapAttribution(),
        ],
      ),
    );
  }

  Marker _marker(LatLng point, Color colour, IconData icon) {
    return Marker(
      point: point,
      width: 34,
      height: 34,
      child: Container(
        decoration: BoxDecoration(
          color: colour,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white, width: 3),
        ),
        child: Icon(icon, color: Colors.white, size: 16),
      ),
    );
  }
}

class _Stat extends StatelessWidget {
  const _Stat({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: theme.textTheme.bodySmall),
          const SizedBox(height: 2),
          FittedBox(
            child: Text(value, style: theme.textTheme.titleMedium, maxLines: 1),
          ),
        ],
      ),
    );
  }
}

class _Line extends StatelessWidget {
  const _Line({required this.icon, required this.colour, required this.text});

  final IconData icon;
  final Color colour;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 16, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(
            text,
            style: Theme.of(context).textTheme.bodyMedium,
            maxLines: 3,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}
