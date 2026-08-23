import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/shift.dart';
import '../../providers/order_provider.dart';
import '../../providers/shift_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/status_pill.dart';

class ActiveRideScreen extends StatefulWidget {
  const ActiveRideScreen({super.key});

  @override
  State<ActiveRideScreen> createState() => _ActiveRideScreenState();
}

class _ActiveRideScreenState extends State<ActiveRideScreen> {
  static const List<String> _activeStatuses = ['Confirmed', 'InProgress'];

  final _orders = OrderProvider();
  final _shifts = ShiftProvider();

  List<Order> _active = const [];
  Shift? _openShift;
  Order? _justCompleted;
  bool _isLoading = true;
  int? _busyOrderId;
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
      final shift = await _shifts.readOpenShift();

      final mine = <Order>[];

      for (final status in _activeStatuses) {
        final result = await _orders.get(
          filter: {
            'Status': status,
            'Page': 1,
            'PageSize': 20,
            'SortBy': 'CreatedAt desc',
          },
        );

        mine.addAll(result.items ?? const <Order>[]);
      }

      mine.sort((a, b) {
        final left = a.createdAt;
        final right = b.createdAt;

        if (left == null || right == null) {
          return 0;
        }
        return right.compareTo(left);
      });

      if (!mounted) return;

      setState(() {
        _openShift = shift;
        _active = mine;
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

  Future<void> _pickup(Order order) async {
    await _transition(
      order,
      () => _orders.pickup(order.id!),
      'Passenger picked up. The ride is under way.',
    );
  }

  Future<void> _complete(Order order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Complete this ride?'),
        content: const Text(
          'The company price list sets the fare and the passenger can pay it.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('NOT YET'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('COMPLETE'),
          ),
        ],
      ),
    );

    if (confirmed != true) {
      return;
    }

    await _transition(
      order,
      () => _orders.complete(order.id!),
      'Ride completed.',
      markCompleted: true,
    );
  }

  Future<void> _transition(
    Order order,
    Future<Order> Function() action,
    String success, {
    bool markCompleted = false,
  }) async {
    setState(() => _busyOrderId = order.id);

    try {
      final updated = await action();

      if (!mounted) return;

      if (markCompleted) {
        setState(() => _justCompleted = updated);
      }

      AppMessages.success(context, success);
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
        setState(() => _busyOrderId = null);
      }
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

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          if (_justCompleted != null) _buildCompletedBanner(_justCompleted!),
          if (_openShift?.isDispatchEligible != true && _active.isNotEmpty)
            _buildShiftWarning(),
          if (_active.isEmpty)
            _buildEmpty()
          else
            for (final order in _active)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.md),
                child: _ActiveRideCard(
                  order: order,
                  isBusy: _busyOrderId == order.id,
                  onPickup: () => _pickup(order),
                  onComplete: () => _complete(order),
                ),
              ),
        ],
      ),
    );
  }

  Widget _buildEmpty() {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(top: AppSpacing.xl),
      child: Column(
        children: [
          Container(
            width: 84,
            height: 84,
            decoration: const BoxDecoration(
              color: AppColors.sectionTint,
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.directions_car_outlined,
              size: 38,
              color: AppColors.mobileHeader,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          Text('No active ride', style: theme.textTheme.headlineSmall),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Accept one from Ride requests and it will show up here.',
            textAlign: TextAlign.center,
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }

  Widget _buildShiftWarning() {
    final theme = Theme.of(context);

    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.toggleOff.withValues(alpha: 0.1),
        border: Border.all(color: AppColors.toggleOff.withValues(alpha: 0.4)),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.warning_amber, color: AppColors.toggleOff, size: 20),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              'You are off shift or on a break. Go back on shift in My shift '
              'before you can pick up or complete a ride.',
              style: theme.textTheme.bodySmall,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCompletedBanner(Order order) {
    final theme = Theme.of(context);

    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.toggleOn.withValues(alpha: 0.1),
        border: Border.all(color: AppColors.toggleOn.withValues(alpha: 0.4)),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.check_circle, color: AppColors.toggleOn, size: 20),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Ride #${order.id} completed',
                  style: theme.textTheme.titleMedium,
                ),
                const SizedBox(height: 2),
                Text(
                  'Fare ${Formats.money(order.fare)} · the passenger can pay '
                  'and review it now.',
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _ActiveRideCard extends StatelessWidget {
  const _ActiveRideCard({
    required this.order,
    required this.isBusy,
    required this.onPickup,
    required this.onComplete,
  });

  final Order order;
  final bool isBusy;
  final VoidCallback onPickup;
  final VoidCallback onComplete;

  static const String _tileUrl =
      'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
  static const String _userAgent = 'ba.fit.pickmeup_mobile';

  bool get _isInProgress => order.status == 'InProgress';

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _buildMap(),
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        order.passengerName ?? '',
                        style: theme.textTheme.titleMedium,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    StatusPill(status: order.status),
                  ],
                ),
                const SizedBox(height: AppSpacing.xs),
                Text(
                  _isInProgress
                      ? 'Ride in progress — heading to the destination.'
                      : 'Confirmed — head to the pick-up point.',
                  style: theme.textTheme.bodySmall,
                ),
                const SizedBox(height: AppSpacing.sm),
                _Line(
                  icon: Icons.my_location,
                  colour: AppColors.toggleOn,
                  text: order.pickupAddress ?? '',
                ),
                if ((order.destinationAddress ?? '').isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.only(top: AppSpacing.xs),
                    child: _Line(
                      icon: Icons.flag,
                      colour: AppColors.danger,
                      text: order.destinationAddress!,
                    ),
                  ),
                const SizedBox(height: AppSpacing.sm),
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        order.distanceKm == null
                            ? 'Distance not recorded'
                            : Formats.distance(order.distanceKm),
                        style: theme.textTheme.bodySmall,
                      ),
                    ),
                    if ((order.carPlate ?? '').isNotEmpty)
                      Text(
                        order.carPlate!,
                        style: theme.textTheme.bodySmall,
                      ),
                  ],
                ),
                const SizedBox(height: AppSpacing.md),
                FilledButton.icon(
                  onPressed: isBusy
                      ? null
                      : (_isInProgress ? onComplete : onPickup),
                  icon: Icon(
                    _isInProgress ? Icons.flag : Icons.person_pin_circle,
                    size: 20,
                  ),
                  label: isBusy
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : Text(_isInProgress ? 'COMPLETE RIDE' : 'PICKED UP'),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMap() {
    final pickup = order.pickupLat != null && order.pickupLng != null
        ? LatLng(order.pickupLat!, order.pickupLng!)
        : null;
    final destination =
        order.destinationLat != null && order.destinationLng != null
        ? LatLng(order.destinationLat!, order.destinationLng!)
        : null;

    if (pickup == null) {
      return const SizedBox.shrink();
    }

    return SizedBox(
      height: 170,
      child: FlutterMap(
        options: MapOptions(
          initialCenter: pickup,
          initialZoom: 14,
          initialCameraFit: destination == null
              ? null
              : CameraFit.bounds(
                  bounds: LatLngBounds(pickup, destination),
                  padding: const EdgeInsets.all(36),
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
      width: 32,
      height: 32,
      child: Container(
        decoration: BoxDecoration(
          color: colour,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white, width: 3),
        ),
        child: Icon(icon, color: Colors.white, size: 15),
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
