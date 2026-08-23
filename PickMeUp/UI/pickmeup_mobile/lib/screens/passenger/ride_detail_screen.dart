import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/payment.dart';
import '../../providers/order_provider.dart';
import '../../providers/payment_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/status_pill.dart';

class RideDetailScreen extends StatefulWidget {
  const RideDetailScreen({super.key, required this.orderId});

  final int orderId;

  static Future<void> open(BuildContext context, int orderId) {
    return Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => RideDetailScreen(orderId: orderId),
      ),
    );
  }

  @override
  State<RideDetailScreen> createState() => _RideDetailScreenState();
}

class _RideDetailScreenState extends State<RideDetailScreen> {
  static const String _tileUrl = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
  static const String _userAgent = 'ba.fit.pickmeup_mobile';

  final _orders = OrderProvider();
  final _payments = PaymentProvider();

  Order? _ride;
  Payment? _payment;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final ride = await _orders.getById(widget.orderId);

      Payment? payment;

      try {
        payment = await _payments.readByOrder(widget.orderId);
      } catch (_) {
        payment = null;
      }

      if (!mounted) return;

      setState(() {
        _ride = ride;
        _payment = payment;
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
                      ride.companyName ?? 'Any available driver',
                      style: theme.textTheme.headlineSmall,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  StatusPill(status: ride.status),
                ],
              ),
              const SizedBox(height: AppSpacing.xs),
              Text(
                Formats.moment(ride.createdAt),
                style: theme.textTheme.bodySmall,
              ),
              const SizedBox(height: AppSpacing.lg),
              _Line(
                icon: Icons.my_location,
                colour: AppColors.toggleOn,
                label: 'Pick-up',
                value: ride.pickupAddress ?? '',
              ),
              if ((ride.destinationAddress ?? '').isNotEmpty) ...[
                const SizedBox(height: AppSpacing.sm),
                _Line(
                  icon: Icons.flag,
                  colour: AppColors.danger,
                  label: 'Destination',
                  value: ride.destinationAddress!,
                ),
              ],
              const Divider(height: AppSpacing.xl),
              Text('Driver and vehicle', style: theme.textTheme.titleMedium),
              const SizedBox(height: AppSpacing.sm),
              _Line(
                icon: Icons.person_outline,
                colour: AppColors.textMuted,
                label: 'Driver',
                value: ride.driverName ?? 'Not assigned yet',
              ),
              const SizedBox(height: AppSpacing.sm),
              _Line(
                icon: Icons.directions_car_outlined,
                colour: AppColors.textMuted,
                label: 'Vehicle',
                value: ride.carPlate ?? 'Not assigned yet',
              ),
              const Divider(height: AppSpacing.xl),
              Text('What it cost', style: theme.textTheme.titleMedium),
              const SizedBox(height: AppSpacing.sm),
              Row(
                children: [
                  Expanded(
                    child: _Stat(
                      label: 'Fare',
                      value: ride.fare == null
                          ? 'Not priced yet'
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
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              _buildPaymentRow(theme),
              if ((ride.cancellationReason ?? '').isNotEmpty) ...[
                const Divider(height: AppSpacing.xl),
                Text('Why it was cancelled', style: theme.textTheme.titleMedium),
                const SizedBox(height: AppSpacing.xs),
                Text(
                  ride.cancellationReason!,
                  style: theme.textTheme.bodySmall,
                ),
              ],
              if (ride.status == 'Completed') ...[
                const SizedBox(height: AppSpacing.lg),
                OutlinedButton.icon(
                  onPressed: () => AppMessages.success(
                    context,
                    'Reviews open in the next release.',
                  ),
                  icon: const Icon(Icons.star_outline, size: 18),
                  label: const Text('LEAVE A REVIEW'),
                ),
              ],
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildPaymentRow(ThemeData theme) {
    final payment = _payment;

    if (payment == null) {
      return Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        decoration: BoxDecoration(
          color: AppColors.sectionTint,
          borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        ),
        child: Row(
          children: [
            const Icon(
              Icons.credit_card_off_outlined,
              size: 18,
              color: AppColors.textMuted,
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Text(
                'No payment has been started for this ride.',
                style: theme.textTheme.bodySmall,
              ),
            ),
          ],
        ),
      );
    }

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: Text('Payment', style: theme.textTheme.titleMedium),
              ),
              StatusPill(status: payment.status),
            ],
          ),
          const SizedBox(height: AppSpacing.xs),
          Row(
            children: [
              Expanded(
                child: Text(
                  payment.paidAt == null
                      ? 'Charged ${Formats.money(payment.amountCharged, payment.currency ?? 'BAM')}'
                      : 'Paid on ${Formats.day(payment.paidAt)}',
                  style: theme.textTheme.bodySmall,
                ),
              ),
              Text(
                Formats.money(payment.amountCharged, payment.currency ?? 'BAM'),
                style: theme.textTheme.titleMedium,
              ),
            ],
          ),
          if ((payment.refundedTotal ?? 0) > 0)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.xs),
              child: Row(
                children: [
                  Expanded(
                    child: Text(
                      'Refunded to you',
                      style: theme.textTheme.bodySmall,
                    ),
                  ),
                  Text(
                    Formats.money(
                      payment.refundedTotal,
                      payment.currency ?? 'BAM',
                    ),
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildMap(Order ride) {
    final pickup = ride.pickupLat != null && ride.pickupLng != null
        ? LatLng(ride.pickupLat!, ride.pickupLng!)
        : null;
    final destination = ride.destinationLat != null && ride.destinationLng != null
        ? LatLng(ride.destinationLat!, ride.destinationLng!)
        : null;

    if (pickup == null) {
      return const SizedBox.shrink();
    }

    final bounds = destination == null
        ? null
        : LatLngBounds(pickup, destination);

    return SizedBox(
      height: 260,
      child: FlutterMap(
        options: MapOptions(
          initialCenter: pickup,
          initialZoom: 14,
          initialCameraFit: bounds == null
              ? null
              : CameraFit.bounds(
                  bounds: bounds,
                  padding: const EdgeInsets.all(48),
                ),
          interactionOptions: const InteractionOptions(
            flags: InteractiveFlag.pinchZoom | InteractiveFlag.drag,
          ),
        ),
        children: [
          TileLayer(
            urlTemplate: _tileUrl,
            userAgentPackageName: _userAgent,
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

class _Line extends StatelessWidget {
  const _Line({
    required this.icon,
    required this.colour,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final Color colour;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 16, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label, style: theme.textTheme.bodySmall),
              Text(
                value,
                style: theme.textTheme.bodyMedium,
                maxLines: 3,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ),
      ],
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
