import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../utils/formats.dart';

class StatusPill extends StatelessWidget {
  const StatusPill({super.key, required this.status});

  final String? status;

  static Color colourFor(String? status) => switch (status) {
    'Completed' || 'Paid' => AppColors.toggleOn,
    'Cancelled' || 'Failed' => AppColors.danger,
    'Refunded' || 'PartiallyRefunded' => AppColors.accentCyan,
    'InProgress' || 'Confirmed' => AppColors.mobileHeader,
    _ => AppColors.textMuted,
  };

  static IconData iconFor(String? status) => switch (status) {
    'Completed' || 'Paid' => Icons.check_circle,
    'Cancelled' || 'Failed' => Icons.cancel_outlined,
    'Refunded' || 'PartiallyRefunded' => Icons.undo,
    'InProgress' => Icons.navigation_outlined,
    'Confirmed' => Icons.directions_car_outlined,
    _ => Icons.schedule,
  };

  @override
  Widget build(BuildContext context) {
    final colour = colourFor(status);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: colour.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(iconFor(status), size: 14, color: colour),
          const SizedBox(width: 6),
          Text(
            Formats.statusLabel(status),
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: colour,
            ),
          ),
        ],
      ),
    );
  }
}
