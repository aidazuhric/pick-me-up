import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../utils/formats.dart';

class OrderDetail extends StatelessWidget {
  const OrderDetail({super.key, required this.order});

  final Order order;

  static Future<void> show(BuildContext context, {required Order order}) {
    return showDialog<void>(
      context: context,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 780,
            height: 600,
            child: OrderDetail(order: order),
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final history = order.statusHistory ?? const <OrderStatusHistoryEntry>[];

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
                  Text('Ride for ${order.passengerName}',
                      style: theme.textTheme.headlineSmall),
                  const SizedBox(height: 2),
                  Text(
                    'Placed ${Formats.moment(order.createdAt)}',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
            _StatusPill(status: order.status ?? ''),
            const SizedBox(width: AppSpacing.sm),
            IconButton(
              tooltip: 'Close',
              onPressed: () => Navigator.of(context).pop(),
              icon: const Icon(Icons.close, size: 20),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        const Divider(height: 1),
        Expanded(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(vertical: AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _Row(label: 'Route', value: order.route),
                _Row(label: 'Driver', value: order.driverName ?? 'Not assigned'),
                _Row(label: 'Vehicle', value: order.carPlate ?? 'Not assigned'),
                _Row(
                  label: 'Distance',
                  value: order.distanceKm == null
                      ? ''
                      : '${order.distanceKm!.toStringAsFixed(2)} km',
                ),
                _Row(label: 'Fare', value: Formats.money(order.fare)),
                if (order.cancellationReason != null)
                  _Row(
                    label: 'Cancellation reason',
                    value: order.cancellationReason!,
                  ),
                const SizedBox(height: AppSpacing.md),
                Text('Status history', style: theme.textTheme.titleSmall),
                const SizedBox(height: AppSpacing.sm),
                if (history.isEmpty)
                  Text('No transitions recorded.',
                      style: theme.textTheme.bodySmall),
                for (final entry in history)
                  Padding(
                    padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                    child: _HistoryRow(entry: entry),
                  ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: AppSpacing.formLabelWidth,
            child: Text(label, style: theme.textTheme.bodySmall),
          ),
          Expanded(child: Text(value, style: theme.textTheme.bodyMedium)),
        ],
      ),
    );
  }
}

class _HistoryRow extends StatelessWidget {
  const _HistoryRow({required this.entry});

  final OrderStatusHistoryEntry entry;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final from = entry.fromStatus == null
        ? 'Placed'
        : '${Formats.statusLabel(entry.fromStatus)} → ${Formats.statusLabel(entry.toStatus)}';

    return Container(
      padding: const EdgeInsets.all(AppSpacing.sm + AppSpacing.xs),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 210,
            child: Text(from, style: theme.textTheme.titleSmall),
          ),
          SizedBox(
            width: 150,
            child: Text(Formats.moment(entry.changedAt),
                style: theme.textTheme.bodySmall),
          ),
          SizedBox(
            width: 150,
            child:
                Text(entry.changedBy ?? '', style: theme.textTheme.bodySmall),
          ),
          Expanded(
            child: Text(entry.reason ?? '', style: theme.textTheme.bodySmall),
          ),
        ],
      ),
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final color = switch (status) {
      'Completed' => AppColors.toggleOn,
      'Cancelled' => AppColors.danger,
      'InProgress' => AppColors.accentCyan,
      'Confirmed' => AppColors.primaryIndigo,
      _ => AppColors.textMuted,
    };

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm + AppSpacing.xs,
        vertical: AppSpacing.xs + 2,
      ),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Text(
        Formats.statusLabel(status),
        style:
            TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: color),
      ),
    );
  }
}
