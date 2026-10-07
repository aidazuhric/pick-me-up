import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/notification_item.dart';
import '../../models/order.dart';
import '../../providers/notification_store.dart';
import '../../providers/order_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/status_pill.dart';
import 'cancel_ride_dialog.dart';
import 'pay_ride_screen.dart';
import 'ride_detail_screen.dart';
import '../../widgets/map_attribution.dart';

class RideStatusScreen extends StatefulWidget {
  const RideStatusScreen({super.key});

  @override
  State<RideStatusScreen> createState() => _RideStatusScreenState();
}

class _RideStatusScreenState extends State<RideStatusScreen> {
  static const List<String> _activeStatuses = [
    'Pending',
    'Confirmed',
    'InProgress',
  ];

  final _orders = OrderProvider();

  StreamSubscription<NotificationItem>? _pushes;

  Order? _ride;
  int? _trackedId;
  int _otherActive = 0;
  bool _isLoading = true;
  bool _isCancelling = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _pushes = context.read<NotificationStore>().pushes.listen(_onPushed);
    });
  }

  @override
  void dispose() {
    _pushes?.cancel();
    super.dispose();
  }

  void _onPushed(NotificationItem pushed) {
    if (!mounted) {
      return;
    }

    _load(focusOrderId: pushed.orderId);
  }

  Future<Order?> _readOrder(int orderId) async {
    try {
      return await _orders.getById(orderId);
    } catch (_) {
      return null;
    }
  }

  Future<void> _load({int? focusOrderId}) async {
    if (mounted) {
      setState(() => _error = null);
    }

    try {
      final active = <Order>[];

      for (final status in _activeStatuses) {
        final result = await _orders.get(
          filter: {
            'Status': status,
            'Page': 1,
            'PageSize': 20,
            'SortBy': 'CreatedAt desc',
          },
        );

        active.addAll(result.items ?? const <Order>[]);
      }

      active.sort((a, b) {
        final left = a.createdAt;
        final right = b.createdAt;

        if (left == null || right == null) {
          return 0;
        }
        return right.compareTo(left);
      });

      final focus = focusOrderId ?? _trackedId;

      Order? tracked = focus == null
          ? null
          : active.where((o) => o.id == focus).firstOrNull;

      if (tracked == null && focusOrderId != null) {
        tracked = await _readOrder(focusOrderId);
      }

      if (tracked == null && _trackedId != null) {
        final kept = await _readOrder(_trackedId!);

        if (kept != null &&
            (active.isEmpty || (kept.id ?? 0) > (active.first.id ?? 0))) {
          tracked = kept;
        }
      }

      tracked ??= active.isEmpty ? null : active.first;

      if (tracked == null) {
        final recent = await _orders.get(
          filter: {'Page': 1, 'PageSize': 1, 'SortBy': 'CreatedAt desc'},
        );

        final last = (recent.items ?? const <Order>[]).firstOrNull;

        if (last?.status == 'Completed' || last?.status == 'Cancelled') {
          tracked = last;
        }
      }

      if (!mounted) return;

      setState(() {
        _ride = tracked;
        _trackedId = tracked?.id;
        _otherActive = active.where((o) => o.id != tracked?.id).length;
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

  Future<void> _cancelRide(Order ride) async {
    final reason = await CancelRideDialog.show(context);

    if (reason == null || !mounted) {
      return;
    }

    setState(() => _isCancelling = true);

    try {
      await _orders.cancel(ride.id!, reason);

      if (!mounted) return;

      AppMessages.success(
        context,
        _refundOnCancel(ride)
            ? 'Ride #${ride.id} is cancelled and your payment is on its way back.'
            : 'Ride #${ride.id} is cancelled.',
      );
      await _load();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      AppMessages.failure(context, e.message);
      await _load();
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _isCancelling = false);
      }
    }
  }

  Future<void> _payNow() async {
    await Navigator.of(context).push<void>(
      MaterialPageRoute<void>(
        builder: (_) => Scaffold(
          appBar: AppBar(title: const Text('Pay for a ride')),
          body: const PayRideScreen(),
        ),
      ),
    );

    if (mounted) {
      await _load();
    }
  }

  Future<void> _openDetail(int orderId) async {
    await RideDetailScreen.open(context, orderId);

    if (mounted) {
      await _load();
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final ride = _ride;

    if (ride == null) {
      return _buildEmpty();
    }

    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: () => _load(),
      child: ListView(
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
                        'Ride #${ride.id}',
                        style: theme.textTheme.headlineSmall,
                      ),
                    ),
                    StatusPill(status: ride.status),
                  ],
                ),
                const SizedBox(height: AppSpacing.xs),
                Text(_headline(ride), style: theme.textTheme.bodyMedium),
                const SizedBox(height: AppSpacing.lg),
                if (ride.status == 'Cancelled')
                  _buildCancelled(theme, ride)
                else
                  _buildTimeline(ride),
                if (ride.driverName != null) ...[
                  const SizedBox(height: AppSpacing.lg),
                  _buildDriverCard(theme, ride),
                ],
                const SizedBox(height: AppSpacing.lg),
                _buildRoute(theme, ride),
                if (ride.status == 'Completed') ...[
                  const SizedBox(height: AppSpacing.lg),
                  _buildCompletedActions(theme, ride),
                ],
                if (_isCancellable(ride)) ...[
                  const SizedBox(height: AppSpacing.lg),
                  OutlinedButton.icon(
                    onPressed: _isCancelling ? null : () => _cancelRide(ride),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.danger,
                      side: const BorderSide(color: AppColors.danger),
                    ),
                    icon: const Icon(Icons.cancel_outlined, size: 18),
                    label: Text(
                      _isCancelling
                          ? 'CANCELLING…'
                          : _refundOnCancel(ride)
                          ? 'CANCEL & REFUND'
                          : 'CANCEL RIDE',
                    ),
                  ),
                  if (_refundOnCancel(ride))
                    Padding(
                      padding: const EdgeInsets.only(top: AppSpacing.xs),
                      child: Text(
                        'No driver has taken this ride yet, so cancelling returns '
                        '${ride.fare == null ? 'what you paid' : Formats.money(ride.fare!)} to your card.',
                        style: theme.textTheme.bodySmall,
                      ),
                    ),
                ],
                if (_otherActive > 0) ...[
                  const SizedBox(height: AppSpacing.lg),
                  Text(
                    _otherActive == 1
                        ? 'You have 1 other ride under way. Open Ride history to see it.'
                        : 'You have $_otherActive other rides under way. Open Ride history to see them.',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  static bool _refundOnCancel(Order ride) =>
      ride.isPrepaid == true && ride.status == 'Pending';

  static bool _isCancellable(Order ride) =>
      ride.id != null &&
      const ['Pending', 'Confirmed', 'InProgress'].contains(ride.status);

  String _headline(Order ride) => switch (ride.status) {
    'Pending' when ride.isPrepaid != true =>
      'Not paid yet — no driver sees this ride until it is paid.',
    'Pending' =>
      ride.isGeneralOrder == true
          ? 'Paid — waiting for a driver to accept.'
          : 'Paid — waiting for a driver from ${ride.companyName ?? 'the company'} to accept.',
    'Confirmed' => 'Your driver is on the way.',
    'InProgress' => 'You have been picked up — heading to your destination.',
    'Completed' => 'Ride completed. Thanks for riding with us.',
    'Cancelled' => 'This ride was cancelled.',
    _ => '',
  };

  Widget _buildEmpty() {
    final theme = Theme.of(context);

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 84,
              height: 84,
              decoration: const BoxDecoration(
                color: AppColors.sectionTint,
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.local_taxi_outlined,
                size: 38,
                color: AppColors.mobileHeader,
              ),
            ),
            const SizedBox(height: AppSpacing.lg),
            Text('No active ride', style: theme.textTheme.headlineSmall),
            const SizedBox(height: AppSpacing.sm),
            Text(
              'Order a taxi from Home and you can follow it here.',
              textAlign: TextAlign.center,
              style: theme.textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTimeline(Order ride) {
    const steps = ['Pending', 'Confirmed', 'InProgress', 'Completed'];
    const labels = ['Requested', 'Driver accepted', 'Picked up', 'Completed'];

    final current = steps.indexOf(ride.status ?? '');
    final isPaid = ride.isPrepaid == true;

    return Column(
      children: [
        _TimelineStep(
          label: isPaid ? 'Paid' : 'Awaiting payment',
          moment: Formats.moment(ride.prepaidAt),
          isDone: isPaid,
          isCurrent: !isPaid,
          isLast: false,
        ),
        for (var i = 0; i < steps.length; i++)
          _TimelineStep(
            label: labels[i],
            moment: _momentFor(ride, steps[i]),
            isDone: current >= i,
            isCurrent: current == i,
            isLast: i == steps.length - 1,
          ),
      ],
    );
  }

  String _momentFor(Order ride, String step) => switch (step) {
    'Pending' => Formats.moment(ride.createdAt),
    'Confirmed' => Formats.moment(ride.acceptedAt),
    'Completed' => Formats.moment(ride.completedAt),
    _ => '',
  };

  Widget _buildCancelled(ThemeData theme, Order ride) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.danger.withValues(alpha: 0.1),
        border: Border.all(color: AppColors.danger.withValues(alpha: 0.4)),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.cancel_outlined, color: AppColors.danger, size: 20),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Cancelled', style: theme.textTheme.titleMedium),
                if ((ride.cancellationReason ?? '').isNotEmpty) ...[
                  const SizedBox(height: 2),
                  Text(
                    ride.cancellationReason!,
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDriverCard(ThemeData theme, Order ride) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        children: [
          const CircleAvatar(
            radius: 22,
            backgroundColor: AppColors.mobileHeader,
            child: Icon(Icons.person, color: Colors.white, size: 24),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  ride.driverName ?? '',
                  style: theme.textTheme.titleMedium,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  ride.companyName ?? '',
                  style: theme.textTheme.bodySmall,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
          if ((ride.carPlate ?? '').isNotEmpty)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              decoration: BoxDecoration(
                color: AppColors.surface,
                border: Border.all(color: AppColors.border),
                borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
              ),
              child: Text(ride.carPlate!, style: theme.textTheme.titleSmall),
            ),
        ],
      ),
    );
  }

  Widget _buildRoute(ThemeData theme, Order ride) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _Line(
          icon: Icons.my_location,
          colour: AppColors.toggleOn,
          text: ride.pickupAddress ?? '',
        ),
        if ((ride.destinationAddress ?? '').isNotEmpty)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.xs),
            child: _Line(
              icon: Icons.flag,
              colour: AppColors.danger,
              text: ride.destinationAddress!,
            ),
          ),
        if (ride.distanceKm != null)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.xs),
            child: _Line(
              icon: Icons.straighten,
              colour: AppColors.textMuted,
              text: Formats.distance(ride.distanceKm),
            ),
          ),
      ],
    );
  }

  Widget _buildCompletedActions(ThemeData theme, Order ride) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            color: AppColors.toggleOn.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          ),
          child: Row(
            children: [
              Expanded(
                child: Text('What you owe', style: theme.textTheme.bodyMedium),
              ),
              Text(
                ride.fare == null ? 'Not priced yet' : Formats.money(ride.fare),
                style: theme.textTheme.headlineSmall,
              ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        FilledButton.icon(
          onPressed: _payNow,
          icon: const Icon(Icons.credit_card, size: 20),
          label: const Text('PAY NOW'),
        ),
        const SizedBox(height: AppSpacing.sm),
        OutlinedButton.icon(
          onPressed: ride.id == null ? null : () => _openDetail(ride.id!),
          icon: const Icon(Icons.star_outline, size: 18),
          label: const Text('LEAVE A REVIEW'),
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

    return GestureDetector(
      onTap: ride.id == null ? null : () => _openDetail(ride.id!),
      child: SizedBox(
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
            flags: InteractiveFlag.none,
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

class _TimelineStep extends StatelessWidget {
  const _TimelineStep({
    required this.label,
    required this.moment,
    required this.isDone,
    required this.isCurrent,
    required this.isLast,
  });

  final String label;
  final String moment;
  final bool isDone;
  final bool isCurrent;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colour = isDone ? AppColors.toggleOn : AppColors.divider;

    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Column(
            children: [
              Container(
                width: 22,
                height: 22,
                decoration: BoxDecoration(
                  color: isDone ? colour : AppColors.surface,
                  border: Border.all(color: colour, width: 2),
                  shape: BoxShape.circle,
                ),
                child: isDone
                    ? const Icon(Icons.check, size: 13, color: Colors.white)
                    : null,
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    width: 2,
                    color: colour,
                    margin: const EdgeInsets.symmetric(vertical: 2),
                  ),
                ),
            ],
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Padding(
              padding: EdgeInsets.only(bottom: isLast ? 0 : AppSpacing.md),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      fontWeight: isCurrent ? FontWeight.w700 : FontWeight.w400,
                      color: isDone
                          ? AppColors.textPrimary
                          : AppColors.textMuted,
                    ),
                  ),
                  if (moment.isNotEmpty)
                    Text(moment, style: theme.textTheme.bodySmall),
                ],
              ),
            ),
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
        Icon(icon, size: 15, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(
            text,
            style: Theme.of(context).textTheme.bodySmall,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}
