import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../models/notification_item.dart';
import '../models/order.dart';
import '../providers/notification_store.dart';
import '../providers/order_provider.dart';
import '../screens/driver/active_ride_screen.dart';
import '../utils/api_client_exception.dart';
import '../utils/formats.dart';
import 'app_messages.dart';

class DriverRequestOverlay extends StatefulWidget {
  const DriverRequestOverlay({super.key});

  static const String availableCategory = 'order-available';
  static const int queueLimit = 3;

  @override
  State<DriverRequestOverlay> createState() => _DriverRequestOverlayState();
}

class _DriverRequestOverlayState extends State<DriverRequestOverlay> {
  final _orders = OrderProvider();

  StreamSubscription<NotificationItem>? _subscription;
  List<Order> _queue = const [];
  bool _isBusy = false;

  @override
  void initState() {
    super.initState();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;

      _subscription = context
          .read<NotificationStore>()
          .pushes
          .where((n) => n.category == DriverRequestOverlay.availableCategory)
          .listen(_onAvailablePush);
    });
  }

  @override
  void dispose() {
    _subscription?.cancel();
    super.dispose();
  }

  Future<void> _onAvailablePush(NotificationItem pushed) async {
    final orderId = pushed.orderId;

    if (orderId == null || !mounted) {
      return;
    }

    if (_queue.any((o) => o.id == orderId)) {
      return;
    }

    final offered = await _readOfferedRide(orderId);

    if (offered == null || !mounted) {
      return;
    }

    setState(() {
      _queue = [
        offered,
        ..._queue.where((o) => o.id != orderId),
      ].take(DriverRequestOverlay.queueLimit).toList();
    });
  }

  Future<Order?> _readOfferedRide(int orderId) async {
    try {
      final result = await _orders.get(
        filter: {
          'AvailableForMe': true,
          'Page': 1,
          'PageSize': 20,
          'SortBy': 'CreatedAt desc',
        },
      );

      for (final order in result.items ?? const <Order>[]) {
        if (order.id == orderId) {
          return order;
        }
      }
    } catch (_) {
      return null;
    }

    return null;
  }

  void _drop(int orderId) {
    setState(() => _queue = _queue.where((o) => o.id != orderId).toList());
  }

  Future<void> _accept(Order order) async {
    setState(() => _isBusy = true);

    try {
      final accepted = await _orders.accept(order.id!);

      if (!mounted) return;

      _drop(order.id!);
      AppMessages.success(
        context,
        'Ride #${accepted.id} is yours. The passenger has been told.',
      );

      await Navigator.of(context).push(
        MaterialPageRoute<void>(
          builder: (_) => Scaffold(
            appBar: AppBar(title: const Text('Active ride')),
            body: const ActiveRideScreen(),
          ),
        ),
      );
    } on ApiClientException catch (e) {
      if (!mounted) return;
      _drop(order.id!);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  Future<void> _decline(Order order) async {
    setState(() => _isBusy = true);

    try {
      await _orders.reject(order.id!);

      if (!mounted) return;

      _drop(order.id!);
      AppMessages.success(context, 'Declined. It goes back to other drivers.');
    } on ApiClientException catch (e) {
      if (!mounted) return;
      _drop(order.id!);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_queue.isEmpty) {
      return const SizedBox.shrink();
    }

    final newest = _queue.first;
    final waiting = _queue.length - 1;

    return Positioned(
      top: 0,
      left: 0,
      right: 0,
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.sm),
          child: Material(
            elevation: 8,
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
            color: AppColors.surface,
            child: _RequestBanner(
              order: newest,
              waiting: waiting,
              isBusy: _isBusy,
              onAccept: () => _accept(newest),
              onDecline: () => _decline(newest),
            ),
          ),
        ),
      ),
    );
  }
}

class _RequestBanner extends StatelessWidget {
  const _RequestBanner({
    required this.order,
    required this.waiting,
    required this.isBusy,
    required this.onAccept,
    required this.onDecline,
  });

  final Order order;
  final int waiting;
  final bool isBusy;
  final VoidCallback onAccept;
  final VoidCallback onDecline;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        border: Border.all(color: AppColors.mobileHeader, width: 1.5),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisSize: MainAxisSize.min,
        children: [
          Row(
            children: [
              const Icon(
                Icons.notifications_active,
                size: 18,
                color: AppColors.mobileHeader,
              ),
              const SizedBox(width: AppSpacing.xs),
              Expanded(
                child: Text(
                  'New ride request',
                  style: theme.textTheme.titleSmall?.copyWith(
                    color: AppColors.mobileHeader,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
              if (waiting > 0)
                Text('+$waiting waiting', style: theme.textTheme.bodySmall),
            ],
          ),
          const SizedBox(height: AppSpacing.xs),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      order.passengerName ?? '',
                      style: theme.textTheme.bodyMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    Text(
                      order.distanceKm == null
                          ? 'Distance not recorded'
                          : Formats.distance(order.distanceKm),
                      style: theme.textTheme.bodySmall,
                    ),
                  ],
                ),
              ),
              Text(
                order.fare == null ? '—' : Formats.money(order.fare!),
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w700,
                  color: AppColors.mobileHeader,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.xs),
          _BannerLine(
            icon: Icons.my_location,
            colour: AppColors.toggleOn,
            text: order.pickupAddress ?? '',
          ),
          _BannerLine(
            icon: Icons.flag,
            colour: AppColors.danger,
            text: (order.destinationAddress ?? '').isEmpty
                ? 'No destination given'
                : order.destinationAddress!,
          ),
          const SizedBox(height: AppSpacing.sm),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: isBusy ? null : onDecline,
                  child: const Text('DECLINE'),
                ),
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                flex: 2,
                child: FilledButton(
                  onPressed: isBusy ? null : onAccept,
                  child: isBusy
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Text('ACCEPT'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _BannerLine extends StatelessWidget {
  const _BannerLine({
    required this.icon,
    required this.colour,
    required this.text,
  });

  final IconData icon;
  final Color colour;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 2),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 14, color: colour),
          const SizedBox(width: AppSpacing.xs),
          Expanded(
            child: Text(
              text,
              style: Theme.of(context).textTheme.bodySmall,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }
}
