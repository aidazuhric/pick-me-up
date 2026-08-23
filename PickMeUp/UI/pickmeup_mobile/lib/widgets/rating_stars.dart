import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

class RatingStars extends StatelessWidget {
  const RatingStars({
    super.key,
    required this.average,
    required this.reviewCount,
    this.size = 15,
    this.showLabel = true,
  });

  final double? average;
  final int? reviewCount;
  final double size;
  final bool showLabel;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final count = reviewCount ?? 0;

    if (average == null || count == 0) {
      return Text('No reviews yet', style: theme.textTheme.bodySmall);
    }

    final value = average!;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        for (var i = 1; i <= 5; i++)
          Icon(
            value >= i
                ? Icons.star
                : (value >= i - 0.5 ? Icons.star_half : Icons.star_border),
            size: size,
            color: AppColors.accentCyan,
          ),
        if (showLabel) ...[
          const SizedBox(width: 6),
          Text(
            '${value.toStringAsFixed(1)} · ${count == 1 ? '1 review' : '$count reviews'}',
            style: theme.textTheme.bodySmall,
          ),
        ],
      ],
    );
  }
}
